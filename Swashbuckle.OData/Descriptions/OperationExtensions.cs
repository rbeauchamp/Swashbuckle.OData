using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal static class OperationExtensions
    {
        public static IDictionary<string, string> GenerateSampleQueryParameterValues(this Operation operation)
        {
            Contract.Requires(operation != null);

            return operation.parameters.Where(parameter => parameter.@in == "path")
                .ToDictionary(queryParameter => queryParameter.name, queryParameter => queryParameter.GenerateSamplePathParameterValue());
        }

        public static string GenerateSampleODataAbsoluteUri(this Operation operation, string serviceRoot, string pathTemplate)
        {
            Contract.Requires(operation != null);
            Contract.Requires(serviceRoot != null);

            var uriTemplate = new UriTemplate(pathTemplate);

            var parameters = operation.GenerateSampleQueryParameterValues();

            var prefix = new Uri(serviceRoot);

            return uriTemplate.BindByName(prefix, parameters).ToString();
        }

        public static IList<Parameter> Parameters(this Operation operation)
        {
            Contract.Requires(operation != null);

            return operation.parameters ?? (operation.parameters = new List<Parameter>());
        }
    }
}