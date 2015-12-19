using System;
using System.Linq;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public class MapByParameterName : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter swaggerParameter, int parameterIndex, HttpActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetParameters()
                .SingleOrDefault(descriptor => string.Equals(descriptor.ParameterName, swaggerParameter.name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}