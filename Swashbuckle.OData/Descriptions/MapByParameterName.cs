using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class MapByParameterName : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter swaggerParameter, int parameterIndex, HttpActionDescriptor actionDescriptor)
        {
            var httpParameterDescriptors = actionDescriptor.GetParameters();
            Contract.Assume(httpParameterDescriptors != null);
            return httpParameterDescriptors
                .SingleOrDefault(descriptor => string.Equals(descriptor.ParameterName, swaggerParameter.name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}