using System.Linq;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal class MapByDescription : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            // Maybe the parameter is a key parameter, e.g., where Id in the URI path maps to a parameter named 'key'
            if (parameter.description.StartsWith("key:"))
            {
                var parameterDescriptor = actionDescriptor.GetParameters().SingleOrDefault(descriptor => descriptor.ParameterName == "key");
                if (parameterDescriptor != null)
                {
                    // Need to assign the correct name expected by OData
                    return new ODataParameterDescriptor(parameter.name, parameterDescriptor.ParameterType, parameterDescriptor.IsOptional)
                    {
                        Configuration = actionDescriptor.ControllerDescriptor.Configuration,
                        ActionDescriptor = actionDescriptor,
                        ParameterBinderAttribute = parameterDescriptor.ParameterBinderAttribute
                    };
                }
            }
            return null;
        }
    }
}