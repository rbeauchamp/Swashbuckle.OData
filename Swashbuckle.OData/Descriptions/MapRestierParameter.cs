using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal class MapRestierParameter : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor.ControllerDescriptor.ControllerName == "Restier")
            {
                return new RestierParameterDescriptor(parameter)
                {
                    Configuration = actionDescriptor.ControllerDescriptor.Configuration
                };
            }
            return null;
        }
    }
}