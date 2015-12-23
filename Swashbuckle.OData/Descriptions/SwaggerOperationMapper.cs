using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Services;
using System.Web.OData.Formatter;
using System.Web.OData.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class SwaggerOperationMapper : IApiDescriptionMapper
    {
        private readonly IEnumerable<IParameterMapper> _parameterMappers;

        public SwaggerOperationMapper(IEnumerable<IParameterMapper> parameterMappers)
        {
            _parameterMappers = parameterMappers;
        }

        public IEnumerable<ApiDescription> Map(HttpActionDescriptor actionDescriptor, ODataRoute route, string relativePathTemplate, Operation operation = null)
        {
            var apiDescriptions = new List<ApiDescription>();

            if (operation != null)
            {
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
                    HttpMethod = actionDescriptor.SupportedHttpMethods.First(),
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
    }
}