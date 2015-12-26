using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public static class ApiDescriptionExtensions
    {
        public static string GetRelativePathForSwagger(this ApiDescription apiDescription)
        {
            var parameters = apiDescription.ParameterDescriptions;

            var newRelativePathSansQueryString = apiDescription.RelativePathSansQueryString();

            newRelativePathSansQueryString = AdjustRelativePathForStringParams(parameters, newRelativePathSansQueryString);
            newRelativePathSansQueryString = AdjustRelativePathForArrayParams(parameters, newRelativePathSansQueryString);

            return apiDescription.RelativePath.Replace(apiDescription.RelativePathSansQueryString(), newRelativePathSansQueryString);
        }

        private static string AdjustRelativePathForStringParams(IEnumerable<ApiParameterDescription> parameters, string newRelativePathSansQueryString)
        {
            foreach (var parameter in parameters)
            {
                if (newRelativePathSansQueryString.Contains("{" + parameter.Name + "}") && parameter.ParameterDescriptor.ParameterType == typeof (string))
                {
                    newRelativePathSansQueryString = newRelativePathSansQueryString.Replace("{" + parameter.Name + "}", "\'{" + parameter.Name + "}\'");
                }
            }
            return newRelativePathSansQueryString;
        }

        private static string AdjustRelativePathForArrayParams(IEnumerable<ApiParameterDescription> parameters, string newRelativePathSansQueryString)
        {
            foreach (var parameter in parameters)
            {
                if (newRelativePathSansQueryString.Contains("{" + parameter.Name + "}")
                    && typeof(IEnumerable).IsAssignableFrom(parameter.ParameterDescriptor.ParameterType)
                    && parameter.ParameterDescriptor.ParameterType != typeof(string))
                {
                    newRelativePathSansQueryString = newRelativePathSansQueryString.Replace("{" + parameter.Name + "}", "[{" + parameter.Name + "}]");
                }
            }
            return newRelativePathSansQueryString;
        }
    }
}