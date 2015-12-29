using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Flurl;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    /// <summary>
    /// Gathers ODataActionDescriptors by verifying potential SwaggerRoutes against the API.
    /// </summary>
    internal class SwaggerRouteStrategy : IODataActionDescriptorExplorer
    {
        private const string ServiceRoot = "http://any/";

        private readonly IEnumerable<ISwaggerRouteGenerator> _swaggerRouteGenerators;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerRouteStrategy"/> class.
        /// </summary>
        /// <param name="swaggerRouteGenerators">The swagger route generators.</param>
        public SwaggerRouteStrategy(IEnumerable<ISwaggerRouteGenerator> swaggerRouteGenerators)
        {
            Contract.Requires(swaggerRouteGenerators != null);

            _swaggerRouteGenerators = swaggerRouteGenerators;
        }

        public IEnumerable<ODataActionDescriptor> Generate(HttpConfiguration httpConfig)
        {
            return _swaggerRouteGenerators
                .SelectMany(generator => generator.Generate(httpConfig))
                .SelectMany(potentialSwaggerRoute => GetActionDescriptors(potentialSwaggerRoute, httpConfig));
        }

        private static IEnumerable<ODataActionDescriptor> GetActionDescriptors(SwaggerRoute potentialSwaggerRoute, HttpConfiguration httpConfig)
        {
            Contract.Requires(potentialSwaggerRoute != null);

            var oDataActionDescriptors = new List<ODataActionDescriptor>();

            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("DELETE"), potentialSwaggerRoute.PathItem.delete, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute, httpConfig));
            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("GET"), potentialSwaggerRoute.PathItem.get, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute, httpConfig));
            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("POST"), potentialSwaggerRoute.PathItem.post, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute, httpConfig));
            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("PUT"), potentialSwaggerRoute.PathItem.put, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute, httpConfig));
            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("PATCH"), potentialSwaggerRoute.PathItem.patch, potentialSwaggerRoute.Template, potentialSwaggerRoute.ODataRoute, httpConfig));

            return oDataActionDescriptors;
        }

        private static ODataActionDescriptor GetActionDescriptors(HttpMethod httpMethod, Operation potentialOperation, string potentialPathTemplate, ODataRoute oDataRoute, HttpConfiguration httpConfig)
        {
            Contract.Requires(potentialOperation == null || httpConfig != null);
            Contract.Requires(potentialPathTemplate != null);

            if (potentialOperation != null)
            {
                var request = CreateHttpRequestMessage(httpMethod, potentialOperation, potentialPathTemplate, oDataRoute, httpConfig);

                var actionDescriptor = request.GetHttpActionDescriptor();

                if (actionDescriptor != null)
                {
                    actionDescriptor = MapForRestierIfNecessary(actionDescriptor, potentialOperation);

                    return new ODataActionDescriptor(actionDescriptor, oDataRoute, potentialPathTemplate, potentialOperation);
                }
            }

            return null;
        }

        private static HttpRequestMessage CreateHttpRequestMessage(HttpMethod httpMethod, Operation potentialOperation, string potentialPathTemplate, ODataRoute oDataRoute, HttpConfiguration httpConfig)
        {
            Contract.Requires(httpConfig != null);
            Contract.Requires(oDataRoute != null);
            Contract.Requires(oDataRoute.Constraints != null);
            Contract.Ensures(Contract.Result<HttpRequestMessage>() != null);
            Contract.Ensures(Contract.Result<HttpRequestMessage>().GetRequestContext() != null);

            var oDataAbsoluteUri = potentialOperation.GenerateSampleODataAbsoluteUri(ServiceRoot, potentialPathTemplate);

            var httpRequestMessage = new HttpRequestMessage(httpMethod, oDataAbsoluteUri);

            var odataPath = GenerateSampleODataPath(oDataRoute, oDataAbsoluteUri);

            var requestContext = new HttpRequestContext
            {
                Configuration = httpConfig
            };
            httpRequestMessage.SetConfiguration(httpConfig);
            httpRequestMessage.SetRequestContext(requestContext);

            var httpRequestMessageProperties = httpRequestMessage.ODataProperties();
            Contract.Assume(httpRequestMessageProperties != null);
            httpRequestMessageProperties.Model = oDataRoute.GetEdmModel();
            httpRequestMessageProperties.Path = odataPath;
            httpRequestMessageProperties.RouteName = oDataRoute.GetODataPathRouteConstraint().RouteName;
            httpRequestMessageProperties.RoutingConventions = oDataRoute.GetODataPathRouteConstraint().RoutingConventions;
            httpRequestMessageProperties.PathHandler = oDataRoute.GetODataPathRouteConstraint().PathHandler;
            var httpRouteCollection = httpConfig.Routes;
            Contract.Assume(httpRouteCollection != null);
            var routeData = httpRouteCollection.GetRouteData(httpRequestMessage);
            httpRequestMessage.SetRouteData(routeData);
            return httpRequestMessage;
        }

        private static HttpActionDescriptor MapForRestierIfNecessary(HttpActionDescriptor actionDescriptor, Operation operation)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(operation != null);
            Contract.Requires(operation.tags != null);
            Contract.Requires(actionDescriptor.ControllerDescriptor != null);
            Contract.Requires(actionDescriptor.ControllerDescriptor.ControllerName != @"Restier" || operation.responses != null);

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
                    Contract.Assume(response.schema.items != null);
                    Contract.Assume(response.schema.items.@ref != null);
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
            Contract.Requires(oDataRoute != null);
            Contract.Requires(oDataRoute.Constraints != null);

            var oDataPathRouteConstraint = oDataRoute.GetODataPathRouteConstraint();

            var model = oDataRoute.GetEdmModel();

            return oDataPathRouteConstraint.PathHandler.Parse(model, ServiceRoot.AppendPathSegment(oDataRoute.RoutePrefix), sampleODataAbsoluteUri);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_swaggerRouteGenerators != null);
        }
    }
}