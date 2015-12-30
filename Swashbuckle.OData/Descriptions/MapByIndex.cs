using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class MapByIndex : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter swaggerParameter, int parameterIndex, HttpActionDescriptor actionDescriptor)
        {
            if (swaggerParameter.@in != "query" && parameterIndex < actionDescriptor.GetParameters().Count)
            {
                var parameterDescriptor = actionDescriptor.GetParameters()[parameterIndex];
                if (parameterDescriptor != null)
                {
                    var httpControllerDescriptor = actionDescriptor.ControllerDescriptor;
                    Contract.Assume(httpControllerDescriptor != null);
                    return new ODataParameterDescriptor(swaggerParameter.name, parameterDescriptor.ParameterType, parameterDescriptor.IsOptional)
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