using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class FromUriArrayTests
    {
        [Test]
        public async Task It_supports_uris_that_contain_arrays()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(Products2Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route is valid
                var products = await httpClient.GetJsonAsync<ODataResponse<Product2>>("/odata/ProductsWithIds(Ids=[0,1])");
                products.Should().NotBeNull();
                products.Value.Count.Should().Be(2);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/ProductsWithIds(Ids=[{Ids}])", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            // Define a route to a controller class that contains functions
            config.MapODataServiceRoute("FromUriArrayRoute", "odata", GetEdmModel());

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Product2>("Products");

            builder.Function("ProductsWithIds")
                   .ReturnsCollectionFromEntitySet<Product2>("Products")
                   .CollectionParameter<int>("Ids");

            return builder.GetEdmModel();
        }
    }

    public class Product2
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class Products2Controller : ODataController
    {
        private static readonly ConcurrentDictionary<int, Product2> Data;

        static Products2Controller()
        {
            Data = new ConcurrentDictionary<int, Product2>();
            var rand = new Random();

            Enumerable.Range(0, 100).Select(i => new Product2
            {
                Id = i,
                Name = "Product " + i,
                Price = rand.NextDouble() * 1000
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        [HttpGet]
        [ODataRoute("ProductsWithIds(Ids={Ids})")]
        public IQueryable<Product2> ProductsWithIds([FromODataUri]int[] Ids)
        {
            return Data.Values.Where(p => Ids.Contains(p.Id)).AsQueryable();
        }
    }
}