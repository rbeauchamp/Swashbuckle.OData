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
using System.Web.OData.Formatter;
using System.Web.OData.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class ApiDescriptionMapper : IApiDescriptionMapper
    {
        public IEnumerable<ApiDescription> Map(HttpActionDescriptor actionDescriptor, ODataRoute route, string relativePathTemplate, Operation operation = null)
        {
            var apiDocumentation = GetApiDocumentation(actionDescriptor);

            // parameters
            var parameterDescriptions = CreateParameterDescriptions(actionDescriptor);

            // request formatters
            var bodyParameter = parameterDescriptions.FirstOrDefault(description => description.Source == ApiParameterSource.FromBody);

            var supportedRequestBodyFormatters = bodyParameter != null 
                ? actionDescriptor.Configuration.Formatters.Where(f => f is ODataMediaTypeFormatter && f.CanReadType(bodyParameter.ParameterDescriptor.ParameterType)) 
                : Enumerable.Empty<MediaTypeFormatter>();

            // response formatters
            var responseDescription = CreateResponseDescription(actionDescriptor);
            var returnType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
            var supportedResponseFormatters = returnType != null && returnType != typeof(void) 
                ? actionDescriptor.Configuration.Formatters.Where(f => f is ODataMediaTypeFormatter && f.CanWriteType(returnType)) 
                : Enumerable.Empty<MediaTypeFormatter>();

            // Replacing the formatter tracers with formatters if tracers are present.
            supportedRequestBodyFormatters = GetInnerFormatters(supportedRequestBodyFormatters);
            supportedResponseFormatters = GetInnerFormatters(supportedResponseFormatters);

            // get HttpMethods supported by an action. Usually there is one HttpMethod per action but we allow multiple of them per action as well.
            IList<HttpMethod> supportedMethods = GetHttpMethodsSupportedByAction(route, actionDescriptor);

            var apiDescriptions = new List<ApiDescription>();
            foreach (var method in supportedMethods)
            {
                var apiDescription = new ApiDescription
                {
                    Documentation = apiDocumentation,
                    HttpMethod = method,
                    RelativePath = relativePathTemplate.TrimStart('/'),
                    ActionDescriptor = actionDescriptor,
                    Route = route
                };
                apiDescription.SupportedResponseFormatters.AddRange(supportedResponseFormatters);
                apiDescription.SupportedRequestBodyFormatters.AddRange(supportedRequestBodyFormatters.ToList());
                if (parameterDescriptions != null)
                {
                    apiDescription.ParameterDescriptions.AddRange(parameterDescriptions);
                }

                // Have to set ResponseDescription because it's internal!??
                apiDescription.GetType().GetProperty("ResponseDescription").SetValue(apiDescription, responseDescription);

                apiDescriptions.Add(apiDescription);
            }
            return apiDescriptions;
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

        private static IEnumerable<MediaTypeFormatter> GetInnerFormatters(IEnumerable<MediaTypeFormatter> mediaTypeFormatters)
        {
            return mediaTypeFormatters.Select(Decorator.GetInner);
        }

        /// <summary>
        ///     Gets a collection of HttpMethods supported by the action. Called when initializing the
        ///     <see cref="ApiExplorer.ApiDescriptions" />.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>A collection of HttpMethods supported by the action.</returns>
        public virtual Collection<HttpMethod> GetHttpMethodsSupportedByAction(IHttpRoute route, HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(route != null);
            Contract.Requires(actionDescriptor != null);

            IList<HttpMethod> actionHttpMethods = actionDescriptor.SupportedHttpMethods;
            var httpMethodConstraint = route.Constraints.Values.FirstOrDefault(c => c is HttpMethodConstraint) as HttpMethodConstraint;

            var supportedMethods = httpMethodConstraint?.AllowedMethods.Intersect(actionHttpMethods).ToList() ?? actionHttpMethods;

            return new Collection<HttpMethod>(supportedMethods);
        }

        private static IList<ApiParameterDescription> CreateParameterDescriptions(HttpActionDescriptor actionDescriptor)
        {
            IList<ApiParameterDescription> parameterDescriptions = new List<ApiParameterDescription>();
            var actionBinding = GetActionBinding(actionDescriptor);

            // try get parameter binding information if available
            if (actionBinding != null)
            {
                var parameterBindings = actionBinding.ParameterBindings;
                if (parameterBindings != null)
                {
                    foreach (var parameter in parameterBindings)
                    {
                        parameterDescriptions.Add(CreateParameterDescriptionFromBinding(parameter));
                    }
                }
            }
            else
            {
                var parameters = actionDescriptor.GetParameters();
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        parameterDescriptions.Add(CreateParameterDescriptionFromDescriptor(parameter));
                    }
                }
            }

            return parameterDescriptions;
        }

        private static HttpActionBinding GetActionBinding(HttpActionDescriptor actionDescriptor)
        {
            var controllerDescriptor = actionDescriptor.ControllerDescriptor;
            if (controllerDescriptor == null)
            {
                return null;
            }

            var controllerServices = controllerDescriptor.Configuration.Services;
            var actionValueBinder = controllerServices.GetActionValueBinder();
            var actionBinding = actionValueBinder?.GetBinding(actionDescriptor);
            return actionBinding;
        }

        private static ApiParameterDescription CreateParameterDescriptionFromBinding(HttpParameterBinding parameterBinding)
        {
            var parameterDescription = CreateParameterDescriptionFromDescriptor(parameterBinding.Descriptor);
            if (parameterBinding.WillReadBody)
            {
                parameterDescription.Source = ApiParameterSource.FromBody;
            }
            else if (parameterBinding.WillReadUri())
            {
                parameterDescription.Source = ApiParameterSource.FromUri;
            }

            return parameterDescription;
        }

        private static ApiParameterDescription CreateParameterDescriptionFromDescriptor(HttpParameterDescriptor parameter)
        {
            Contract.Assert(parameter != null);
            return new ApiParameterDescription
            {
                ParameterDescriptor = parameter,
                Name = parameter.Prefix ?? parameter.ParameterName,
                Documentation = GetApiParameterDocumentation(parameter),
                Source = ApiParameterSource.Unknown
            };
        }

        private static string GetApiParameterDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            Contract.Requires(parameterDescriptor != null);

            var documentationProvider = parameterDescriptor.Configuration.Services.GetDocumentationProvider();

            return documentationProvider?.GetDocumentation(parameterDescriptor);
        }

        private static string GetApiDocumentation(HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(actionDescriptor != null);

            var documentationProvider = actionDescriptor.Configuration.Services.GetDocumentationProvider();
            return documentationProvider?.GetDocumentation(actionDescriptor);
        }
    }
}