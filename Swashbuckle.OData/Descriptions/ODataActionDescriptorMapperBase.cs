using Microsoft.AspNet.OData.Formatter;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using System.Web.Http.Services;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataActionDescriptorMapperBase
    {
        protected void PopulateApiDescriptions(ODataActionDescriptor oDataActionDescriptor, List<ApiParameterDescription> parameterDescriptions, string apiDocumentation, List<ApiDescription> apiDescriptions)
        {
            Contract.Requires(oDataActionDescriptor != null);
            Contract.Requires(apiDescriptions != null);

            // request formatters
            var bodyParameter = default(ApiParameterDescription);
            if (parameterDescriptions != null)
            {
                bodyParameter = parameterDescriptions.FirstOrDefault(description => description.Source == ApiParameterSource.FromBody);
            }

            var httpConfiguration = oDataActionDescriptor.ActionDescriptor.Configuration;
            Contract.Assume(httpConfiguration != null);
            var mediaTypeFormatterCollection = httpConfiguration.Formatters;
            var responseDescription = oDataActionDescriptor.ActionDescriptor.CreateResponseDescription();
            IEnumerable<MediaTypeFormatter> supportedRequestBodyFormatters = new List<MediaTypeFormatter>();
            IEnumerable<MediaTypeFormatter> supportedResponseFormatters = new List<MediaTypeFormatter>();
            if (mediaTypeFormatterCollection != null)
            {
                supportedRequestBodyFormatters = bodyParameter != null ? mediaTypeFormatterCollection.Where(CanReadODataType(oDataActionDescriptor, bodyParameter)) : Enumerable.Empty<MediaTypeFormatter>();

                // response formatters
                var returnType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
                supportedResponseFormatters = returnType != null && returnType != typeof (void) ? mediaTypeFormatterCollection.Where(CanWriteODataType(oDataActionDescriptor, returnType)) : Enumerable.Empty<MediaTypeFormatter>();


                // Replacing the formatter tracers with formatters if tracers are present.
                supportedRequestBodyFormatters = GetInnerFormatters(supportedRequestBodyFormatters);
                supportedResponseFormatters = GetInnerFormatters(supportedResponseFormatters);
            }

            var supportedHttpMethods = GetHttpMethodsSupportedByAction(oDataActionDescriptor.Route, oDataActionDescriptor.ActionDescriptor);
            foreach (var supportedHttpMethod in supportedHttpMethods)
            {
                var apiDescription = new ApiDescription
                {
                    Documentation = apiDocumentation,
                    HttpMethod = supportedHttpMethod,
                    RelativePath = oDataActionDescriptor.RelativePathTemplate.TrimStart('/'),
                    ActionDescriptor = oDataActionDescriptor.ActionDescriptor,
                    Route = oDataActionDescriptor.Route
                };

                var apiSupportedResponseFormatters = apiDescription.SupportedResponseFormatters;
                Contract.Assume(apiSupportedResponseFormatters != null);
                apiSupportedResponseFormatters.AddRange(supportedResponseFormatters);

                var apiSupportedRequestBodyFormatters = apiDescription.SupportedRequestBodyFormatters;
                Contract.Assume(apiSupportedRequestBodyFormatters != null);
                apiSupportedRequestBodyFormatters.AddRange(supportedRequestBodyFormatters);

                if (parameterDescriptions != null)
                {
                    var apiParameterDescriptions = apiDescription.ParameterDescriptions;
                    Contract.Assume(apiParameterDescriptions != null);
                    apiParameterDescriptions.AddRange(parameterDescriptions);
                }

                // Have to set ResponseDescription because it's internal!??
                apiDescription.SetInstanceProperty("ResponseDescription", responseDescription);

                if (apiDescription.ParameterDescriptions != null)
                {
                    apiDescription.RelativePath = apiDescription.GetRelativePathForSwagger();
                }

                apiDescriptions.Add(apiDescription);
            }
        }

        private static Func<MediaTypeFormatter, bool> CanWriteODataType(ODataActionDescriptor oDataActionDescriptor, Type returnType)
        {
            return mediaTypeFormatter =>
            {
                var oDataMediaTypeFormatter = mediaTypeFormatter as ODataMediaTypeFormatter;

                if (oDataMediaTypeFormatter != null)
                {
                    var mediaType = oDataMediaTypeFormatter.SupportedMediaTypes.FirstOrDefault();
                    var instanceFormatter = oDataMediaTypeFormatter.GetPerRequestFormatterInstance(returnType,
                        oDataActionDescriptor.Request, mediaType);
                    return instanceFormatter.CanWriteType(returnType);
                }
                return false;
            };
        }

        private static Func<MediaTypeFormatter, bool> CanReadODataType(ODataActionDescriptor oDataActionDescriptor, ApiParameterDescription bodyParameter)
        {
            return mediaTypeFormatter =>
            {
                var oDataMediaTypeFormatter = mediaTypeFormatter as ODataMediaTypeFormatter;

                if (oDataMediaTypeFormatter != null)
                {
                    var mediaType = oDataMediaTypeFormatter.SupportedMediaTypes.FirstOrDefault();
                    var instanceFormatter = oDataMediaTypeFormatter.GetPerRequestFormatterInstance(bodyParameter.ParameterDescriptor.ParameterType,
                        oDataActionDescriptor.Request, mediaType);
                    return instanceFormatter.CanReadType(bodyParameter.ParameterDescriptor.ParameterType);
                }
                return false;
            };
        }

        /// <summary>
        ///     Gets a collection of HttpMethods supported by the action. Called when initializing the
        ///     <see cref="ApiExplorer.ApiDescriptions" />.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>A collection of HttpMethods supported by the action.</returns>
        private static IEnumerable<HttpMethod> GetHttpMethodsSupportedByAction(IHttpRoute route, HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(route != null);
            Contract.Requires(actionDescriptor != null);
            Contract.Ensures(Contract.Result<IEnumerable<HttpMethod>>() != null);

            Contract.Assume(route.Constraints != null);
            var httpMethodConstraint = route.Constraints.Values.FirstOrDefault(c => c is HttpMethodConstraint) as HttpMethodConstraint;

            IList<HttpMethod> actionHttpMethods = actionDescriptor.SupportedHttpMethods;
            Contract.Assume(actionHttpMethods != null);
            return httpMethodConstraint?.AllowedMethods?.Intersect(actionHttpMethods).ToList() ?? actionHttpMethods;
        }

        private static IEnumerable<MediaTypeFormatter> GetInnerFormatters(IEnumerable<MediaTypeFormatter> mediaTypeFormatters)
        {
            Contract.Requires(mediaTypeFormatters != null);

            return mediaTypeFormatters.Select(Decorator.GetInner);
        }

        protected List<ApiParameterDescription> CreateParameterDescriptions(HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(actionDescriptor != null);

            Contract.Assume(actionDescriptor.ControllerDescriptor == null || actionDescriptor.ControllerDescriptor.Configuration != null);

            var parameterDescriptions = new List<ApiParameterDescription>();
            var actionBinding = GetActionBinding(actionDescriptor);

            var parameterBindings = actionBinding.ParameterBindings;
            if (parameterBindings != null)
            {
                foreach (var parameterBinding in parameterBindings)
                {
                    Contract.Assume(parameterBinding != null || parameterBinding.Descriptor != null);
                    if(parameterBinding.Descriptor.IsODataLibraryType())
                    {
                        continue;
                    }
                    parameterDescriptions.Add(CreateParameterDescriptionFromBinding(parameterBinding));
                }
            }

            return parameterDescriptions;
        }

        private static HttpActionBinding GetActionBinding(HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Ensures(Contract.Result<HttpActionBinding>() != null);

            Contract.Assume(actionDescriptor.ControllerDescriptor?.Configuration != null);

            var controllerDescriptor = actionDescriptor.ControllerDescriptor;
            var controllerServices = controllerDescriptor.Configuration.Services;
            var actionValueBinder = controllerServices.GetActionValueBinder();
            Contract.Assume(actionValueBinder != null);
            var actionBinding = actionValueBinder.GetBinding(actionDescriptor);
            Contract.Assume(actionBinding != null);
            return actionBinding;
        }

        private static ApiParameterDescription CreateParameterDescriptionFromBinding(HttpParameterBinding parameterBinding)
        {
            Contract.Requires(parameterBinding != null);

            Contract.Assume(parameterBinding.Descriptor?.Configuration != null);

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
            Contract.Requires(parameter != null);

            Contract.Assume(parameter.Configuration != null);

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
            Contract.Requires(parameterDescriptor.Configuration != null);

            var documentationProvider = parameterDescriptor.Configuration.Services.GetDocumentationProvider();

            return documentationProvider?.GetDocumentation(parameterDescriptor);
        }
    }
}