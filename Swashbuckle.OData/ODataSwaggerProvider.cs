using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Swashbuckle.Application;
using Swashbuckle.OData.Descriptions;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public class ODataSwaggerProvider : ISwaggerProvider
    {
        private readonly ISwaggerProvider _defaultProvider;
        private readonly HttpConfiguration _httpConfig;
        private readonly ODataSwaggerProviderOptions _options;
        private readonly IDictionary<string, Info> _apiVersions;
        private readonly IApiExplorer _odataApiExplorer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        /// </summary>
        /// <param name="defaultProvider">The default provider.</param>
        /// <param name="swaggerDocsConfig">The swagger docs configuration.</param>
        public ODataSwaggerProvider(ISwaggerProvider defaultProvider, SwaggerDocsConfig swaggerDocsConfig) 
            : this(defaultProvider, swaggerDocsConfig, GlobalConfiguration.Configuration)
        {
            Contract.Requires(defaultProvider != null);
            Contract.Requires(swaggerDocsConfig != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        /// Use this constructor for self-hosted scenarios.
        /// </summary>
        /// <param name="defaultProvider">The default provider.</param>
        /// <param name="swaggerDocsConfig">The swagger docs configuration.</param>
        /// <param name="httpConfig">The HttpConfiguration that contains the OData Edm Model.</param>
        public ODataSwaggerProvider(ISwaggerProvider defaultProvider, SwaggerDocsConfig swaggerDocsConfig, HttpConfiguration httpConfig)
            : this(defaultProvider, DefaultCompositionRoot.GetSwaggerProviderOptions(swaggerDocsConfig), DefaultCompositionRoot.GetApiVersions(swaggerDocsConfig), DefaultCompositionRoot.GetApiExplorer(httpConfig), httpConfig)
        {
            Contract.Requires(defaultProvider != null);
            Contract.Requires(swaggerDocsConfig != null);
            Contract.Requires(httpConfig != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        /// Use this constructor to customize all <see cref="ODataSwaggerProvider" /> dependencies.
        /// </summary>
        /// <param name="defaultProvider">The default provider.</param>
        /// <param name="options">The options.</param>
        /// <param name="apiVersions">The version information.</param>
        /// <param name="odataApiExplorer">The API explorer.</param>
        /// <param name="httpConfig">The HttpConfiguration that contains the OData Edm Model.</param>
        internal ODataSwaggerProvider(ISwaggerProvider defaultProvider, SwaggerProviderOptions options, IDictionary<string, Info> apiVersions, IApiExplorer odataApiExplorer, HttpConfiguration httpConfig)
        {
            Contract.Requires(defaultProvider != null);
            Contract.Requires(odataApiExplorer != null);
            Contract.Requires(httpConfig != null);
            Contract.Requires(options != null);

            _defaultProvider = defaultProvider;
            _httpConfig = httpConfig;
            _options = new ODataSwaggerProviderOptions(options);
            _apiVersions = apiVersions;
            _odataApiExplorer = odataApiExplorer;
        }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            var schemaRegistry = new SchemaRegistry(
                _httpConfig.SerializerSettingsOrDefault(),
                _options.CustomSchemaMappings,
                _options.SchemaFilters,
                _options.ModelFilters,
                _options.IgnoreObsoleteProperties,
                _options.SchemaIdSelector,
                _options.DescribeAllEnumsAsStrings,
                _options.DescribeStringEnumsInCamelCase);

            Info info;
            _apiVersions.TryGetValue(apiVersion, out info);
            if (info == null)
                throw new UnknownApiVersion(apiVersion);

            var paths = GetApiDescriptionsFor(apiVersion)
                .Where(apiDesc => !(_options.IgnoreObsoleteActions && apiDesc.IsObsolete()))
                .OrderBy(_options.GroupingKeySelector, _options.GroupingKeyComparer)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(@group, schemaRegistry));

            var rootUri = new Uri(rootUrl);
            var port = !rootUri.IsDefaultPort ? ":" + rootUri.Port : string.Empty;

            var odataSwaggerDoc = new SwaggerDocument
            {
                info = info,
                host = rootUri.Host + port,
                basePath = rootUri.AbsolutePath != "/" ? rootUri.AbsolutePath : null,
                schemes = _options.Schemes?.ToList() ?? new[] { rootUri.Scheme }.ToList(),
                paths = paths,
                definitions = schemaRegistry.Definitions,
                securityDefinitions = _options.SecurityDefinitions
            };

            foreach(var filter in _options.DocumentFilters)
            {
                Contract.Assume(filter != null);

                filter.Apply(odataSwaggerDoc, schemaRegistry, _odataApiExplorer);
            }

            return MergeODataAndWebApiSwaggerDocs(rootUrl, apiVersion, odataSwaggerDoc);
        }

        private SwaggerDocument MergeODataAndWebApiSwaggerDocs(string rootUrl, string apiVersion, SwaggerDocument odataSwaggerDoc)
        {
            Contract.Requires(odataSwaggerDoc != null);

            var webApiSwaggerDoc = _defaultProvider.GetSwagger(rootUrl, apiVersion);

            Contract.Assume(webApiSwaggerDoc != null);

            webApiSwaggerDoc.paths = webApiSwaggerDoc.paths.UnionEvenIfNull(odataSwaggerDoc.paths).ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => group.First());
            webApiSwaggerDoc.definitions = webApiSwaggerDoc.definitions.UnionEvenIfNull(odataSwaggerDoc.definitions).ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => group.First());
            webApiSwaggerDoc.parameters = webApiSwaggerDoc.parameters.UnionEvenIfNull(odataSwaggerDoc.parameters).ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => group.First());
            webApiSwaggerDoc.responses = webApiSwaggerDoc.responses.UnionEvenIfNull(odataSwaggerDoc.responses).ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => group.First());
            webApiSwaggerDoc.securityDefinitions = webApiSwaggerDoc.securityDefinitions.UnionEvenIfNull(odataSwaggerDoc.securityDefinitions).ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => group.First());
            webApiSwaggerDoc.vendorExtensions = webApiSwaggerDoc.vendorExtensions.UnionEvenIfNull(odataSwaggerDoc.vendorExtensions).ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => group.First());
            webApiSwaggerDoc.tags = webApiSwaggerDoc.tags.UnionEvenIfNull(odataSwaggerDoc.tags, new TagComparer()).ToList();
            webApiSwaggerDoc.consumes = webApiSwaggerDoc.consumes.UnionEvenIfNull(odataSwaggerDoc.consumes).ToList();
            webApiSwaggerDoc.security = webApiSwaggerDoc.security.UnionEvenIfNull(odataSwaggerDoc.security).ToList();
            webApiSwaggerDoc.produces = webApiSwaggerDoc.produces.UnionEvenIfNull(odataSwaggerDoc.produces).ToList();
            webApiSwaggerDoc.schemes = webApiSwaggerDoc.schemes.UnionEvenIfNull(odataSwaggerDoc.schemes).ToList();

            return webApiSwaggerDoc;
        }

        private PathItem CreatePathItem(IEnumerable<ApiDescription> apiDescriptions, SchemaRegistry schemaRegistry)
        {
            Contract.Requires(apiDescriptions != null);
            Contract.Requires(schemaRegistry != null);

            var pathItem = new PathItem();

            // Group further by http method
            var perMethodGrouping = apiDescriptions
                .GroupBy(apiDesc => apiDesc.HttpMethod.Method.ToLower());

            foreach (var group in perMethodGrouping)
            {
                Contract.Assume(group != null);

                var httpMethod = group.Key;

                var apiDescription = group.Count() == 1
                    ? group.First()
                    : _options.ConflictingActionsResolver(group);

                Contract.Assume(apiDescription != null);
                Contract.Assume(apiDescription.ParameterDescriptions != null);
                switch (httpMethod)
                {
                    case "get":
                        pathItem.get = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "put":
                        pathItem.put = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "post":
                        pathItem.post = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "delete":
                        pathItem.delete = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "patch":
                    case "merge":
                        pathItem.patch = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    default:
                        throw new InvalidOperationException($"HttpMethod {httpMethod} is not supported.");
                }
            }

            return pathItem;
        }

        private Operation CreateOperation(ApiDescription apiDescription, SchemaRegistry schemaRegistry)
        {
            Contract.Requires(apiDescription != null);
            Contract.Requires(schemaRegistry != null);
            Contract.Requires(apiDescription.ParameterDescriptions != null);


            var edmModel = ((ODataRoute)apiDescription.Route).GetEdmModel();

            var parameters = apiDescription.ParameterDescriptions
                .Select(paramDesc =>
                {
                    var inPath = apiDescription.RelativePathSansQueryString().Contains("{" + paramDesc.Name + "}");
                    var swaggerApiParameterDescription = paramDesc as SwaggerApiParameterDescription;
                    return swaggerApiParameterDescription != null 
                    ? CreateParameter(swaggerApiParameterDescription, inPath, schemaRegistry, edmModel) 
                    : CreateParameter(paramDesc, inPath, schemaRegistry, edmModel);
                })
                 .ToList();
            var responses = new Dictionary<string, Response>();
            var responseType = apiDescription.ResponseType();
            if (responseType == null || responseType == typeof(void))
                responses.Add("204", new Response { description = "No Content" });
            else
                responses.Add("200", new Response { description = "OK", schema = schemaRegistry.GetOrRegisterResponseType(edmModel, responseType) });

            var operation = new Operation
            {
                summary = apiDescription.Documentation,
                tags = new[] { _options.GroupingKeySelector(apiDescription) },
                operationId = apiDescription.OperationId(),
                produces = apiDescription.Produces()?.ToList(),
                consumes = apiDescription.Consumes()?.ToList(),
                parameters = parameters.Any() ? parameters : null, // parameters can be null but not empty
                responses = responses,
                deprecated = apiDescription.IsObsolete()
            };

            foreach (var filter in _options.OperationFilters)
            {
                Contract.Assume(filter != null);
                filter.Apply(operation, schemaRegistry, apiDescription);
            }

            return operation;
        }

        private static Parameter CreateParameter(ApiParameterDescription paramDesc, bool inPath, SchemaRegistry schemaRegistry, IEdmModel edmModel)
        {
            Contract.Requires(paramDesc != null);
            Contract.Requires(schemaRegistry != null);
            Contract.Assume(paramDesc.ParameterDescriptor != null);

            var @in = inPath
                ? "path"
                : paramDesc.Source == ApiParameterSource.FromUri ? "query" : "body";

            var parameter = new Parameter
            {
                name = paramDesc.Name,
                @in = @in,
                required = inPath || !paramDesc.ParameterDescriptor.IsOptional,
                @default = paramDesc.ParameterDescriptor.DefaultValue
            };

            var schema = schemaRegistry.GetOrRegisterParameterType(edmModel, paramDesc.ParameterDescriptor);
            if (parameter.@in == "body")
                parameter.schema = schema;
            else
                parameter.PopulateFrom(schema);

            return parameter;
        }

        private static Parameter CreateParameter(SwaggerApiParameterDescription paramDesc, bool inPath, SchemaRegistry schemaRegistry, IEdmModel edmModel)
        {
            Contract.Requires(paramDesc != null);
            Contract.Requires(schemaRegistry != null);
            Contract.Assume(paramDesc.ParameterDescriptor != null);

            var @in = inPath
                ? "path"
                : MapToSwaggerParameterLocation(paramDesc.SwaggerSource);

            var parameter = new Parameter
            {
                name = paramDesc.Name,
                description = paramDesc.Documentation,
                @in = @in,
                required = inPath || !paramDesc.ParameterDescriptor.IsOptional,
                @default = paramDesc.ParameterDescriptor.DefaultValue
            };


            var parameterType = paramDesc.ParameterDescriptor.ParameterType;
            Contract.Assume(parameterType != null);
            var schema = schemaRegistry.GetOrRegisterParameterType(edmModel, paramDesc.ParameterDescriptor);
            if (parameter.@in == "body")
                parameter.schema = schema;
            else
                parameter.PopulateFrom(schema);

            return parameter;
        }

        private static string MapToSwaggerParameterLocation(ParameterSource swaggerSource)
        {
            switch (swaggerSource)
            {
                case ParameterSource.Query:
                    return "query";
                case ParameterSource.Header:
                    return "header";
                case ParameterSource.Path:
                    return "path";
                case ParameterSource.FormData:
                    return "formData";
                case ParameterSource.Body:
                    return "body";
                default:
                    throw new ArgumentOutOfRangeException(nameof(swaggerSource), swaggerSource, null);
            }
        }

        private IEnumerable<ApiDescription> GetApiDescriptionsFor(string apiVersion)
        {
            Contract.Ensures(Contract.Result<IEnumerable<ApiDescription>>() != null);

            Contract.Assume(_options.VersionSupportResolver == null || _odataApiExplorer.ApiDescriptions != null);

            var result = _options.VersionSupportResolver == null 
                ? _odataApiExplorer.ApiDescriptions 
                : _odataApiExplorer.ApiDescriptions.Where(apiDesc => _options.VersionSupportResolver(apiDesc, apiVersion));

            Contract.Assume(result != null);
            return result;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_options != null);
            Contract.Invariant(_apiVersions != null);
            Contract.Invariant(_httpConfig != null);
            Contract.Invariant(_odataApiExplorer != null);
        }
    }
}