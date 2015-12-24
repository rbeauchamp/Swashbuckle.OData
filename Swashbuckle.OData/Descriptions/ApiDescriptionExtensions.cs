using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public static class ApiDescriptionExtensions
    {
        public static string GetRelativePathWithQuotedStringParams(this ApiDescription apiDescription)
        {
            var parameters = apiDescription.ParameterDescriptions;

            var newRelativePathSansQueryString = apiDescription.RelativePathSansQueryString();

            foreach (var parameter in parameters)
            {
                if (newRelativePathSansQueryString.Contains("{" + parameter.Name + "}") && parameter.ParameterDescriptor.ParameterType == typeof(string))
                {
                    newRelativePathSansQueryString = newRelativePathSansQueryString.Replace("{" + parameter.Name + "}", "\'{" + parameter.Name + "}\'");
                }
            }

            return apiDescription.RelativePath.Replace(apiDescription.RelativePathSansQueryString(), newRelativePathSansQueryString);
        }
    }
}