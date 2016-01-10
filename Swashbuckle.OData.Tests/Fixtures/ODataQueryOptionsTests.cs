using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using System.Web.OData.Query;

namespace Swashbuckle.OData.Tests.ODataQueryOptions
{
    [TestFixture]
    public class ODataQueryOptionsTests
    {
        [Test]
        public async Task It_supports_controller_with_single_get_method_with_odataqueryoptions()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(Products3Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var products = await httpClient.GetJsonAsync<ODataResponse<List<Product3>>>("/odata/Products3");
                products.Should().NotBeNull();
                products.Value.Count.Should().Be(100);
                var product = await httpClient.GetJsonAsync<ODataResponse<List<Product3>>>("/odata/Products3(1)");
                product.Should().NotBeNull();
                product.Value.Count.Should().Be(100);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Products3", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                PathItem pathItem2;
                swaggerDocument.paths.TryGetValue("/odata/Products3({Id})", out pathItem2);
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

            builder.EntitySet<Product3>("Products3");

            return builder.GetEdmModel();
        }
    }

    public class Product3
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class Products3Controller : ODataController
    {
        private static readonly ConcurrentDictionary<int, Product3> Data;

        static Products3Controller()
        {
            Data = new ConcurrentDictionary<int, Product3>();
            var rand = new Random();

            Enumerable.Range(0, 100).Select(i => new Product3
            {
                Id = i,
                Name = "Product " + i,
                Price = rand.NextDouble() * 1000
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        [EnableQuery]
        public Task<IHttpActionResult> Get(ODataQueryOptions<Product3> queryOptions)
        {
            var results = (IQueryable<Product3>)queryOptions.ApplyTo(Data.Values.AsQueryable());
            return Task.FromResult((IHttpActionResult)Ok(results));
        }
    }
}