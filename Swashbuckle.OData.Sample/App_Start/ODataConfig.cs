using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.Versioning;

namespace SwashbuckleODataSample
{
    public static class ODataConfig
    {
        public const string ODataRoutePrefix = "odata";

        public static void Register(HttpConfiguration config)
        {
            var controllerSelector = new ODataVersionControllerSelector(config);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            // Define a versioned route
            config.MapODataServiceRoute("V1RouteVersioning", "odata/v1", GetModel());
            controllerSelector.RouteVersionSuffixMapping.Add("V1RouteVersioning", "V1");

            // Define a default non-versioned route
            config.MapODataServiceRoute("DefaultODataRoute", ODataRoutePrefix, GetModel());
        }

        private static IEdmModel GetModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }
    }
}