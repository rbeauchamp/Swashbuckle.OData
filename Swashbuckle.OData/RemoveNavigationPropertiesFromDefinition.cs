using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    /// <summary>
    /// Limits the object graph to the top level entity.
    /// </summary>
    internal class RemoveNavigationPropertiesFromDefinition : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var definition in swaggerDoc.definitions)
            {
                if (IsEntityType(definition))
                {
                    var schema = definition.Value;
                    Contract.Assume(schema != null);

                    var properties = schema.properties.ToList();
                    foreach (var property in schema.properties)
                    {
                        RemoveCollectionTypeProperty(property, properties);
                        RemoveReferenceTypeProperty(property, properties);
                    }
                    schema.properties = properties.ToDictionary(property => property.Key, property => property.Value);
                }
            }
        }

        private static bool IsEntityType(KeyValuePair<string, Schema> definition)
        {
            return !definition.Key.Contains("ODataResponse[");
        }

        private static void RemoveCollectionTypeProperty(KeyValuePair<string, Schema> property, ICollection<KeyValuePair<string, Schema>> properties)
        {
            Contract.Requires(properties != null);

            Contract.Assume(property.Value != null);

            Contract.Assume(property.Value.type != "array" || property.Value?.items != null);
            if (property.Value.type == "array" && property.Value.items.@ref != null)
            {
                properties.Remove(property);
            }
        }

        private static void RemoveReferenceTypeProperty(KeyValuePair<string, Schema> property, ICollection<KeyValuePair<string, Schema>> properties)
        {
            Contract.Requires(property.Value != null);
            Contract.Requires(property.Value.type != null || property.Value.@ref == null || properties != null);

            if (property.Value.type == null && property.Value.@ref != null)
            {
                properties.Remove(property);
            }
        }
    }
}