using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Routing;
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

        private static readonly MethodInfo GetODataRootContainerMethod = typeof(Microsoft.AspNet.OData.Extensions.HttpConfigurationExtensions).GetMethod("GetODataRootContainer", BindingFlags.Static | BindingFlags.NonPublic);

        // We need access to the root container but System.Web.OData.Extensions.HttpConfigurationExtensions.GetODataRootContainer is internal.
        public static IServiceProvider GetODataRootContainer(this HttpConfiguration configuration, ODataRoute oDataRoute)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(oDataRoute != null);
            
            return (IServiceProvider)GetODataRootContainerMethod.Invoke(null, new object[] {configuration, oDataRoute.PathRouteConstraint.RouteName});
        }

        public static SwaggerRouteBuilder AddCustomSwaggerRoute(this HttpConfiguration httpConfig, ODataRoute oDataRoute, string routeTemplate)
        {
            Contract.Requires(httpConfig != null);
            Contract.Requires(oDataRoute != null);
            Contract.Requires(httpConfig.Properties != null);
            Contract.Ensures(Contract.Result<SwaggerRouteBuilder>() != null);

            oDataRoute.SetHttpConfiguration(httpConfig);

            var urlDecodedTemplate = HttpUtility.UrlDecode(routeTemplate);
            Contract.Assume(!string.IsNullOrWhiteSpace(urlDecodedTemplate));

            var swaggerRoute = new SwaggerRoute(urlDecodedTemplate, oDataRoute);

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