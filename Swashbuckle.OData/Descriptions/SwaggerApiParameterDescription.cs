using System.Web.Http.Description;

namespace Swashbuckle.OData.Descriptions
{
    internal class SwaggerApiParameterDescription : ApiParameterDescription
    {
        public SwaggerApiParameterSource SwaggerSource { get; set; }
    }
}