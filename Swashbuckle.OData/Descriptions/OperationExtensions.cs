using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal static class OperationExtensions
    {
        public static IDictionary<string, string> GenerateSampleQueryParameterValues(this Operation operation)
        {
            return operation.parameters.Where(parameter => parameter.@in == "path")
                .ToDictionary(queryParameter => queryParameter.name, queryParameter => queryParameter.GenerateSamplePathParameterValue());
        }

        public static string GenerateSampleODataAbsoluteUri(this Operation operation, string serviceRoot, string pathTemplate)
        {
            var uriTemplate = new UriTemplate(pathTemplate);

            var parameters = operation.GenerateSampleQueryParameterValues();

            var prefix = new Uri(serviceRoot);

            return uriTemplate.BindByName(prefix, parameters).ToString();
        }
    }
}