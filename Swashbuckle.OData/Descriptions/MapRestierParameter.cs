using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public class MapRestierParameter : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter swaggerParameter, int parameterIndex, HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor.ControllerDescriptor.ControllerName == "Restier")
            {
                return new RestierParameterDescriptor(swaggerParameter)
                {
                    Configuration = actionDescriptor.ControllerDescriptor.Configuration
                };
            }
            return null;
        }
    }
}