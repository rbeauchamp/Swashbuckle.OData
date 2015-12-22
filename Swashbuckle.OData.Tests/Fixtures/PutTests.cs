using System;
using System.Threading.Tasks;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
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
    public class PutTests
    {
        [Test]
        public async Task It_includes_a_put_operation()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(CustomersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers({Id})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.put.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_does_not_exist_if_not_in_the_controller()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(OrdersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Orders({OrderId})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.put.Should().BeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            // Define a default non- versioned route(default route should be at the end as a last catch-all)
            config.MapODataServiceRoute("DefaultODataRoute", "odata", GetDefaultModel());

            config.EnsureInitialized();
        }

        private static IEdmModel GetDefaultModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }
    }
}