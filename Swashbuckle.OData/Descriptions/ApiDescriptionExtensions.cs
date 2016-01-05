using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public static class ApiDescriptionExtensions
    {
        public static string DefaultGroupingKeySelector(this ApiDescription apiDescription)
        {
            Contract.Requires(apiDescription != null);
            Contract.Requires(apiDescription.ActionDescriptor != null);
            Contract.Requires(apiDescription.ActionDescriptor.ControllerDescriptor != null);

            return apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName == "Restier"
                ? ((RestierHttpActionDescriptor)apiDescription.ActionDescriptor).EntitySetName
                : apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
        }

        public static string OperationId(this ApiDescription apiDescription)
        {
            return $"{apiDescription.DefaultGroupingKeySelector()}_{apiDescription.ActionDescriptor.ActionName}";
        }

        public static string GetRelativePathForSwagger(this ApiDescription apiDescription)
        {
            Contract.Requires(apiDescription != null);
            Contract.Requires(apiDescription.ParameterDescriptions != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var parameters = apiDescription.ParameterDescriptions;

            var newRelativePathSansQueryString = apiDescription.RelativePathSansQueryString();
            Contract.Assume(newRelativePathSansQueryString != null);
            newRelativePathSansQueryString = AdjustRelativePathForStringParams(parameters, newRelativePathSansQueryString);
            newRelativePathSansQueryString = AdjustRelativePathForArrayParams(parameters, newRelativePathSansQueryString);

            var relativePath = apiDescription.RelativePath;
            var oldRelativePathSansQueryString = apiDescription.RelativePathSansQueryString();
            Contract.Assume(relativePath != null);
            Contract.Assume(oldRelativePathSansQueryString != null);
            return relativePath.Replace(oldRelativePathSansQueryString, newRelativePathSansQueryString);
        }

        private static string AdjustRelativePathForStringParams(IEnumerable<ApiParameterDescription> parameters, string newRelativePathSansQueryString)
        {
            Contract.Requires(parameters != null);
            Contract.Requires(newRelativePathSansQueryString != null);

            foreach (var parameter in parameters)
            {
                Contract.Assume(parameter != null);

                var parameterType = parameter.ParameterDescriptor?.ParameterType;
                Contract.Assume(parameterType != null);

                if (newRelativePathSansQueryString.Contains("{" + parameter.Name + "}") && parameterType == typeof (string))
                {
                    newRelativePathSansQueryString = newRelativePathSansQueryString.Replace("{" + parameter.Name + "}", "\'{" + parameter.Name + "}\'");
                }
            }
            return newRelativePathSansQueryString;
        }

        private static string AdjustRelativePathForArrayParams(IEnumerable<ApiParameterDescription> parameters, string newRelativePathSansQueryString)
        {
            Contract.Requires(parameters != null);
            Contract.Requires(newRelativePathSansQueryString != null);

            foreach (var parameter in parameters)
            {
                Contract.Assume(parameter != null);

                var parameterType = parameter.ParameterDescriptor?.ParameterType;
                Contract.Assume(parameterType != null);

                if (newRelativePathSansQueryString.Contains("{" + parameter.Name + "}")
                    && typeof(IEnumerable).IsAssignableFrom(parameterType)
                    && parameterType != typeof(string))
                {
                    newRelativePathSansQueryString = newRelativePathSansQueryString.Replace("{" + parameter.Name + "}", "[{" + parameter.Name + "}]");
                }
            }
            return newRelativePathSansQueryString;
        }
    }
}