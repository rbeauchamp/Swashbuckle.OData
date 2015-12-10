using System;
using System.Linq;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal class MapByParameterName : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetParameters()
                .SingleOrDefault(descriptor => string.Equals(descriptor.ParameterName, parameter.name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}