using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Description;
using System.Web.OData;
using Swashbuckle.OData.Descriptions;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    /// <summary>
    /// Adds query option parameters to the operation if the action has the [EnableQuery] attribute applied.
    /// </summary>
    public class EnableQueryFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            Contract.Assume(operation != null);
            Contract.Assume(schemaRegistry != null);
            Contract.Assume(apiDescription != null);

            if (HasEnableQueryAttribute(apiDescription) && !HasAnyQueryOptionParameters(operation))
            {
                operation.parameters = ODataSwaggerUtilities.AddQueryOptionParameters(operation.parameters ?? new List<Parameter>());
            }
        }

        private static bool HasAnyQueryOptionParameters(Operation operation)
        {
            return operation.parameters != null && operation.parameters.Any(parameter => parameter.name.StartsWith("$") && parameter.@in == "query");
        }

        private static bool HasEnableQueryAttribute(ApiDescription apiDescription)
        {
            var httpActionDescriptor = apiDescription.ActionDescriptor;
            Contract.Assume(httpActionDescriptor != null);
            return httpActionDescriptor.GetCustomAttributes<EnableQueryAttribute>().Any();
        }
    }
}