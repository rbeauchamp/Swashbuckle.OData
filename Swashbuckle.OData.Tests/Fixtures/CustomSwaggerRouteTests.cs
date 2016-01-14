using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;
using SwashbuckleODataSample;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.ODataControllers;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class CustomSwaggerRouteTests
    {
        [Test]
        public async Task It_allows_definition_of_custom_routes()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, builder => Configuration(builder, typeof(OrdersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers({Id})/Orders", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.post.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_allows_definition_of_custom_delete_routes()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, builder => Configuration(builder, typeof(OrdersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers({Id})/Orders({orderID})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.delete.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            // Define a custom route with custom routing conventions
            var conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new CustomNavigationPropertyRoutingConvention());
            var customODataRoute = config.MapODataServiceRoute("CustomODataRoute", "odata", GetCustomRouteModel(), batchHandler: null, pathHandler: new DefaultODataPathHandler(), routingConventions: conventions);
            config.AddCustomSwaggerRoute(customODataRoute, "/Customers({Id})/Orders")
                .Operation(HttpMethod.Post)
                .PathParameter<int>("Id")
                .BodyParameter<Order>("order");

            config.AddCustomSwaggerRoute(customODataRoute, "/Customers({Id})/Orders({orderID})")
                .Operation(HttpMethod.Delete)
                .PathParameter<int>("Id")
                .PathParameter<Guid>("orderID");

            config.EnsureInitialized();
        }

        private static IEdmModel GetCustomRouteModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }
    }
}