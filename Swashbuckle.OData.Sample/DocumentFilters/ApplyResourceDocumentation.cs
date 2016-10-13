using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace SwashbuckleODataSample.DocumentFilters
{
    /// <summary>
    /// Applies top-level Swagger documentation to the resources.
    /// </summary>
    public class ApplyResourceDocumentation : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.tags = new List<Tag>
            {
                new Tag { name = "Clients", description = "an ApiController resource" },
                new Tag { name = "Customers", description = "an ODataController resource" },
                new Tag { name = "Orders", description = "an ODataController resource" },
                new Tag { name = "CustomersV1", description = "a versioned ODataController resource" },
                new Tag { name = "Users", description = "a RESTier resource" },
                new Tag { name = "Products", description = "demonstrates OData functions and actions" },
                new Tag { name = "ProductWithCompositeEnumIntKeys", description = "demonstrates composite keys with an enum as a key" },
                new Tag { name = "ProductWithEnumKeys", description = "demonstrates use of enum as a key" },
            };
        }
    }
}