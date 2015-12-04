using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public class SwaggerToApiExplorerMapper
    {
        private readonly IEnumerable<IParameterMapper> _parameterMappers;

        public SwaggerToApiExplorerMapper(IEnumerable<IParameterMapper> parameterMappers)
        {
            Contract.Requires(parameterMappers != null);

            _parameterMappers = parameterMappers;
        }

        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            return _parameterMappers
                .Select(mapper => mapper.Map(parameter, index, actionDescriptor))
                .FirstOrDefault(httpParameterDescriptor => httpParameterDescriptor != null);
        }
    }
}