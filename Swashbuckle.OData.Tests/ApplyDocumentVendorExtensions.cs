using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    public class ApplyDocumentVendorExtensions : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.host = "foo";
        }
    }
}