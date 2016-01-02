using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;
using SwashbuckleODataSample.Models;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class StringKeyTests
    {
        [Test]
        public async Task It_supports_entities_with_keys_of_type_string_issue_34()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductWithStringKeysController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                var productToUpdate = new ProductWithStringKey
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Product 1",
                    Price = 2.30
                };
                // Verify that the OData route in the test controller is valid
                var response = await httpClient.PutAsJsonAsync($"/odata/ProductWithStringKeys('{productToUpdate.Id}')", productToUpdate);
                await response.ValidateSuccessAsync();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/ProductWithStringKeys('{Id}')", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.put.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            // Define a route to a controller class that contains functions
            config.MapODataServiceRoute("StringKeyTestsRoute", "odata", GetEdmModel());

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<ProductWithStringKey>("ProductWithStringKeys");

            return builder.GetEdmModel();
        }
    }
}