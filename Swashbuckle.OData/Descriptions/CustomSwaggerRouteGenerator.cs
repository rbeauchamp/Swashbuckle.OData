using Microsoft.AspNet.OData.Routing;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;

namespace Swashbuckle.OData.Descriptions
{
    internal class CustomSwaggerRouteGenerator : ISwaggerRouteGenerator
    {
        public IEnumerable<SwaggerRoute> Generate(HttpConfiguration httpConfig)
        {
            return httpConfig.GetODataRoutes().SelectMany(oDataRoute => GetCustomSwaggerRoutes(httpConfig, oDataRoute));
        }

        public static List<SwaggerRoute> GetCustomSwaggerRoutes(HttpConfiguration httpConfig, ODataRoute oDataRoute)
        {
            Contract.Requires(httpConfig != null);
            Contract.Requires(oDataRoute != null);
            Contract.Requires(httpConfig.Properties != null);
            Contract.Ensures(Contract.Result<List<SwaggerRoute>>() != null);

            object swaggerRoutes;
            httpConfig.Properties.TryGetValue(oDataRoute, out swaggerRoutes);

            return swaggerRoutes as List<SwaggerRoute> ?? new List<SwaggerRoute>();
        }
    }
}