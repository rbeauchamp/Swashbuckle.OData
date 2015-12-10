using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    public class ApplyNewHostName : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.host = "foo";
        }
    }
}