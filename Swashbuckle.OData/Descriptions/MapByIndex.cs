using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal class MapByIndex : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            if (parameter.@in != "query" && index < actionDescriptor.GetParameters().Count)
            {
                var parameterDescriptor = actionDescriptor.GetParameters()[index];
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