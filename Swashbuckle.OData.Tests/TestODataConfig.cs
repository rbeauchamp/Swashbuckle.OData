using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Restier.EntityFramework;
using Microsoft.Restier.WebApi;
using Microsoft.Restier.WebApi.Batch;
using Swashbuckle.OData;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.Versioning;

namespace SwashbuckleODataSample
{
    public static class TestODataConfig
    {
        public const string ODataRoutePrefix = "odata";

        public static void Register(HttpConfiguration config, HttpServer server)
        {
            ConfigureWebApiOData(config);
            ConfigureRestierOData(config, server);
        }

        private static async void ConfigureRestierOData(HttpConfiguration config, HttpServer server)
        {
            await config.MapRestierRoute<DbApi<TestRestierODataContext>>("RESTierRoute", "restier", new RestierBatchHandler(server));
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

            // Define a custom route with custom routing conventions
            var conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new CustomNavigationPropertyRoutingConvention());
            var customODataRoute = config.MapODataServiceRoute("CustomODataRoute", ODataRoutePrefix, GetModel(), batchHandler: null, pathHandler: new DefaultODataPathHandler(), routingConventions: conventions);
            config.AddCustomSwaggerRoute(customODataRoute, "/Customers({Id})/Orders")
                .Operation(HttpMethod.Post)
                .PathParameter<int>("Id")
                .BodyParameter<Order>("order");

            // Define a default non-versioned route (default route should be at the end as a last catch-all)
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