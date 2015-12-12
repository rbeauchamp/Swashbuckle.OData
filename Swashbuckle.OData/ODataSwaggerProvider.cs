using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public class ODataSwaggerProvider : ISwaggerProvider
    {
        private readonly IApiExplorer _apiExplorer;
        private readonly IDictionary<string, Info> _apiVersions;
        private readonly SwaggerGeneratorOptions _options;

        private readonly Func<HttpConfiguration> _httpConfigurationProvider;
        private readonly ISwaggerProvider _defaultProvider;
        private readonly Func<ApiDescription, string> _groupingKeySelector;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        /// </summary>
        /// <param name="defaultProvider">The default provider.</param>
        /// <param name="swaggerDocsConfig">The swagger docs configuration.</param>
        public ODataSwaggerProvider(ISwaggerProvider defaultProvider, SwaggerDocsConfig swaggerDocsConfig) : this(defaultProvider, swaggerDocsConfig, () => GlobalConfiguration.Configuration)
        {
            Contract.Requires(defaultProvider != null);
            Contract.Requires(swaggerDocsConfig != null);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        ///     Use this constructor for self-hosted scenarios.
        /// </summary>
        /// <param name="defaultProvider">The default provider.</param>
        /// <param name="swaggerDocsConfig">The swagger docs configuration.</param>
        /// <param name="httpConfigurationProvider">
        ///     A function that will return the HttpConfiguration that contains the OData Edm
        ///     Model.
        /// </param>
        public ODataSwaggerProvider(ISwaggerProvider defaultProvider, SwaggerDocsConfig swaggerDocsConfig, Func<HttpConfiguration> httpConfigurationProvider)
        {
            Contract.Requires(defaultProvider != null);
            Contract.Requires(swaggerDocsConfig != null);
            Contract.Requires(httpConfigurationProvider != null);

            _apiExplorer = new ODataApiExplorer(httpConfigurationProvider);
            _apiVersions = GetApiVersions(defaultProvider);
            _defaultProvider = defaultProvider;
            _httpConfigurationProvider = httpConfigurationProvider;
            _options = GetSwaggerGeneratorOptions(defaultProvider);
            _groupingKeySelector = SetGroupingKeySelector(swaggerDocsConfig, _options);
        }

        private static Func<ApiDescription, string> SetGroupingKeySelector(SwaggerDocsConfig swaggerDocsConfig, SwaggerGeneratorOptions options)
        {
            return TheUserSetAGroupingKeySelector(swaggerDocsConfig) 
                ? options.GroupingKeySelector 
                : DefineODataGroupingKeySelectorThatSupportsRestier();
        }

        private static Func<ApiDescription, string> DefineODataGroupingKeySelectorThatSupportsRestier()
        {
            return apiDescription => apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName == "Restier" 
                ? ((SwaggerApiHttpActionDescriptor) apiDescription.ActionDescriptor).EntitySetName 
                : apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
        }

        private static bool TheUserSetAGroupingKeySelector(SwaggerDocsConfig swaggerDocsConfig)
        {
            var groupingKeySelector = typeof(SwaggerDocsConfig).GetInstanceField(swaggerDocsConfig, "_groupingKeySelector") as Func<ApiDescription, string>;
            return groupingKeySelector != null;
        }

        /// <summary>
        /// Gets the API versions. I'd rather not use reflection because the implementation may change, but can't find a better way.
        /// </summary>
        /// <param name="defaultProvider">The default provider.</param>
        /// <returns></returns>
        private static IDictionary<string, Info> GetApiVersions(ISwaggerProvider defaultProvider)
        {
            Contract.Requires(defaultProvider is SwaggerGenerator, "The ODataSwaggerProvider currently requires a defaultProvider of type SwaggerGenerator");

            var swaggerGenerator = (SwaggerGenerator)defaultProvider;

            var apiVersions = typeof(SwaggerGenerator).GetInstanceField(swaggerGenerator, "_apiVersions") as IDictionary<string, Info>;
            Contract.Assume(apiVersions != null, "The ODataSwaggerProvider currently requires that the SwaggerGenerator has a non-null field '_apiVersions' of type SwaggerGeneratorOptions");
            return apiVersions;
        }

        /// <summary>
        /// Gets the swagger generator options via Reflection. I'd rather not use reflection because the implementation may change, but can't find a better way.
        /// </summary>
        /// <param name="defaultProvider">The default provider.</param>
        private static SwaggerGeneratorOptions GetSwaggerGeneratorOptions(ISwaggerProvider defaultProvider)
        {
            Contract.Requires(defaultProvider is SwaggerGenerator, "The ODataSwaggerProvider currently requires a defaultProvider of type SwaggerGenerator");

            var swaggerGenerator = (SwaggerGenerator) defaultProvider;

            var options = typeof (SwaggerGenerator).GetInstanceField(swaggerGenerator, "_options") as SwaggerGeneratorOptions;
            Contract.Assume(options != null, "The ODataSwaggerProvider currently requires that the SwaggerGenerator has a non-null field '_options' of type SwaggerGeneratorOptions");
            return options;
        }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            var schemaRegistry = GetSchemaRegistry();

            Info info;
            _apiVersions.TryGetValue(apiVersion, out info);
            if (info == null)
                throw new UnknownApiVersion(apiVersion);

            var paths = GetApiDescriptionsFor(apiVersion)
                .Where(apiDesc => !(_options.IgnoreObsoleteActions && apiDesc.IsObsolete()))
                .OrderBy(_groupingKeySelector, _options.GroupingKeyComparer)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group, schemaRegistry));

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

            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(odataSwaggerDoc, schemaRegistry, _apiExplorer);
            }

            return MergeODataAndWebApiSwaggerDocs(rootUrl, apiVersion, odataSwaggerDoc);
        }

        private SwaggerDocument MergeODataAndWebApiSwaggerDocs(string rootUrl, string apiVersion, SwaggerDocument odataSwaggerDoc)
        {
            var webApiSwaggerDoc = _defaultProvider.GetSwagger(rootUrl, apiVersion);

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
            webApiSwaggerDoc.tags = webApiSwaggerDoc.tags.UnionEvenIfNull(odataSwaggerDoc.tags, EqualityComparer<Tag>.Create(tag => tag.name)).ToList();
            webApiSwaggerDoc.consumes = webApiSwaggerDoc.consumes.UnionEvenIfNull(odataSwaggerDoc.consumes).ToList();
            webApiSwaggerDoc.security = webApiSwaggerDoc.security.UnionEvenIfNull(odataSwaggerDoc.security).ToList();
            webApiSwaggerDoc.produces = webApiSwaggerDoc.produces.UnionEvenIfNull(odataSwaggerDoc.produces).ToList();
            webApiSwaggerDoc.schemes = webApiSwaggerDoc.schemes.UnionEvenIfNull(odataSwaggerDoc.schemes).ToList();

            return webApiSwaggerDoc;
        }

        private SchemaRegistry GetSchemaRegistry()
        {
            return new SchemaRegistry(
                _httpConfigurationProvider().SerializerSettingsOrDefault(), 
                _options.CustomSchemaMappings, _options.SchemaFilters, 
                _options.ModelFilters, 
                _options.IgnoreObsoleteProperties, 
                _options.SchemaIdSelector, 
                _options.DescribeAllEnumsAsStrings, 
                _options.DescribeStringEnumsInCamelCase);
        }

        private PathItem CreatePathItem(IEnumerable<ApiDescription> apiDescriptions, SchemaRegistry schemaRegistry)
        {
            var pathItem = new PathItem();

            // Group further by http method
            var perMethodGrouping = apiDescriptions
                .GroupBy(apiDesc => apiDesc.HttpMethod.Method.ToLower());

            foreach (var group in perMethodGrouping)
            {
                var httpMethod = group.Key;

                var apiDescription = group.Count() == 1
                    ? group.First()
                    : _options.ConflictingActionsResolver(group);

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
                    case "options":
                        pathItem.options = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "head":
                        pathItem.head = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "patch":
                        pathItem.patch = CreateOperation(apiDescription, schemaRegistry);
                        break;
                }
            }

            return pathItem;
        }

        private Operation CreateOperation(ApiDescription apiDescription, SchemaRegistry schemaRegistry)
        {
            Contract.Requires(apiDescription != null);
            Contract.Requires(schemaRegistry != null);

            var parameters = apiDescription.ParameterDescriptions
                .Select(paramDesc =>
                {
                    var inPath = apiDescription.RelativePathSansQueryString().Contains("{" + paramDesc.Name + "}");
                    return CreateParameter(paramDesc as SwaggerApiParameterDescription, inPath, schemaRegistry);
                })
                 .ToList();

            var responses = new Dictionary<string, Response>();
            var responseType = apiDescription.ResponseType();
            if (responseType == null || responseType == typeof(void))
                responses.Add("204", new Response { description = "No Content" });
            else
                responses.Add("200", new Response { description = "OK", schema = schemaRegistry.GetOrRegisterODataType(responseType) });

            var operation = new Operation
            {
                summary = apiDescription.Documentation,
                tags = new[] { _groupingKeySelector(apiDescription) },
                operationId = apiDescription.FriendlyId(),
                produces = apiDescription.Produces().ToList(),
                consumes = apiDescription.Consumes().ToList(),
                parameters = parameters.Any() ? parameters : null, // parameters can be null but not empty
                responses = responses,
                deprecated = apiDescription.IsObsolete()
            };

            foreach (var filter in _options.OperationFilters)
            {
                filter.Apply(operation, schemaRegistry, apiDescription);
            }

            return operation;
        }

        private static Parameter CreateParameter(SwaggerApiParameterDescription paramDesc, bool inPath, SchemaRegistry schemaRegistry)
        {
            var @in = inPath
                ? "path"
                : MapToSwaggerParameterLocation(paramDesc.SwaggerSource);

            var parameter = new Parameter
            {
                name = paramDesc.Name,
                description = paramDesc.Documentation,
                @in = @in
            };

            if (paramDesc.ParameterDescriptor == null)
            {
                parameter.type = "string";
                parameter.required = true;
                return parameter;
            }

            parameter.required = inPath || !paramDesc.ParameterDescriptor.IsOptional;
            parameter.@default = paramDesc.ParameterDescriptor.DefaultValue;

            var schema = schemaRegistry.GetOrRegisterODataType(paramDesc.ParameterDescriptor.ParameterType);
            if (parameter.@in == "body")
                parameter.schema = schema;
            else
                parameter.PopulateFrom(schema);

            return parameter;
        }

        private static string MapToSwaggerParameterLocation(SwaggerApiParameterSource swaggerSource)
        {
            switch (swaggerSource)
            {
                case SwaggerApiParameterSource.Query:
                    return "query";
                case SwaggerApiParameterSource.Header:
                    return "header";
                case SwaggerApiParameterSource.Path:
                    return "path";
                case SwaggerApiParameterSource.FormData:
                    return "formData";
                case SwaggerApiParameterSource.Body:
                    return "body";
                default:
                    throw new ArgumentOutOfRangeException(nameof(swaggerSource), swaggerSource, null);
            }
        }

        private IEnumerable<ApiDescription> GetApiDescriptionsFor(string apiVersion)
        {
            return _options.VersionSupportResolver == null ? _apiExplorer.ApiDescriptions : _apiExplorer.ApiDescriptions.Where(apiDesc => _options.VersionSupportResolver(apiDesc, apiVersion));
        }
    }
}