using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Web.OData.Routing;
using Flurl;
using Newtonsoft.Json;
using Swashbuckle.OData.Descriptions;

namespace Swashbuckle.OData
{
    public static class HttpConfigurationExtensions
    {
        internal static JsonSerializerSettings SerializerSettingsOrDefault(this HttpConfiguration httpConfig)
        {
            var formatter = httpConfig.Formatters.JsonFormatter;
            return formatter != null 
                ? formatter.SerializerSettings 
                : new JsonSerializerSettings();
        }

        public static SwaggerRouteBuilder AddCustomSwaggerRoute(this HttpConfiguration httpConfig, ODataRoute oDataRoute, string routeTemplate)
        {
            var fullRouteTemplate = HttpUtility.UrlDecode(oDataRoute.RoutePrefix.AppendPathSegment(routeTemplate));

            var swaggerRoute = new SwaggerRoute(fullRouteTemplate);

            var swaggerRouteBuilder = new SwaggerRouteBuilder(swaggerRoute, oDataRoute);

            httpConfig.Properties.AddOrUpdate(oDataRoute, 
                key => new List<SwaggerRoute> { swaggerRoute }, 
                (key, value) =>
                {
                    var swaggerRoutes = value as List<SwaggerRoute>;
                    swaggerRoutes.Add(swaggerRoute);
                    return swaggerRoutes;
                });

            return swaggerRouteBuilder;
        }

        public static List<SwaggerRoute> GetCustomSwaggerRoutes(this HttpConfiguration httpConfig, ODataRoute oDataRoute)
        {
            object swaggerRoutes;
            httpConfig.Properties.TryGetValue(oDataRoute, out swaggerRoutes);

            return swaggerRoutes as List<SwaggerRoute> ?? new List<SwaggerRoute>();
        }
    }
}