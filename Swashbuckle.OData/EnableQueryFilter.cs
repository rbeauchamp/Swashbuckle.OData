using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Description;
using System.Web.OData;
using Swashbuckle.OData.Descriptions;
using Swashbuckle.Swagger;
using System.Web.Http;
using System;

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
                operation.parameters = ReturnsCollection(apiDescription) 
                    ? ODataSwaggerUtilities.AddQueryOptionParametersForEntitySet(operation.parameters ?? new List<Parameter>())
                    : ODataSwaggerUtilities.AddQueryOptionParametersForEntity(operation.parameters ?? new List<Parameter>()); 
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

        private static bool ReturnsCollection(ApiDescription apiDescription)
        {
            var httpActionDescriptor = apiDescription.ActionDescriptor;
            Contract.Assume(httpActionDescriptor != null);

            Type returnType = httpActionDescriptor.ReturnType;

            var responseTypeAttr = httpActionDescriptor.GetCustomAttributes<ResponseTypeAttribute>().FirstOrDefault();
            if (responseTypeAttr != null)
                returnType = responseTypeAttr.ResponseType;

            return returnType.IsCollection();
        }


    }
}