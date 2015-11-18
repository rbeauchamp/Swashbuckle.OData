using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using SwashbuckleODataSample.Models;

namespace SwashbuckleODataSample
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            var edmModel = builder.GetEdmModel();
            config.MapODataServiceRoute("odata", "odata", edmModel);

            SwaggerConfig.Register(edmModel);
        }
    }
}