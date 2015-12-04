using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public class ODataApiExplorer : IApiExplorer
    {
        private const string ServiceRoot = "http://any/";
        private readonly Lazy<Collection<ApiDescription>> _apiDescriptions;
        private readonly Func<HttpConfiguration> _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorer" /> class.
        /// </summary>
        /// <param name="httpConfigurationProvider">The HTTP configuration provider.</param>
        public ODataApiExplorer(Func<HttpConfiguration> httpConfigurationProvider)
        {
            _config = httpConfigurationProvider;
            _apiDescriptions = new Lazy<Collection<ApiDescription>>(GetApiDescriptions);
        }

        /// <summary>
        ///     Gets the API descriptions. The descriptions are initialized on the first access.
        /// </summary>
        public Collection<ApiDescription> ApiDescriptions
        {
            get { return _apiDescriptions.Value; }
        }

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
        ///     Explores the route.
        /// </summary>
        /// <param name="oDataRoute">The route.</param>
        /// <returns></returns>
        private Collection<ApiDescription> GetApiDescriptions(ODataRoute oDataRoute)
        {
            var apiDescriptions = new Collection<ApiDescription>();

            foreach (var potentialPathTemplateAndOperations in GetDefaultEdmSwaggerDocument(oDataRoute).paths)
            {
                apiDescriptions.AddRange(GetApiDescriptions(oDataRoute, potentialPathTemplateAndOperations.Key, potentialPathTemplateAndOperations.Value));
            }

            return apiDescriptions;
        }

        private Collection<ApiDescription> GetApiDescriptions(ODataRoute oDataRoute, string potentialPathTemplate, PathItem potentialOperations)
        {
            var apiDescriptions = new Collection<ApiDescription>();

            apiDescriptions.AddIfNotNull(GetApiDescription(HttpMethod.Delete, oDataRoute, potentialPathTemplate, potentialOperations.delete));
            apiDescriptions.AddIfNotNull(GetApiDescription(HttpMethod.Get, oDataRoute, potentialPathTemplate, potentialOperations.get));

            return apiDescriptions;
        }

        private ApiDescription GetApiDescription(HttpMethod httpMethod, ODataRoute oDataRoute, string potentialPathTemplate, Operation potentialOperation)
        {
            if (potentialOperation != null)
            {
                var odataPath = GenerateSampleODataPath(oDataRoute, potentialPathTemplate, potentialOperation);

                var httpControllerDescriptor = GetControllerDesciptor(oDataRoute, odataPath);

                if (httpControllerDescriptor != null)
                {
                    var oDataPathRouteConstraint = GetODataPathRouteConstraint(oDataRoute);

                    var controllerContext = new HttpControllerContext
                    {
                        Request = new HttpRequestMessage(httpMethod, ServiceRoot),
                        RouteData = new HttpRouteData(new HttpRoute())
                    };

                    var actionMappings = _config().Services.GetActionSelector().GetActionMapping(httpControllerDescriptor);

                    var action = GetActionName(oDataPathRouteConstraint, odataPath, controllerContext, actionMappings);

                    if (action != null)
                    {
                        return GetApiDescription(actionMappings[action].First(), httpMethod, potentialOperation, oDataRoute, potentialPathTemplate);
                    }
                }
            }

            return null;
        }

        private ApiDescription GetApiDescription(HttpActionDescriptor actionDescriptor, HttpMethod httpMethod, Operation operation, ODataRoute route, string potentialPathTemplate)
        {
            var apiDocumentation = GetApiDocumentation(actionDescriptor);

            var parameterDescriptions = CreateParameterDescriptions(operation, actionDescriptor);

            // request formatters
            var bodyParameter = parameterDescriptions.FirstOrDefault(description => description.Source == ApiParameterSource.FromBody);
            var supportedRequestBodyFormatters = bodyParameter != null 
                ? actionDescriptor.Configuration.Formatters.Where(f => f.CanReadType(bodyParameter.ParameterDescriptor.ParameterType)) 
                : Enumerable.Empty<MediaTypeFormatter>();

            // response formatters
            var responseDescription = CreateResponseDescription(actionDescriptor);
            var returnType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
            var supportedResponseFormatters = returnType != null && returnType != typeof (void) 
                ? actionDescriptor.Configuration.Formatters.Where(f => f.CanWriteType(returnType)) 
                : Enumerable.Empty<MediaTypeFormatter>();

            // Replacing the formatter tracers with formatters if tracers are present.
            supportedRequestBodyFormatters = GetInnerFormatters(supportedRequestBodyFormatters);
            supportedResponseFormatters = GetInnerFormatters(supportedResponseFormatters);

            var apiDescription =  new ApiDescription
            {
                Documentation = apiDocumentation,
                HttpMethod = httpMethod,
                RelativePath = potentialPathTemplate.TrimStart('/'),
                ActionDescriptor = actionDescriptor,
                Route = route
            };

            apiDescription.SupportedResponseFormatters.AddRange(supportedResponseFormatters);
            apiDescription.SupportedRequestBodyFormatters.AddRange(supportedRequestBodyFormatters.ToList());
            apiDescription.ParameterDescriptions.AddRange(parameterDescriptions);

            // Have to set ResponseDescription because it's internal!??
            apiDescription.GetType().GetProperty("ResponseDescription").SetValue(apiDescription, responseDescription);

            return apiDescription;
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
            return documentationProvider != null 
                ? documentationProvider.GetResponseDocumentation(actionDescriptor) 
                : null;
        }

        private List<ApiParameterDescription> CreateParameterDescriptions(Operation operation, HttpActionDescriptor actionDescriptor)
        {
            return operation.parameters.Select(parameter => GetParameterDescription(parameter, actionDescriptor)).ToList();
        }

        private ApiParameterDescription GetParameterDescription(Parameter parameter, HttpActionDescriptor actionDescriptor)
        {
            var httpParameterDescriptor = GetHttpParameterDescriptor(parameter, actionDescriptor);
            if (httpParameterDescriptor != null)
            {
                return new ApiParameterDescription
                {
                    ParameterDescriptor = httpParameterDescriptor,
                    Name = httpParameterDescriptor.Prefix ?? httpParameterDescriptor.ParameterName,
                    Documentation = GetApiParameterDocumentation(httpParameterDescriptor),
                    Source = parameter.@in == "path" || parameter.@in == "query" ? ApiParameterSource.FromUri : ApiParameterSource.FromBody
                };
            }
            return new ApiParameterDescription
            {
                ParameterDescriptor = new ODataParameterDescriptor(parameter.name, GetType(parameter), parameter.required.Value)
                {
                    Configuration = _config(),
                    ActionDescriptor = actionDescriptor
                },
                Name = parameter.name,
                Documentation = parameter.description,
                Source = parameter.@in == "path" || parameter.@in == "query" ? ApiParameterSource.FromUri : ApiParameterSource.FromBody
            };
        }

        private static HttpParameterDescriptor GetHttpParameterDescriptor(Parameter parameter, HttpActionDescriptor actionDescriptor)
        {
            var httpParameterDescriptor = actionDescriptor.GetParameters().SingleOrDefault(descriptor => descriptor.ParameterName == parameter.name);
            // Maybe the parameter is a key parameter, e.g., where Id in the URI path maps to a parameter named 'key'
            if (httpParameterDescriptor == null && parameter.description.StartsWith("key:"))
            {
                httpParameterDescriptor = actionDescriptor.GetParameters().SingleOrDefault(descriptor => descriptor.ParameterName == "key");
            }
            return httpParameterDescriptor;
        }

        private static string GetApiParameterDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            var documentationProvider = parameterDescriptor.Configuration.Services.GetDocumentationProvider();

            return documentationProvider != null 
                ? documentationProvider.GetDocumentation(parameterDescriptor) 
                : null;
        }

        private static string GetApiDocumentation(HttpActionDescriptor actionDescriptor)
        {
            var documentationProvider = actionDescriptor.Configuration.Services.GetDocumentationProvider();
            if (documentationProvider != null)
            {
                return documentationProvider.GetDocumentation(actionDescriptor);
            }

            return null;
        }

        private static ODataPath GenerateSampleODataPath(ODataRoute oDataRoute, string pathTemplate, Operation operation)
        {
            var sampleODataPathString = GenerateSampleODataPathString(pathTemplate, operation);

            var oDataPathRouteConstraint = GetODataPathRouteConstraint(oDataRoute);

            var model = GetEdmModel(oDataRoute);

            return oDataPathRouteConstraint.PathHandler.Parse(model, ServiceRoot, sampleODataPathString);
        }

        private static string GenerateSampleODataPathString(string pathTemplate, Operation operation)
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
                            throw new Exception(string.Format("Could not generate sample value for query parameter type {0} and format {1}", type, "null"));
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
                default:
                    throw new Exception(string.Format("Could not generate sample value for query parameter type {0} and format {1}", type, format));
            }
        }

        private static Type GetType(Parameter queryParameter)
        {
            var type = queryParameter.type;
            var format = queryParameter.format;

            switch (format)
            {
                case null:
                    switch (type)
                    {
                        case "string":
                            return typeof(string);
                        case "boolean":
                            return typeof(bool);
                        default:
                            throw new Exception(string.Format("Could not determine .NET type for parameter type {0} and format {1}", type, "null"));
                    }
                case "int32":
                    return typeof(int);
                case "int64":
                    return typeof(long);
                case "byte":
                    return typeof(byte);
                case "date":
                    return typeof(DateTime);
                case "date-time":
                    return typeof(DateTimeOffset);
                case "double":
                    return typeof(double);
                case "float":
                    return typeof(float);
                default:
                    throw new Exception(string.Format("Could not determine .NET type for parameter type {0} and format {1}", type, format));
            }
        }

        private HttpControllerDescriptor GetControllerDesciptor(ODataRoute oDataRoute, ODataPath potentialPath)
        {
            var oDataPathRouteConstraint = GetODataPathRouteConstraint(oDataRoute);

            var controllerName = GetControllerName(oDataPathRouteConstraint, potentialPath);

            var controllerMappings = _config().Services.GetHttpControllerSelector().GetControllerMapping();

            HttpControllerDescriptor controllerDescriptor = null;
            if (controllerName != null && controllerMappings != null)
            {
                controllerMappings.TryGetValue(controllerName, out controllerDescriptor);
            }
            return controllerDescriptor;
        }

        /// <summary>
        ///     Selects the name of the controller to dispatch the request to.
        /// </summary>
        /// <param name="oDataPathRouteConstraint">The o data path route constraint.</param>
        /// <param name="path">The OData path of the request.</param>
        /// <returns>
        ///     The name of the controller to dispatch to, or <c>null</c> if one cannot be resolved.
        /// </returns>
        private static string GetControllerName(ODataPathRouteConstraint oDataPathRouteConstraint, ODataPath path)
        {
            return oDataPathRouteConstraint.RoutingConventions.Select(routingConvention => routingConvention.SelectController(path, new HttpRequestMessage())).FirstOrDefault(controllerName => controllerName != null);
        }

        private static string GetActionName(ODataPathRouteConstraint oDataPathRouteConstraint, ODataPath path, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            return oDataPathRouteConstraint.RoutingConventions.Select(routingConvention => routingConvention.SelectAction(path, controllerContext, actionMap)).FirstOrDefault(action => action != null);
        }

        public static string FindMatchingAction(ILookup<string, HttpActionDescriptor> actionMap, params string[] targetActionNames)
        {
            return targetActionNames.FirstOrDefault(actionMap.Contains);
        }

        private static SwaggerDocument GetDefaultEdmSwaggerDocument(ODataRoute oDataRoute)
        {
            return new ODataSwaggerConverter(GetEdmModel(oDataRoute)).ConvertToSwaggerModel();
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

    internal class ODataParameterDescriptor : HttpParameterDescriptor
    {
        public ODataParameterDescriptor(string parameterName, Type parameterType, bool isOptional)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
            IsOptional = isOptional;
        }

        public override string ParameterName { get; }

        public override Type ParameterType { get; }

        public override bool IsOptional { get; }
    }
}