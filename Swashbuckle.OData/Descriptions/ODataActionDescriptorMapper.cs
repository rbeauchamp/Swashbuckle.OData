using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataActionDescriptorMapper : ODataActionDescriptorMapperBase, IODataActionDescriptorMapper
    {
        public IEnumerable<ApiDescription> Map(ODataActionDescriptor oDataActionDescriptor)
        {
            var apiDescriptions = new List<ApiDescription>();

            var apiDocumentation = GetApiDocumentation(oDataActionDescriptor.ActionDescriptor);

            var parameterDescriptions = CreateParameterDescriptions(oDataActionDescriptor.ActionDescriptor);

            PopulateApiDescriptions(oDataActionDescriptor, parameterDescriptions, apiDocumentation, apiDescriptions);

            return apiDescriptions;
        }

        private static List<ApiParameterDescription> CreateParameterDescriptions(HttpActionDescriptor actionDescriptor)
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
                    Contract.Assume(parameterBinding != null);
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

        private static string GetApiDocumentation(HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(actionDescriptor.Configuration != null);

            var documentationProvider = actionDescriptor.Configuration.Services.GetDocumentationProvider();
            return documentationProvider?.GetDocumentation(actionDescriptor);
        }
    }
}