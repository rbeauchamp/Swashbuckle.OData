using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using System.Web.OData.Formatter;

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
                    oDataMediaTypeFormatter.SetInstanceProperty("Request", oDataActionDescriptor.Request);
                    return mediaTypeFormatter.CanWriteType(returnType);
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
                    oDataMediaTypeFormatter.SetInstanceProperty("Request", oDataActionDescriptor.Request);
                    return mediaTypeFormatter.CanReadType(bodyParameter.ParameterDescriptor.ParameterType);
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
    }
}