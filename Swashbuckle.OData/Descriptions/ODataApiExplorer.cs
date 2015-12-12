using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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
using Microsoft.OData.Edm;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal class ODataApiExplorer : IApiExplorer
    {
        private const string ServiceRoot = "http://any/";
        private readonly Lazy<Collection<ApiDescription>> _apiDescriptions;
        private readonly Func<HttpConfiguration> _config;
        private readonly SwaggerToApiExplorerMapper _swaggerToApiExplorerMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorer" /> class.
        /// </summary>
        /// <param name="httpConfigurationProvider">The HTTP configuration provider.</param>
        public ODataApiExplorer(Func<HttpConfiguration> httpConfigurationProvider)
        {
            _config = httpConfigurationProvider;
            _apiDescriptions = new Lazy<Collection<ApiDescription>>(GetApiDescriptions);
            _swaggerToApiExplorerMapper = GetSwaggerToApiExplorerMapper();
        }

        private static SwaggerToApiExplorerMapper GetSwaggerToApiExplorerMapper()
        {
            var parameterMappers = new List<IParameterMapper>
            {
                new MapRestierParameter(),
                new MapByParameterName(),
                new MapByDescription(),
                new MapByIndex(),
                new MapToDefault()
            };

            return new SwaggerToApiExplorerMapper(parameterMappers);
        }

        /// <summary>
        /// Gets the API descriptions. The descriptions are initialized on the first access.
        /// </summary>
        public Collection<ApiDescription> ApiDescriptions => _apiDescriptions.Value;

        private Collection<ApiDescription> GetApiDescriptions()
        {
            var apiDescriptions = new Collection<ApiDescription>();

            foreach (var odataRoute in FlattenRoutes(_config().Routes).OfType<ODataRoute>())
            {
                apiDescriptions.AddRange(GetApiDescriptions(odataRoute));
            }

            return apiDescriptions;
        }

        /// <summary>
        /// Explores the route.
        /// </summary>
        /// <param name="oDataRoute">The route.</param>
        /// <returns></returns>
        private Collection<ApiDescription> GetApiDescriptions(ODataRoute oDataRoute)
        {
            var apiDescriptions = new Collection<ApiDescription>();

            foreach (var potentialPathTemplateAndOperations in GetDefaultEdmSwaggerDocument(oDataRoute))
            {
                apiDescriptions.AddRange(GetApiDescriptions(oDataRoute, potentialPathTemplateAndOperations.Key, potentialPathTemplateAndOperations.Value));
            }

            return apiDescriptions;
        }

        private Collection<ApiDescription> GetApiDescriptions(ODataRoute oDataRoute, string potentialPathTemplate, PathItem potentialOperations)
        {
            var apiDescriptions = new Collection<ApiDescription>();

            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("DELETE"), oDataRoute, potentialPathTemplate, potentialOperations.delete));
            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("GET"), oDataRoute, potentialPathTemplate, potentialOperations.get));
            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("POST"), oDataRoute, potentialPathTemplate, potentialOperations.post));
            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("PUT"), oDataRoute, potentialPathTemplate, potentialOperations.put));
            apiDescriptions.AddIfNotNull(GetApiDescription(new HttpMethod("PATCH"), oDataRoute, potentialPathTemplate, potentialOperations.patch));

            return apiDescriptions;
        }

        private ApiDescription GetApiDescription(HttpMethod httpMethod, ODataRoute oDataRoute, string potentialPathTemplate, Operation potentialOperation)
        {
            if (potentialOperation != null)
            {
                var sampleODataAbsoluteUri = GenerateSampleODataAbsoluteUri(potentialPathTemplate, potentialOperation);
                var odataPath = GenerateSampleODataPath(oDataRoute, sampleODataAbsoluteUri);
                var requestContext = new HttpRequestContext
                {
                    Configuration = _config()
                };
                var request = GetHttpRequestMessage(httpMethod, oDataRoute, requestContext, odataPath, sampleODataAbsoluteUri);

                var controllerDesciptor = GetControllerDesciptor(request);
                if (controllerDesciptor != null)
                {
                    return GetApiDescription(httpMethod, oDataRoute, potentialPathTemplate, potentialOperation, controllerDesciptor, request, requestContext);
                }
            }

            return null;
        }

        private ApiDescription GetApiDescription(HttpMethod httpMethod, ODataRoute oDataRoute, string potentialPathTemplate, Operation potentialOperation, HttpControllerDescriptor controllerDesciptor, HttpRequestMessage request, HttpRequestContext requestContext)
        {
            var perControllerConfig = controllerDesciptor.Configuration;
            request.SetConfiguration(perControllerConfig);
            requestContext.Configuration = perControllerConfig;
            requestContext.RouteData = request.GetRouteData();
            requestContext.Url = new UrlHelper(request);
            requestContext.VirtualPathRoot = perControllerConfig.VirtualPathRoot;

            var controller = controllerDesciptor.CreateController(request);
            HttpActionDescriptor actionDescriptor;
            using (controller as IDisposable)
            {
                var controllerContext = new HttpControllerContext(requestContext, request, controllerDesciptor, controller);
                try
                {
                    actionDescriptor = perControllerConfig.Services.GetActionSelector().SelectAction(controllerContext);
                }
                catch (HttpResponseException ex)
                {
                    if (ex.Response.StatusCode == HttpStatusCode.NotFound || ex.Response.StatusCode == HttpStatusCode.MethodNotAllowed)
                    {
                        return null;
                    }
                    throw;
                }
            }

            if (actionDescriptor != null)
            {
                return GetApiDescription(actionDescriptor, httpMethod, potentialOperation, oDataRoute, potentialPathTemplate);
            }
            return null;
        }

        private HttpRequestMessage GetHttpRequestMessage(HttpMethod httpMethod, ODataRoute oDataRoute, HttpRequestContext requestContext, ODataPath odataPath, string oDataAbsoluteUri)
        {
            var httpRequestMessage = new HttpRequestMessage(httpMethod, oDataAbsoluteUri);
            httpRequestMessage.SetConfiguration(_config());
            httpRequestMessage.SetRequestContext(requestContext);
            httpRequestMessage.ODataProperties().Model = GetEdmModel(oDataRoute);
            httpRequestMessage.ODataProperties().Path = odataPath;
            var routeData = _config().Routes.GetRouteData(httpRequestMessage);
            httpRequestMessage.SetRouteData(routeData);
            return httpRequestMessage;
        }

        private ApiDescription GetApiDescription(HttpActionDescriptor actionDescriptor, HttpMethod httpMethod, Operation operation, ODataRoute route, string potentialPathTemplate)
        {
            var localActionDescriptor = GetActionDescriptor(operation, actionDescriptor);

            var apiDocumentation = GetApiDocumentation(localActionDescriptor, operation);

            var parameterDescriptions = CreateParameterDescriptions(operation, localActionDescriptor);

            // request formatters
            var bodyParameter = parameterDescriptions.FirstOrDefault(description => description.SwaggerSource == SwaggerApiParameterSource.Body);
            var supportedRequestBodyFormatters = bodyParameter != null 
                ? localActionDescriptor.Configuration.Formatters.Where(f => f is ODataMediaTypeFormatter && f.CanReadType(bodyParameter.ParameterDescriptor.ParameterType)) 
                : Enumerable.Empty<MediaTypeFormatter>();

            // response formatters
            var responseDescription = CreateResponseDescription(localActionDescriptor);
            var returnType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
            var supportedResponseFormatters = returnType != null && returnType != typeof (void) 
                ? localActionDescriptor.Configuration.Formatters.Where(f => f is ODataMediaTypeFormatter && f.CanWriteType(returnType)) 
                : Enumerable.Empty<MediaTypeFormatter>();

            // Replacing the formatter tracers with formatters if tracers are present.
            supportedRequestBodyFormatters = GetInnerFormatters(supportedRequestBodyFormatters);
            supportedResponseFormatters = GetInnerFormatters(supportedResponseFormatters);

            var apiDescription =  new ApiDescription
            {
                Documentation = apiDocumentation,
                HttpMethod = httpMethod,
                RelativePath = potentialPathTemplate.TrimStart('/'),
                ActionDescriptor = localActionDescriptor,
                Route = route
            };

            apiDescription.SupportedResponseFormatters.AddRange(supportedResponseFormatters);
            apiDescription.SupportedRequestBodyFormatters.AddRange(supportedRequestBodyFormatters.ToList());
            apiDescription.ParameterDescriptions.AddRange(parameterDescriptions);

            // Have to set ResponseDescription because it's internal!??
            apiDescription.GetType().GetProperty("ResponseDescription").SetValue(apiDescription, responseDescription);

            return apiDescription;
        }

        private static HttpActionDescriptor GetActionDescriptor(Operation operation, HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor.ControllerDescriptor.ControllerName == "Restier")
            {
                Response response;
                operation.responses.TryGetValue("200", out response);
                if (!string.IsNullOrWhiteSpace(response?.schema?.@ref))
                {
                    return new SwaggerApiHttpActionDescriptor(actionDescriptor.ActionName, response.schema.GetEntityType(), actionDescriptor.SupportedHttpMethods, operation.tags.First())
                    {
                        Configuration = actionDescriptor.Configuration,
                        ControllerDescriptor = actionDescriptor.ControllerDescriptor
                    };
                }
                if (response?.schema?.type == "array")
                {
                    return new SwaggerApiHttpActionDescriptor(actionDescriptor.ActionName, response.schema.GetEntitySetType(), actionDescriptor.SupportedHttpMethods, operation.tags.First())
                    {
                        Configuration = actionDescriptor.Configuration,
                        ControllerDescriptor = actionDescriptor.ControllerDescriptor
                    };
                }
                return new SwaggerApiHttpActionDescriptor(actionDescriptor.ActionName, null, actionDescriptor.SupportedHttpMethods, operation.tags.First())
                {
                    Configuration = actionDescriptor.Configuration,
                    ControllerDescriptor = actionDescriptor.ControllerDescriptor
                };
            }
            return actionDescriptor;
        }

        private static IEnumerable<MediaTypeFormatter> GetInnerFormatters(IEnumerable<MediaTypeFormatter> mediaTypeFormatters)
        {
            return mediaTypeFormatters.Select(Decorator.GetInner);
        }

        private static ResponseDescription CreateResponseDescription(HttpActionDescriptor actionDescriptor)
        {
            var responseTypeAttribute = actionDescriptor.GetCustomAttributes<ResponseTypeAttribute>();
            var responseType = responseTypeAttribute.Select(attribute => attribute.ResponseType).FirstOrDefault();

            return new ResponseDescription
            {
                DeclaredType = actionDescriptor.ReturnType,
                ResponseType = responseType,
                Documentation = GetApiResponseDocumentation(actionDescriptor)
            };
        }

        private static string GetApiResponseDocumentation(HttpActionDescriptor actionDescriptor)
        {
            var documentationProvider = actionDescriptor.Configuration.Services.GetDocumentationProvider();
            return documentationProvider?.GetResponseDocumentation(actionDescriptor);
        }

        private List<SwaggerApiParameterDescription> CreateParameterDescriptions(Operation operation, HttpActionDescriptor actionDescriptor)
        {
            return operation.parameters.Select((parameter, index) => GetParameterDescription(parameter, index, actionDescriptor)).ToList();
        }

        private SwaggerApiParameterDescription GetParameterDescription(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            var httpParameterDescriptor = GetHttpParameterDescriptor(parameter, index, actionDescriptor);

            return new SwaggerApiParameterDescription
            {
                ParameterDescriptor = httpParameterDescriptor,
                Name = httpParameterDescriptor.Prefix ?? httpParameterDescriptor.ParameterName,
                Documentation = GetApiParameterDocumentation(parameter, httpParameterDescriptor),
                SwaggerSource = MapSource(parameter)
            };
        }

        private static SwaggerApiParameterSource MapSource(Parameter parameter)
        {
            switch (parameter.@in)
            {
                case "query":
                    return SwaggerApiParameterSource.Query;
                case "header":
                    return SwaggerApiParameterSource.Header;
                case "path":
                    return SwaggerApiParameterSource.Path;
                case "formData":
                    return SwaggerApiParameterSource.FormData;
                case "body":
                    return SwaggerApiParameterSource.Body;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
            }
        }

        private HttpParameterDescriptor GetHttpParameterDescriptor(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            return _swaggerToApiExplorerMapper.Map(parameter, index, actionDescriptor);
        }

        private static string GetApiParameterDocumentation(Parameter parameter, HttpParameterDescriptor parameterDescriptor)
        {
            var documentationProvider = parameterDescriptor.Configuration.Services.GetDocumentationProvider();

            return documentationProvider != null 
                ? documentationProvider.GetDocumentation(parameterDescriptor) 
                : parameter.description;
        }

        private static string GetApiDocumentation(HttpActionDescriptor actionDescriptor, Operation operation)
        {
            var documentationProvider = actionDescriptor.Configuration.Services.GetDocumentationProvider();
            return documentationProvider != null 
                ? documentationProvider.GetDocumentation(actionDescriptor) 
                : operation.description;
        }

        private static ODataPath GenerateSampleODataPath(ODataRoute oDataRoute, string sampleODataAbsoluteUri)
        {
            var oDataPathRouteConstraint = GetODataPathRouteConstraint(oDataRoute);

            var model = GetEdmModel(oDataRoute);

            return oDataPathRouteConstraint.PathHandler.Parse(model, ServiceRoot.AppendPathSegment(oDataRoute.RoutePrefix), sampleODataAbsoluteUri);
        }

        private static string GenerateSampleODataAbsoluteUri(string pathTemplate, Operation operation)
        {
            var uriTemplate = new UriTemplate(pathTemplate);

            var parameters = GenerateSampleQueryParameterValues(operation);

            var prefix = new Uri(ServiceRoot);

            return uriTemplate.BindByName(prefix, parameters).ToString();
        }

        private static IDictionary<string, string> GenerateSampleQueryParameterValues(Operation operation)
        {
            return operation.parameters.Where(parameter => parameter.@in == "path").ToDictionary(queryParameter => queryParameter.name, GenerateSampleQueryParameterValue);
        }

        private static string GenerateSampleQueryParameterValue(Parameter queryParameter)
        {
            var type = queryParameter.type;
            var format = queryParameter.format;

            switch (format)
            {
                case null:
                    switch (type)
                    {
                        case "string":
                            return "SampleString";
                        case "boolean":
                            return "true";
                        default:
                            throw new Exception($"Could not generate sample value for query parameter type {type} and format {"null"}");
                    }
                case "int32":
                case "int64":
                    return "42";
                case "byte":
                    return "1";
                case "date":
                    return "2015-12-12T12:00";
                case "date-time":
                    return "2015-10-10T17:00:00Z";
                case "double":
                    return "2.34d";
                case "float":
                    return "2.0f";
                case "guid":
                    return Guid.NewGuid().ToString();
                case "binary":
                    return Convert.ToBase64String(new byte[]{ 130, 200, 234, 23 });
                default:
                    throw new Exception($"Could not generate sample value for query parameter type {type} and format {format}");
            }
        }

        private HttpControllerDescriptor GetControllerDesciptor(HttpRequestMessage request)
        {
            try
            {
                return _config().Services.GetHttpControllerSelector().SelectController(request);
            }
            catch (HttpResponseException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public static string FindMatchingAction(ILookup<string, HttpActionDescriptor> actionMap, params string[] targetActionNames)
        {
            return targetActionNames.FirstOrDefault(actionMap.Contains);
        }

        private static Dictionary<string, PathItem> GetDefaultEdmSwaggerDocument(ODataRoute oDataRoute)
        {
            return new SwaggerPathGenerator(oDataRoute.RoutePrefix, GetEdmModel(oDataRoute)).GenerateSwaggerPaths();
        }

        private static IEdmModel GetEdmModel(ODataRoute oDataRoute)
        {
            return GetODataPathRouteConstraint(oDataRoute).EdmModel;
        }

        private static ODataPathRouteConstraint GetODataPathRouteConstraint(ODataRoute oDataRoute)
        {
            return oDataRoute.Constraints.Values.SingleOrDefault(value => value is ODataPathRouteConstraint) as ODataPathRouteConstraint;
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
    }
}