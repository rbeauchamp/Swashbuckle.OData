using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Services;
using System.Web.OData.Formatter;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class SwaggerOperationMapper : IODataActionDescriptorMapper
    {
        private readonly IEnumerable<IParameterMapper> _parameterMappers;

        public SwaggerOperationMapper(IEnumerable<IParameterMapper> parameterMappers)
        {
            _parameterMappers = parameterMappers;
        }

        public IEnumerable<ApiDescription> Map(ODataActionDescriptor oDataActionDescriptor)
        {
            var apiDescriptions = new List<ApiDescription>();

            if (oDataActionDescriptor.Operation != null)
            {
                var apiDocumentation = GetApiDocumentation(oDataActionDescriptor.ActionDescriptor, oDataActionDescriptor.Operation);

                var parameterDescriptions = CreateParameterDescriptions(oDataActionDescriptor.Operation, oDataActionDescriptor.ActionDescriptor);

                // request formatters
                var bodyParameter = default(SwaggerApiParameterDescription);
                if (parameterDescriptions != null)
                {
                    bodyParameter = parameterDescriptions.FirstOrDefault(description => description.SwaggerSource == ParameterSource.Body);
                }

                var supportedRequestBodyFormatters = bodyParameter != null ? oDataActionDescriptor.ActionDescriptor.Configuration.Formatters.Where(f => f is ODataMediaTypeFormatter && f.CanReadType(bodyParameter.ParameterDescriptor.ParameterType)) : Enumerable.Empty<MediaTypeFormatter>();

                // response formatters
                var responseDescription = oDataActionDescriptor.ActionDescriptor.CreateResponseDescription();
                var returnType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
                var supportedResponseFormatters = returnType != null && returnType != typeof (void) ? oDataActionDescriptor.ActionDescriptor.Configuration.Formatters.Where(f => f is ODataMediaTypeFormatter && f.CanWriteType(returnType)) : Enumerable.Empty<MediaTypeFormatter>();

                // Replacing the formatter tracers with formatters if tracers are present.
                supportedRequestBodyFormatters = GetInnerFormatters(supportedRequestBodyFormatters);
                supportedResponseFormatters = GetInnerFormatters(supportedResponseFormatters);

                var apiDescription = new ApiDescription
                {
                    Documentation = apiDocumentation,
                    HttpMethod = oDataActionDescriptor.ActionDescriptor.SupportedHttpMethods.First(),
                    RelativePath = oDataActionDescriptor.RelativePathTemplate.TrimStart('/'),
                    ActionDescriptor = oDataActionDescriptor.ActionDescriptor,
                    Route = oDataActionDescriptor.Route
                };

                apiDescription.SupportedResponseFormatters.AddRange(supportedResponseFormatters);
                apiDescription.SupportedRequestBodyFormatters.AddRange(supportedRequestBodyFormatters.ToList());
                if (parameterDescriptions != null)
                {
                    apiDescription.ParameterDescriptions.AddRange(parameterDescriptions);
                }

                // Have to set ResponseDescription because it's internal!??
                apiDescription.GetType().GetProperty("ResponseDescription").SetValue(apiDescription, responseDescription);

                apiDescription.RelativePath = apiDescription.GetRelativePathForSwagger();

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