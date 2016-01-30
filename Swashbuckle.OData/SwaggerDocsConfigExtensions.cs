using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal static class SwaggerDocsConfigExtensions
    {
        public static T GetFieldValue<T>(this SwaggerDocsConfig swaggerDocsConfig, string fieldName, bool ensureNonNull = false)
        {
            Contract.Requires(swaggerDocsConfig != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(fieldName));
            Contract.Ensures(Contract.Result<T>() != null || !ensureNonNull);

            return swaggerDocsConfig.GetInstanceField<T>(fieldName, ensureNonNull);
        }

        public static Dictionary<string, SecurityScheme> GetSecurityDefinitions(this SwaggerDocsConfig swaggerDocsConfig)
        {
            var securitySchemeBuilders = swaggerDocsConfig.GetFieldValue<IDictionary<string, SecuritySchemeBuilder>>("_securitySchemeBuilders");
            Contract.Assume(securitySchemeBuilders != null);

            return securitySchemeBuilders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.InvokeFunction<SecurityScheme>("Build"));
        }
    }
}