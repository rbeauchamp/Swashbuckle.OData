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
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            // Define a versioned route
            config.MapODataServiceRoute("V1RouteVersioning", "odata/v1", GetVersionedModel());
            controllerSelector.RouteVersionSuffixMapping.Add("V1RouteVersioning", "V1");

            // Define a versioned route that doesn't map to any controller
            config.MapODataServiceRoute("odata/v2", "odata/v2", GetFakeModel());
            controllerSelector.RouteVersionSuffixMapping.Add("odata/v2", "V2");

            // Define a custom route with custom routing conventions
            var conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new CustomNavigationPropertyRoutingConvention());
            var customODataRoute = config.MapODataServiceRoute("CustomODataRoute", ODataRoutePrefix, GetCustomRouteModel(), batchHandler: null, pathHandler: new DefaultODataPathHandler(), routingConventions: conventions);
            config.AddCustomSwaggerRoute(customODataRoute, "/Customers({Id})/Orders")
                .Operation(HttpMethod.Post)
                .PathParameter<int>("Id")
                .BodyParameter<Order>("order");

            // Define a route to a controller class that contains functions
            config.MapODataServiceRoute("FunctionsODataRoute", ODataRoutePrefix, GetFunctionsEdmModel());

            // Define a default non- versioned route(default route should be at the end as a last catch-all)
            config.MapODataServiceRoute("DefaultODataRoute", ODataRoutePrefix, GetDefaultModel());
        }

        private static IEdmModel GetDefaultModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }

        private static IEdmModel GetCustomRouteModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }

        private static IEdmModel GetVersionedModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            return builder.GetEdmModel();
        }

        private static IEdmModel GetFakeModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("FakeCustomers");
            return builder.GetEdmModel();
        }

        private static IEdmModel GetFunctionsEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Product>("Products");

            var productType = builder.EntityType<Product>();

            // Function bound to a collection that accepts an enum parameter
            var enumParamFunction = productType.Collection.Function("GetByEnumValue");
            enumParamFunction.Parameter<MyEnum>("EnumValue");
            enumParamFunction.ReturnsCollectionFromEntitySet<Product>("Products");

            // Function bound to an entity set
            // Returns the most expensive product, a single entity
            productType.Collection
                .Function("MostExpensive")
                .Returns<double>();

            // Function bound to an entity set
            // Returns the top 10 product, a collection
            productType.Collection
                .Function("Top10")
                .ReturnsCollectionFromEntitySet<Product>("Products");

            // Function bound to a single entity
            // Returns the instance's price rank among all products
            productType
                .Function("GetPriceRank")
                .Returns<int>();

            // Function bound to a single entity
            // Accept a string as parameter and return a double
            // This function calculate the general sales tax base on the 
            // state
            productType
                .Function("CalculateGeneralSalesTax")
                .Returns<double>()
                .Parameter<string>("state");

            // Function bound to an entity set
            // Accepts an array as a parameter
            productType.Collection
                .Function("ProductsWithIds")
                .ReturnsCollectionFromEntitySet<Product>("Products")
                .CollectionParameter<int>("Ids");

            // An action bound to an entity set
            // Accepts multiple action parameters
            var action = productType.Collection.Action("Create");
                action.ReturnsFromEntitySet<Product>("Products");
                action.Parameter<string>("Name").OptionalParameter = false;
                action.Parameter<double>("Price").OptionalParameter = false;
                action.Parameter<MyEnum>("EnumValue").OptionalParameter = false;

            // An action bound to an entity
            productType.Action("Rate")
                .Parameter<int>("Rating");

            return builder.GetEdmModel();
        }
    }
}