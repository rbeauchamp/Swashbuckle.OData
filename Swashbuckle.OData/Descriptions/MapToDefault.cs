using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public class MapToDefault : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter swaggerParameter, int parameterIndex, HttpActionDescriptor actionDescriptor)
        {
            return new ODataParameterDescriptor(swaggerParameter.name, swaggerParameter.GetClrType(), !swaggerParameter.required.Value)
            {
                Configuration = actionDescriptor.ControllerDescriptor.Configuration,
                ActionDescriptor = actionDescriptor
            };
        }
    }
}