using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Restier.EntityFramework;
using Microsoft.Restier.WebApi;
using Microsoft.Restier.WebApi.Batch;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.Repositories;
using SwashbuckleODataSample.Versioning;

namespace SwashbuckleODataSample
{
    public static class ODataConfig
    {
        public const string ODataRoutePrefix = "odata";

        public static void Register(HttpConfiguration config)
        {
            ConfigureWebApiOData(config);
            ConfigureRestierOData(config);
        }

        private static async void ConfigureRestierOData(HttpConfiguration config)
        {
            await config.MapRestierRoute<DbApi<RestierODataContext>>("RESTierRoute", "restier", new RestierBatchHandler(GlobalConfiguration.DefaultServer));
        }

        private static void ConfigureWebApiOData(HttpConfiguration config)
        {
            var controllerSelector = new ODataVersionControllerSelector(config);
            config.Services.Replace(typeof (IHttpControllerSelector), controllerSelector);

            // Define a versioned route
            config.MapODataServiceRoute("V1RouteVersioning", "odata/v1", GetModel());
            controllerSelector.RouteVersionSuffixMapping.Add("V1RouteVersioning", "V1");

            // Define a versioned route that doesn't map to any controller
            config.MapODataServiceRoute("odata/v2", "odata/v2", GetFakeModel());
            controllerSelector.RouteVersionSuffixMapping.Add("odata/v2", "V2");

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

        private static IEdmModel GetFakeModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("FakeCustomers");
            return builder.GetEdmModel();
        }
    }
}