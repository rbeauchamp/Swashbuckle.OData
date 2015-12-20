using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Routing;
using Flurl;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataApiExplorer : IApiExplorer
    {
        private const string ServiceRoot = "http://any/";
        private readonly Lazy<Collection<ApiDescription>> _apiDescriptions;
        private readonly HttpConfiguration _httpConfig;
        private readonly IODataRouteGenerator _routeGenerator;
        private readonly IEnumerable<IParameterMapper> _parameterMappers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorer" /> class.
        /// </summary>
        /// <param name="httpConfig">The HTTP configuration provider.</param>
        /// <param name="routeGenerator">The swagger path generator.</param>
        /// <param name="parameterMappers">The parameter mappers.</param>
        public ODataApiExplorer(HttpConfiguration httpConfig, IODataRouteGenerator routeGenerator, IEnumerable<IParameterMapper> parameterMappers)
        {
            _httpConfig = httpConfig;
            _routeGenerator = routeGenerator;
            _parameterMappers = parameterMappers;
            _apiDescriptions = new Lazy<Collection<ApiDescription>>(GetApiDescriptions);
        }

        /// <summary>
        /// Gets the API descriptions. The descriptions are initialized on the first access.
        /// </summary>
        public Collection<ApiDescription> ApiDescriptions => _apiDescriptions.Value;

        private Collection<ApiDescription> GetApiDescriptions()
        {
            var apiDescriptions = new List<ApiDescription>();

            foreach (var odataRoute in FlattenRoutes(_httpConfig.Routes).OfType<ODataRoute>())
            {
                apiDescriptions.AddRange(GetApiDescriptions(odataRoute));
            }

            return new Collection<ApiDescription>(apiDescriptions.Distinct(EqualityComparer<ApiDescription>.Create(description => new { description.HttpMethod, description.RelativePath, description.ActionDescriptor })).ToList());
        }

        /// <summary>
        /// Explores the route.
        /// </summary>
        /// <param name="oDataRoute">The route.</param>
        /// <returns></returns>
        private List<ApiDescription> GetApiDescriptions(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);

            var apiDescriptions = new List<ApiDescription>();

            var standardRoutes = _routeGenerator.Generate(oDataRoute.RoutePrefix, oDataRoute.GetEdmModel());

            var customRoutes = _httpConfig.GetCustomSwaggerRoutes(oDataRoute);

            foreach (var potentialRoute in standardRoutes.Concat(customRoutes))
            {
                apiDescriptions.AddRange(GetApiDescriptions(oDataRoute, potentialRoute));
            }

            return apiDescriptions;
        }

        private List<ApiDescription> GetApiDescriptions(ODataRoute oDataRoute, SwaggerRoute potentialSwaggerRoute)
        {
            Contract.Requires(potentialSwaggerRoute != null);

            var apiDescriptions = new List<ApiDescription>();

            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("DELETE"), potentialSwaggerRoute.PathItem.delete, potentialSwaggerRoute.Template, oDataRoute));
            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("GET"), potentialSwaggerRoute.PathItem.get, potentialSwaggerRoute.Template, oDataRoute));
            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("POST"), potentialSwaggerRoute.PathItem.post, potentialSwaggerRoute.Template, oDataRoute));
            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("PUT"), potentialSwaggerRoute.PathItem.put, potentialSwaggerRoute.Template, oDataRoute));
            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("PATCH"), potentialSwaggerRoute.PathItem.patch, potentialSwaggerRoute.Template, oDataRoute));

            return apiDescriptions;
        }

        private ApiDescription GetApiDescription(HttpMethod httpMethod, Operation potentialOperation, string potentialPathTemplate, ODataRoute oDataRoute)
        {
            if (potentialOperation != null)
            {
                HttpActionDescriptor actionDescriptor;
                using (var request = CreateHttpRequestMessage(httpMethod, potentialOperation, potentialPathTemplate, oDataRoute))
                {
                    actionDescriptor = request.GetHttpActionDescriptor();
                }

                actionDescriptor = MapForRestierIfNecessary(actionDescriptor, potentialOperation);

                return GetApiDescription(actionDescriptor, httpMethod, potentialOperation, potentialPathTemplate, oDataRoute);
            }

            return null;
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

        private ApiDescription GetApiDescription(HttpActionDescriptor actionDescriptor, HttpMethod httpMethod, Operation operation, string potentialPathTemplate, ODataRoute oDataRoute)
        {
            Contract.Requires(actionDescriptor == null || operation != null);

            if (actionDescriptor == null)
            {
                return null;
            }

            var apiDocumentation = GetApiDocumentation(actionDescriptor, operation);

            var parameterDescriptions = CreateParameterDescriptions(operation, actionDescriptor);

            // request formatters
            var bodyParameter = default(SwaggerApiParameterDescription);
            if (parameterDescriptions != null)
            {
                bodyParameter = parameterDescriptions.FirstOrDefault(description => description.SwaggerSource == ParameterSource.Body);
            }

            var supportedRequestBodyFormatters = bodyParameter != null ? actionDescriptor.Configuration.Formatters.Where(f => f is ODataMediaTypeFormatter && f.CanReadType(bodyParameter.ParameterDescriptor.ParameterType)) : Enumerable.Empty<MediaTypeFormatter>();

            // response formatters
            var responseDescription = actionDescriptor.CreateResponseDescription();
            var returnType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
            var supportedResponseFormatters = returnType != null && returnType != typeof (void) ? actionDescriptor.Configuration.Formatters.Where(f => f is ODataMediaTypeFormatter && f.CanWriteType(returnType)) : Enumerable.Empty<MediaTypeFormatter>();

            // Replacing the formatter tracers with formatters if tracers are present.
            supportedRequestBodyFormatters = GetInnerFormatters(supportedRequestBodyFormatters);
            supportedResponseFormatters = GetInnerFormatters(supportedResponseFormatters);

            var apiDescription = new ApiDescription
            {
                Documentation = apiDocumentation,
                HttpMethod = httpMethod,
                RelativePath = potentialPathTemplate.TrimStart('/'),
                ActionDescriptor = actionDescriptor,
                Route = oDataRoute
            };

            apiDescription.SupportedResponseFormatters.AddRange(supportedResponseFormatters);
            apiDescription.SupportedRequestBodyFormatters.AddRange(supportedRequestBodyFormatters.ToList());
            if (parameterDescriptions != null)
            {
                apiDescription.ParameterDescriptions.AddRange(parameterDescriptions);
            }

            // Have to set ResponseDescription because it's internal!??
            apiDescription.GetType().GetProperty("ResponseDescription").SetValue(apiDescription, responseDescription);

            return apiDescription;
        }

        private static HttpActionDescriptor MapForRestierIfNecessary(HttpActionDescriptor actionDescriptor, Operation operation)
        {
            if (actionDescriptor == null)
            {
                return null;
            }
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

        private static IEnumerable<MediaTypeFormatter> GetInnerFormatters(IEnumerable<MediaTypeFormatter> mediaTypeFormatters)
        {
            Contract.Requires(mediaTypeFormatters != null);

            return mediaTypeFormatters.Select(Decorator.GetInner);
        }

        private List<SwaggerApiParameterDescription> CreateParameterDescriptions(Operation operation, HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(operation != null);

            return operation.parameters?.Select((parameter, index) => GetParameterDescription(parameter, index, actionDescriptor)).ToList();
        }

        private SwaggerApiParameterDescription GetParameterDescription(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            var httpParameterDescriptor = GetHttpParameterDescriptor(parameter, index, actionDescriptor);

            return new SwaggerApiParameterDescription
            {
                ParameterDescriptor = httpParameterDescriptor,
                Name = httpParameterDescriptor.Prefix ?? httpParameterDescriptor.ParameterName,
                Documentation = GetApiParameterDocumentation(parameter, httpParameterDescriptor),
                SwaggerSource = parameter.MapSource()
            };
        }

        private HttpParameterDescriptor GetHttpParameterDescriptor(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            return _parameterMappers
                .Select(mapper => mapper.Map(parameter, index, actionDescriptor))
                .FirstOrDefault(httpParameterDescriptor => httpParameterDescriptor != null);
        }

        private static string GetApiParameterDocumentation(Parameter parameter, HttpParameterDescriptor parameterDescriptor)
        {
            Contract.Requires(parameterDescriptor != null);

            var documentationProvider = parameterDescriptor.Configuration.Services.GetDocumentationProvider();

            return documentationProvider != null 
                ? documentationProvider.GetDocumentation(parameterDescriptor) 
                : parameter.description;
        }

        private static string GetApiDocumentation(HttpActionDescriptor actionDescriptor, Operation operation)
        {
            Contract.Requires(actionDescriptor != null);

            var documentationProvider = actionDescriptor.Configuration.Services.GetDocumentationProvider();
            return documentationProvider != null 
                ? documentationProvider.GetDocumentation(actionDescriptor) 
                : operation.description;
        }

        private static ODataPath GenerateSampleODataPath(ODataRoute oDataRoute, string sampleODataAbsoluteUri)
        {
            var oDataPathRouteConstraint = oDataRoute.GetODataPathRouteConstraint();

            var model = oDataRoute.GetEdmModel();

            return oDataPathRouteConstraint.PathHandler.Parse(model, ServiceRoot.AppendPathSegment(oDataRoute.RoutePrefix), sampleODataAbsoluteUri);
        }

        private static IEnumerable<IHttpRoute> FlattenRoutes(IEnumerable<IHttpRoute> routes)
        {
            foreach (var route in routes)
            {
                var nested = route as IEnumerable<IHttpRoute>;
                if (nested != null)
                {
                    foreach (var subRoute in FlattenRoutes(nested))
                    {
                        yield return subRoute;
                    }
                }
                else
                {
                    yield return route;
                }
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_apiDescriptions != null);
        }
    }
}