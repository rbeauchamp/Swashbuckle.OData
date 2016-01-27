using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Routing;
using Flurl;
using Newtonsoft.Json;
using Swashbuckle.OData.Descriptions;

namespace Swashbuckle.OData
{
    public static class HttpConfigurationExtensions
    {
        internal static IEnumerable<ODataRoute> GetODataRoutes(this HttpConfiguration httpConfig)
        {
            Contract.Requires(httpConfig != null);

            return FlattenRoutes(httpConfig.Routes).OfType<ODataRoute>();
        }

        private static IEnumerable<IHttpRoute> FlattenRoutes(IEnumerable<IHttpRoute> routes)
        {
            foreach (var route in routes)
            {
                var nested = route as IEnumerable<IHttpRoute>;
                if (nested != null)
                {
                    foreach (var subRoute in FlattenRoutes(nested))
                    {
                        yield return subRoute;
                    }
                }
                else
                {
                    yield return route;
                }
            }
        }

        internal static JsonSerializerSettings SerializerSettingsOrDefault(this HttpConfiguration httpConfig)
        {
            Contract.Requires(httpConfig != null);

            var mediaTypeFormatterCollection = httpConfig.Formatters;
            Contract.Assume(mediaTypeFormatterCollection != null);

            var formatter = mediaTypeFormatterCollection.JsonFormatter;
            return formatter != null 
                ? formatter.SerializerSettings 
                : new JsonSerializerSettings();
        }

        public static SwaggerRouteBuilder AddCustomSwaggerRoute(this HttpConfiguration httpConfig, ODataRoute oDataRoute, string routeTemplate)
        {
            Contract.Requires(httpConfig != null);
            Contract.Requires(oDataRoute != null);
            Contract.Requires(httpConfig.Properties != null);
            Contract.Ensures(Contract.Result<SwaggerRouteBuilder>() != null);

            var fullRouteTemplate = HttpUtility.UrlDecode(oDataRoute.GetRoutePrefix().AppendPathSegment(routeTemplate));
            Contract.Assume(!string.IsNullOrWhiteSpace(fullRouteTemplate));

            var swaggerRoute = new SwaggerRoute(fullRouteTemplate, oDataRoute);

            var swaggerRouteBuilder = new SwaggerRouteBuilder(swaggerRoute);

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
    }
}