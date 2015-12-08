using System.Web.Http.Description;

namespace Swashbuckle.OData
{
    internal class SwaggerApiParameterDescription : ApiParameterDescription
    {
        public SwaggerApiParameterSource SwaggerSource { get; set; }
    }
}