using System;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using SwashbuckleODataSample;
using SwashbuckleODataSample.ApiControllers;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.ODataControllers;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class ODataSwaggerProviderTests
    {
        [Test]
        public async Task It_applies_document_filters()
        {
            // Arrange
            Action<SwaggerDocsConfig> config = c => c.DocumentFilter<ApplyNewHostName>();
            var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

            using (WebApp.Start(HttpClientUtils.BaseAddress, builder => Configuration(builder, swaggerDocsConfig: config)))
            {
                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.host.Should().Be("foo");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_multiple_odata_routes()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, builder => Configuration(builder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_odata_routes_that_dont_map_to_a_controller()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, builder => Configuration(builder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_explores_the_correct_controller()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, builder => Configuration(builder, typeof(CustomersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem defaultCustomerController;
                swaggerDocument.paths.TryGetValue("/odata/Customers({Id})", out defaultCustomerController);
                defaultCustomerController.Should().NotBeNull();
                defaultCustomerController.put.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_explores_the_correct_versioned_controller()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, builder => Configuration(builder, typeof(CustomersV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem versionedCustomerController;
                swaggerDocument.paths.TryGetValue("/odata/v1/Customers({Id})", out versionedCustomerController);
                versionedCustomerController.Should().NotBeNull();
                versionedCustomerController.put.Should().BeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_both_webapi_and_odata_controllers()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, builder => Configuration(builder, typeof(ClientsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem clientsWebApi;
                swaggerDocument.paths.TryGetValue("/api/Clients", out clientsWebApi);
                clientsWebApi.Should().NotBeNull();
                clientsWebApi.get.Should().NotBeNull();
                clientsWebApi.patch.Should().BeNull();

                PathItem clientWebApi;
                swaggerDocument.paths.TryGetValue("/api/Clients/{id}", out clientWebApi);
                clientWebApi.Should().NotBeNull();
                clientWebApi.put.Should().NotBeNull();
                clientWebApi.patch.Should().BeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController = null, Action<SwaggerDocsConfig> swaggerDocsConfig = null)
        {
            var config = appBuilder.GetStandardHttpConfig(swaggerDocsConfig, null, targetController);

            var controllerSelector = new UnitTestODataVersionControllerSelector(config, targetController);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            WebApiConfig.Register(config);

            // Define a versioned route
            config.MapODataServiceRoute("V1RouteVersioning", "odata/v1", GetVersionedModel());
            controllerSelector.RouteVersionSuffixMapping.Add("V1RouteVersioning", "V1");

            // Define a versioned route that doesn't map to any controller
            config.MapODataServiceRoute("odata/v2", "odata/v2", GetFakeModel());
            controllerSelector.RouteVersionSuffixMapping.Add("odata/v2", "V2");

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
    }
}