using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class SwaggerOperationMapper : ODataActionDescriptorMapperBase, IODataActionDescriptorMapper
    {
        private readonly IEnumerable<IParameterMapper> _parameterMappers;

        public SwaggerOperationMapper(IEnumerable<IParameterMapper> parameterMappers)
        {
            _parameterMappers = parameterMappers;
        }

        public IEnumerable<ApiDescription> Map(ODataActionDescriptor oDataActionDescriptor)
        {
            var apiDescriptions = new List<ApiDescription>();

            var operation = oDataActionDescriptor.Operation;
            if (operation != null)
            {
                var apiDocumentation = GetApiDocumentation(oDataActionDescriptor.ActionDescriptor, operation);

                var parameterDescriptions = CreateParameterDescriptions(operation, oDataActionDescriptor.ActionDescriptor);

                PopulateApiDescriptions(oDataActionDescriptor, parameterDescriptions, apiDocumentation, apiDescriptions);
            }

            return apiDescriptions;
        }

        private List<ApiParameterDescription> CreateParameterDescriptions(Operation operation, HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(operation != null);

            return operation.parameters?.Select((parameter, index) => GetParameterDescription(parameter, index, actionDescriptor)).ToList();
        }

        private ApiParameterDescription GetParameterDescription(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
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
            Contract.Requires(_parameterMappers != null);

            return _parameterMappers
                .Select(mapper => mapper.Map(parameter, index, actionDescriptor))
                .FirstOrDefault(httpParameterDescriptor => httpParameterDescriptor != null);
        }

        private static string GetApiParameterDocumentation(Parameter parameter, HttpParameterDescriptor parameterDescriptor)
        {
            Contract.Requires(parameterDescriptor != null);
            Contract.Requires(parameterDescriptor.Configuration != null);

            var documentationProvider = parameterDescriptor.Configuration.Services.GetDocumentationProvider();

            return documentationProvider != null
                ? documentationProvider.GetDocumentation(parameterDescriptor)
                : parameter.description;
        }

        private static string GetApiDocumentation(HttpActionDescriptor actionDescriptor, Operation operation)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(actionDescriptor.Configuration != null);

            var documentationProvider = actionDescriptor.Configuration.Services.GetDocumentationProvider();
            return documentationProvider != null
                ? documentationProvider.GetDocumentation(actionDescriptor)
                : operation.description;
        }
    }
}