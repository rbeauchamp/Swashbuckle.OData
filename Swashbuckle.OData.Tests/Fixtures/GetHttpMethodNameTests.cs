using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    /// <summary>
    /// Tests the scenario where the controller GET action names don't include the Entity name.
    /// </summary>
    [TestFixture]
    public class GetHttpMethodNameTests
    {
        [Test]
        public async Task It_supports_get_by_id_action_with_http_method_name_issue_28_problem_1()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(Products1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var product = await httpClient.GetJsonAsync<Product1>("/odata/Products1(1)");
                product.Should().NotBeNull();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Products1({Id})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_get_action_with_http_method_name_issue_28_problem_2()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(Products1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var products = await httpClient.GetJsonAsync<ODataResponse<Product1>>("/odata/Products1");
                products.Should().NotBeNull();
                products.Value.Should().NotBeNull();
                products.Value.Count.Should().BeGreaterThan(0);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Products1", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_generates_a_single_get_path_issue_28_problem_3()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(Products1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.paths.Count.Should().Be(2);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Product1>("Products1");

            return builder.GetEdmModel();
        }
    }

    public class Product1
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class Products1Controller : ODataController
    {
        private static readonly ConcurrentDictionary<int, Product1> Data;

        static Products1Controller()
        {
            Data = new ConcurrentDictionary<int, Product1>();
            var rand = new Random();

            Enumerable.Range(0, 100).Select(i => new Product1
            {
                Id = i,
                Name = "Product " + i,
                Price = rand.NextDouble() * 1000
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        [EnableQuery]
        public Task<IHttpActionResult> Get(ODataQueryOptions<Product1> queryOptions)
        {
            var results = (IQueryable<Product1>)queryOptions.ApplyTo(Data.Values.AsQueryable());
            return Task.FromResult((IHttpActionResult) Ok(results));
        }

        [EnableQuery]
        public Task<IHttpActionResult> Get(int key)
        {
            var result = Data.Values.SingleOrDefault(product => product.Id == key);
            return result == null 
                ? Task.FromResult((IHttpActionResult) NotFound()) 
                : Task.FromResult((IHttpActionResult) Ok(result));
        }
    }
}