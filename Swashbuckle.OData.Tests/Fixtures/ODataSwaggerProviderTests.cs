using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.OData.Tests.WebHost;
using Swashbuckle.Swagger;
using SwashbuckleODataSample;

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
            var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder, config)))
            {
                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.host.Should().Be("foo");
            }
        }

        [Test]
        public async Task It_supports_multiple_odata_routes()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.Should().NotBeNull();
            }
        }

        [Test]
        public async Task It_supports_odata_routes_that_dont_map_to_a_controller()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.Should().NotBeNull();
            }
        }

        [Test]
        public async Task It_explores_the_correct_controller()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem versionedCustomerController;
                swaggerDocument.paths.TryGetValue("/odata/v1/Customers({Id})", out versionedCustomerController);
                versionedCustomerController.Should().NotBeNull();
                versionedCustomerController.put.Should().BeNull();

                PathItem defaultCustomerController;
                swaggerDocument.paths.TryGetValue("/odata/Customers({Id})", out defaultCustomerController);
                defaultCustomerController.Should().NotBeNull();
                defaultCustomerController.put.Should().NotBeNull();
            }
        }

        [Test]
        public async Task It_supports_both_webapi_and_odata_controllers()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

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
            }
        }

        [Test]
        public async Task It_generates_valid_swagger_2_0_json()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var response = await httpClient.GetAsync("swagger/docs/v1");

                // Assert
                await response.ValidateSuccessAsync();
                var swaggerJson = await response.Content.ReadAsStringAsync();

                var resolver = new JSchemaPreloadedResolver();
                resolver.Add(new Uri("http://json-schema.org/draft-04/schema"), File.ReadAllText(@"schema-draft-v4.json"));

                var swaggerSchema = File.ReadAllText(@"swagger-2.0-schema.json");
                var schema = JSchema.Parse(swaggerSchema, resolver);

                var swaggerJObject = JObject.Parse(swaggerJson);
                IList<string> messages;
                var isValid = swaggerJObject.IsValid(schema, out messages);
                isValid.Should().BeTrue();
            }
        }
    }
}