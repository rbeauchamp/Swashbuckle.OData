using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
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
            Contract.Requires(httpConfig != null);

            var oDataActionDescriptors = new List<ODataActionDescriptor>();

            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("DELETE"), potentialSwaggerRoute.PathItem.delete, potentialSwaggerRoute, httpConfig));
            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("GET"), potentialSwaggerRoute.PathItem.get, potentialSwaggerRoute, httpConfig));
            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("POST"), potentialSwaggerRoute.PathItem.post, potentialSwaggerRoute, httpConfig));
            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("PUT"), potentialSwaggerRoute.PathItem.put, potentialSwaggerRoute, httpConfig));
            oDataActionDescriptors.AddIfNotNull(GetActionDescriptors(new HttpMethod("PATCH"), potentialSwaggerRoute.PathItem.patch, potentialSwaggerRoute, httpConfig));

            return oDataActionDescriptors;
        }

        private static ODataActionDescriptor GetActionDescriptors(HttpMethod httpMethod, Operation potentialOperation, SwaggerRoute potentialSwaggerRoute, HttpConfiguration httpConfig)
        {
            Contract.Requires(potentialOperation == null || httpConfig != null);
            Contract.Requires(potentialSwaggerRoute != null);

            if (potentialOperation != null)
            {
                var request = CreateHttpRequestMessage(httpMethod, potentialOperation, potentialSwaggerRoute, httpConfig);

                var actionDescriptor = request.GetHttpActionDescriptor(httpConfig);

                if (actionDescriptor != null)
                {
                    actionDescriptor = MapForRestierIfNecessary(request, actionDescriptor);

                    return new ODataActionDescriptor(actionDescriptor, potentialSwaggerRoute.ODataRoute, potentialSwaggerRoute.PrefixedTemplate, request, potentialOperation);
                }
            }

            return null;
        }

        private static HttpRequestMessage CreateHttpRequestMessage(HttpMethod httpMethod, Operation potentialOperation, SwaggerRoute potentialSwaggerRoute, HttpConfiguration httpConfig)
        {
            Contract.Requires(httpConfig != null);
            Contract.Requires(potentialSwaggerRoute != null);
            Contract.Ensures(Contract.Result<HttpRequestMessage>() != null);

            Contract.Assume(potentialSwaggerRoute.ODataRoute.Constraints != null);

            var oDataAbsoluteUri = potentialOperation.GenerateSampleODataUri(ServiceRoot, potentialSwaggerRoute.PrefixedTemplate);
            var oDataRoute = potentialSwaggerRoute.ODataRoute;

            var httpRequestMessage = new HttpRequestMessage(httpMethod, oDataAbsoluteUri);
            var odataPath = GenerateSampleODataPath(potentialOperation, potentialSwaggerRoute, httpConfig.GetODataRootContainer(oDataRoute));

            var requestContext = new HttpRequestContext
            {
                Configuration = httpConfig
            };
            httpRequestMessage.SetConfiguration(httpConfig);
            httpRequestMessage.SetRequestContext(requestContext);
            var httpRequestMessageProperties = httpRequestMessage.ODataProperties();
            Contract.Assume(httpRequestMessageProperties != null);
            httpRequestMessageProperties.Path = odataPath;
            httpRequestMessage.SetRouteData(oDataRoute.GetRouteData("/", httpRequestMessage));
            return httpRequestMessage;
        }

        private static HttpActionDescriptor MapForRestierIfNecessary(HttpRequestMessage request, HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(request != null);
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(actionDescriptor.ControllerDescriptor != null);

            if (actionDescriptor.ControllerDescriptor.ControllerName == "Restier")
            {
                var odataPath = request.ODataProperties().Path;
                var entitySetName = odataPath.NavigationSource.Name;
                Type returnType = null;
                if (ReturnsValue(request))
                {
                    var model = request.GetRequestContainer().GetRequiredService<IEdmModel>();
                    if (odataPath.EdmType.TypeKind == EdmTypeKind.Collection)
                    {
                        var edmElementType = ((IEdmCollectionType) odataPath.EdmType).ElementType;
                        var elementType = EdmLibHelpers.GetClrType(edmElementType, model);
                        var queryableType = typeof (IQueryable<>);
                        returnType = queryableType.MakeGenericType(elementType);
                    }
                    else
                    {
                        returnType = EdmLibHelpers.GetClrType(odataPath.EdmType.ToEdmTypeReference(false), model);
                    }
                }

                return new RestierHttpActionDescriptor(actionDescriptor.ActionName, returnType, actionDescriptor.SupportedHttpMethods, entitySetName)
                {
                    Configuration = actionDescriptor.Configuration,
                    ControllerDescriptor = actionDescriptor.ControllerDescriptor
                };
            }
            return actionDescriptor;
        }

        private static bool ReturnsValue(HttpRequestMessage request)
        {
            return request.Method == HttpMethod.Get || request.Method == HttpMethod.Post;
        }

        private static ODataPath GenerateSampleODataPath(Operation operation, SwaggerRoute swaggerRoute, IServiceProvider rootContainer)
        {
            Contract.Requires(operation != null);
            Contract.Requires(swaggerRoute != null);
            Contract.Requires(swaggerRoute.ODataRoute.Constraints != null);
            Contract.Ensures(Contract.Result<ODataPath>() != null);

            var pathHandler = rootContainer.GetRequiredService<IODataPathHandler>();
            Contract.Assume(pathHandler != null);

            var odataPath = operation.GenerateSampleODataUri(ServiceRoot, swaggerRoute.Template).Replace(ServiceRoot, string.Empty);

            var result = pathHandler.Parse(ServiceRoot, odataPath, rootContainer);
            Contract.Assume(result != null);
            return result;
        }
    }
}