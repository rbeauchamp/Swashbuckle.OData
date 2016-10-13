using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;
using System;

namespace Swashbuckle.OData.Descriptions
{
    internal class MapByDescription : IParameterMapper
    {
        /// <summary>
        /// The name for parameter keys in the route.
        /// </summary>
        private const string KeyName = "key";

        /// <summary>
        /// Swagger description substring dividing the key from the paramter name.
        /// </summary>
        private const string FindKeyReplacementSubStr = ": ";

        public HttpParameterDescriptor Map(Parameter swaggerParameter, int parameterIndex, HttpActionDescriptor actionDescriptor)
        {
            // Maybe the parameter is a key parameter, e.g., where Id in the URI path maps to a parameter named 'key'
            if (swaggerParameter.description != null && swaggerParameter.description.StartsWith("key:"))
            {
                // Find either a single 'key' in the route or composite keys
                // which take the form of key<parameter name>
                var keyParameterName = swaggerParameter
                                        .description
                                        .Replace(FindKeyReplacementSubStr, 
                                                    String.Empty)
                                        .ToLower();
                var parameterDescriptor = 
                    actionDescriptor
                        .GetParameters()?
                        .SingleOrDefault(descriptor =>
                            descriptor.ParameterName.ToLower() == KeyName
                            || descriptor.ParameterName.ToLower().Equals(keyParameterName)
                        );
                if (parameterDescriptor != null && !parameterDescriptor.IsODataLibraryType())
                {
                    var httpControllerDescriptor = actionDescriptor.ControllerDescriptor;
                    Contract.Assume(httpControllerDescriptor != null);
                    return new ODataParameterDescriptor(swaggerParameter.name, parameterDescriptor.ParameterType, parameterDescriptor.IsOptional, parameterDescriptor)
                    {
                        Configuration = httpControllerDescriptor.Configuration,
                        ActionDescriptor = actionDescriptor,
                        ParameterBinderAttribute = parameterDescriptor.ParameterBinderAttribute
                    };
                }
            }
            return null;
        }
    }
}