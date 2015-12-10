using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal class MapToDefault : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            return new ODataParameterDescriptor(parameter.name, parameter.GetClrType(), !parameter.required.Value)
            {
                Configuration = actionDescriptor.ControllerDescriptor.Configuration,
                ActionDescriptor = actionDescriptor
            };
        }
    }
}