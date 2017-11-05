using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Description;
using System.Web.OData;
using Swashbuckle.OData.Descriptions;
using Swashbuckle.Swagger;
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

            //Look if it has set a response type in the attributes
            var swgResponseTypeAttr = httpActionDescriptor.GetCustomAttributes<Swagger.Annotations.SwaggerResponseAttribute>()?.FirstOrDefault();
            if (swgResponseTypeAttr != null)
                returnType = swgResponseTypeAttr.Type;
            else
            {
                var responseTypeAttr = httpActionDescriptor.GetCustomAttributes<ResponseTypeAttribute>()?.FirstOrDefault();
                if (responseTypeAttr != null)
                    returnType = responseTypeAttr.ResponseType;
            }

            returnType = GetValueTypeFromODataResponseOrDescendants(returnType);

            return returnType == null ? false : returnType.IsCollection();
        }

        /// <summary>
        /// if <paramref name="type"/> or one of its parents is an implementation of <see cref="Swashbuckle.OData.ODataResponse{TValue}"/>
        /// returns TValue, otherwise, return type
        /// </summary>
        /// <param name="type">type to be evaluated</param>
        /// <returns>TValue or returntype</returns>
        private static Type GetValueTypeFromODataResponseOrDescendants(Type returnType)
        {
            var type = returnType;

            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ODataResponse<>))                
                    return type.GetGenericArguments().First();                    
                                
                type = type.BaseType;
            }
            return returnType;
        }


    }
}