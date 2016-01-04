using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;

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

    public class ProductWithStringKey
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class ProductWithStringKeysController : ODataController
    {
        private static readonly ConcurrentDictionary<string, ProductWithStringKey> Data;

        static ProductWithStringKeysController()
        {
            Data = new ConcurrentDictionary<string, ProductWithStringKey>();
            var rand = new Random();

            Enumerable.Range(0, 100).Select(i => new ProductWithStringKey
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Product " + i,
                Price = rand.NextDouble() * 1000
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        public IHttpActionResult Put([FromODataUri] string key, [FromBody] ProductWithStringKey product)
        {
            key.Should().NotStartWith("'");
            key.Should().NotEndWith("'");

            return Updated(Data.Values.First());
        }
    }
}