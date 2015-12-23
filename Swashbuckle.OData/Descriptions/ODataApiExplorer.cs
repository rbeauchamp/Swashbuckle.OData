using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Flurl;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataApiExplorer : IApiExplorer
    {
        private const string ServiceRoot = "http://any/";
        private readonly Lazy<Collection<ApiDescription>> _apiDescriptions;
        private readonly HttpConfiguration _httpConfig;
        private readonly IEnumerable<ISwaggerRouteGenerator> _routeGenerators;
        private readonly IEnumerable<IApiDescriptionMapper> _apiDescriptionMappers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorer" /> class.
        /// </summary>
        /// <param name="httpConfig">The HTTP configuration.</param>
        /// <param name="routeGenerators">The route generators.</param>
        /// <param name="apiDescriptionMappers">The ApiDescription mappers.</param>
        public ODataApiExplorer(HttpConfiguration httpConfig, IEnumerable<ISwaggerRouteGenerator> routeGenerators, IEnumerable<IApiDescriptionMapper> apiDescriptionMappers)
        {
            _httpConfig = httpConfig;
            _routeGenerators = routeGenerators;
            _apiDescriptionMappers = apiDescriptionMappers;
            _apiDescriptions = new Lazy<Collection<ApiDescription>>(GetApiDescriptions);
        }

        /// <summary>
        /// Gets the API descriptions. The descriptions are initialized on the first access.
        /// </summary>
        public Collection<ApiDescription> ApiDescriptions => _apiDescriptions.Value;

        private Collection<ApiDescription> GetApiDescriptions()
        {
            var apiDescriptions = new List<ApiDescription>();

            apiDescriptions.AddRange(GetApiDescriptionsFromSwaggerRouteGenerators());
            apiDescriptions.AddRange(GetApiDescriptionsFromAttributeRoutes());

            return apiDescriptions.Distinct(EqualityComparer<ApiDescription>.Create(description => new
            {
                description.HttpMethod,
                description.RelativePath,
                description.ActionDescriptor
            })).ToCollection();
        }

        private IEnumerable<ApiDescription> GetApiDescriptionsFromAttributeRoutes()
        {
            return _httpConfig.GetODataRoutes().SelectMany(GetApiDescriptionsFromAttributeRoutes);
        }

        private IEnumerable<ApiDescription> GetApiDescriptionsFromAttributeRoutes(ODataRoute oDataRoute)
        {
            var attributeRoutingConvention = (AttributeRoutingConvention)oDataRoute.GetODataPathRouteConstraint().RoutingConventions.SingleOrDefault(convention => convention is AttributeRoutingConvention);

            if (attributeRoutingConvention != null)
            {
                return attributeRoutingConvention
                    .GetInstanceField<IDictionary<ODataPathTemplate, HttpActionDescriptor>>("_attributeMappings")
                    .SelectMany(pair => GetApiDescriptionsFromAttributeRoutes(pair.Value, oDataRoute));
            }

            return new List<ApiDescription>();
        }

        private IEnumerable<ApiDescription> GetApiDescriptionsFromAttributeRoutes(HttpActionDescriptor actionDescriptor, ODataRoute oDataRoute)
        {
            var odataRouteAttribute = actionDescriptor.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault();
            if (odataRouteAttribute != null)
            {
                var pathTemplate = HttpUtility.UrlDecode(oDataRoute.RoutePrefix.AppendPathSegment(odataRouteAttribute.PathTemplate));
                return _apiDescriptionMappers
                    .Select(mapper => mapper.Map(actionDescriptor, oDataRoute, pathTemplate))
                    .FirstOrDefault(apiDescriptions => apiDescriptions.Any()) ?? new List<ApiDescription>();
            }
            return new List<ApiDescription>();
        }

        private IEnumerable<ApiDescription> GetApiDescriptionsFromSwaggerRouteGenerators()
        {
            return _routeGenerators.SelectMany(generator => generator.Generate(_httpConfig)).SelectMany(GetApiDescriptions);
        }

        private List<ApiDescription> GetApiDescriptions(SwaggerRoute potentialSwaggerRoute)
        {
            Contract.Requires(potentialSwaggerRoute != null);

            var apiDescriptions = new List<ApiDescription>();

            apiDescriptions.AddRange(GetApiDescriptions(new HttpMethod("DELETE"), potentialSwaggerRoute.PathItem.delete, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute));
            apiDescriptions.AddRange(GetApiDescriptions(new HttpMethod("GET"), potentialSwaggerRoute.PathItem.get, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute));
            apiDescriptions.AddRange(GetApiDescriptions(new HttpMethod("POST"), potentialSwaggerRoute.PathItem.post, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute));
            apiDescriptions.AddRange(GetApiDescriptions(new HttpMethod("PUT"), potentialSwaggerRoute.PathItem.put, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute));
            apiDescriptions.AddRange(GetApiDescriptions(new HttpMethod("PATCH"), potentialSwaggerRoute.PathItem.patch, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute));

            return apiDescriptions;
        }

        private IEnumerable<ApiDescription> GetApiDescriptions(HttpMethod httpMethod, Operation potentialOperation, string potentialPathTemplate, ODataRoute oDataRoute)
        {
            if (potentialOperation != null)
            {
                var request = CreateHttpRequestMessage(httpMethod, potentialOperation, potentialPathTemplate, oDataRoute);

                var actionDescriptor = request.GetHttpActionDescriptor();

                if (actionDescriptor != null)
                {
                    actionDescriptor = MapForRestierIfNecessary(actionDescriptor, potentialOperation);

                    return GetApiDescriptions(actionDescriptor, potentialOperation, potentialPathTemplate, oDataRoute);
                }
            }

            return new List<ApiDescription>();
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod httpMethod, Operation potentialOperation, string potentialPathTemplate, ODataRoute oDataRoute)
        {
            var oDataAbsoluteUri = potentialOperation.GenerateSampleODataAbsoluteUri(ServiceRoot, potentialPathTemplate);

            var httpRequestMessage = new HttpRequestMessage(httpMethod, oDataAbsoluteUri);

            var odataPath = GenerateSampleODataPath(oDataRoute, oDataAbsoluteUri);

            var requestContext = new HttpRequestContext
            {
                Configuration = _httpConfig
            };
            httpRequestMessage.SetConfiguration(_httpConfig);
            httpRequestMessage.SetRequestContext(requestContext);
            httpRequestMessage.ODataProperties().Model = oDataRoute.GetEdmModel();
            httpRequestMessage.ODataProperties().Path = odataPath;
            httpRequestMessage.ODataProperties().RouteName = oDataRoute.GetODataPathRouteConstraint().RouteName;
            httpRequestMessage.ODataProperties().RoutingConventions = oDataRoute.GetODataPathRouteConstraint().RoutingConventions;
            httpRequestMessage.ODataProperties().PathHandler = oDataRoute.GetODataPathRouteConstraint().PathHandler;
            var routeData = _httpConfig.Routes.GetRouteData(httpRequestMessage);
            httpRequestMessage.SetRouteData(routeData);
            return httpRequestMessage;
        }

        private IEnumerable<ApiDescription> GetApiDescriptions(HttpActionDescriptor actionDescriptor, Operation operation, string relativePathTemplate, ODataRoute route)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(operation != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(relativePathTemplate));
            Contract.Requires(route != null);

            return _apiDescriptionMappers
                .Select(mapper => mapper.Map(actionDescriptor, route, relativePathTemplate, operation))
                .FirstOrDefault(apiDescriptions => apiDescriptions.Any()) ?? new List<ApiDescription>();
        }

        private static HttpActionDescriptor MapForRestierIfNecessary(HttpActionDescriptor actionDescriptor, Operation operation)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(operation != null);

            if (actionDescriptor.ControllerDescriptor.ControllerName == "Restier")
            {
                Response response;
                operation.responses.TryGetValue("200", out response);
                if (!string.IsNullOrWhiteSpace(response?.schema?.@ref))
                {
                    return new RestierHttpActionDescriptor(actionDescriptor.ActionName, response.schema.GetEntityType(), actionDescriptor.SupportedHttpMethods, operation.tags.First())
                    {
                        Configuration = actionDescriptor.Configuration,
                        ControllerDescriptor = actionDescriptor.ControllerDescriptor
                    };
                }
                if (response?.schema?.type == "array")
                {
                    return new RestierHttpActionDescriptor(actionDescriptor.ActionName, response.schema.GetEntitySetType(), actionDescriptor.SupportedHttpMethods, operation.tags.First())
                    {
                        Configuration = actionDescriptor.Configuration,
                        ControllerDescriptor = actionDescriptor.ControllerDescriptor
                    };
                }
                return new RestierHttpActionDescriptor(actionDescriptor.ActionName, null, actionDescriptor.SupportedHttpMethods, operation.tags.First())
                {
                    Configuration = actionDescriptor.Configuration,
                    ControllerDescriptor = actionDescriptor.ControllerDescriptor
                };
            }
            return actionDescriptor;
        }

        private static ODataPath GenerateSampleODataPath(ODataRoute oDataRoute, string sampleODataAbsoluteUri)
        {
            var oDataPathRouteConstraint = oDataRoute.GetODataPathRouteConstraint();

            var model = oDataRoute.GetEdmModel();

            return oDataPathRouteConstraint.PathHandler.Parse(model, ServiceRoot.AppendPathSegment(oDataRoute.RoutePrefix), sampleODataAbsoluteUri);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_apiDescriptions != null);
        }
    }
}