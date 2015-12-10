using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal interface IParameterMapper
    {
        HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor);
    }
}