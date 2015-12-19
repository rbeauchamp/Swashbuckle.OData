using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal static class SwaggerDocsConfigExtensions
    {
        public static T GetFieldValue<T>(this SwaggerDocsConfig swaggerDocsConfig, string fieldName)
        {
            Contract.Requires(swaggerDocsConfig != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(fieldName));

            return swaggerDocsConfig.GetInstanceField<T>(fieldName);
        }

        public static Dictionary<string, SecurityScheme> GetSecurityDefinitions(this SwaggerDocsConfig swaggerDocsConfig)
        {
            var securitySchemeBuilders = swaggerDocsConfig.GetFieldValue<IDictionary<string, SecuritySchemeBuilder>>("_securitySchemeBuilders");

            return securitySchemeBuilders.Any()
                ? securitySchemeBuilders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.InvokeFunction<SecurityScheme>("Build"))
                : null;
        }
    }
}