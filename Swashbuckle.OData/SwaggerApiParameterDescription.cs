using System.Web.Http.Description;

namespace Swashbuckle.OData
{
    public class SwaggerApiParameterDescription : ApiParameterDescription
    {
        public SwaggerApiParameterSource SwaggerSource { get; set; }
    }
}