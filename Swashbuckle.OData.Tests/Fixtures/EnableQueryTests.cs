using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
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
    public class EnableQueryTests
    {
        [Test]
        public async Task It_provides_query_options_for_methods_that_have_an_EnableQuery_attribute()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(Product5sController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Verify that the OData route in the test controller is valid
                var response = await httpClient.GetJsonAsync<ODataResponse<List<Product5>>>("/odata/Product5s/Default.Top10()");
                response.Value.Should().NotBeNull();
                response.Value.Count.Should().Be(10);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Product5s/Default.Top10()", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem?.get.Should().NotBeNull();
                pathItem?.get.parameters.Should().NotBeNull();

                var filterParameter = pathItem.get.parameters.SingleOrDefault(parameter => parameter.name == "$filter");
                filterParameter.Should().NotBeNull();
                filterParameter?.description.Should().NotBeNullOrWhiteSpace();
                filterParameter?.type.ShouldBeEquivalentTo("string");
                filterParameter?.@in.ShouldBeEquivalentTo("query");
                pathItem.get.parameters.Where(parameter => parameter.name.StartsWith("$")).Should().OnlyContain(parameter => parameter.required == false);
                pathItem.get.parameters.Count(parameter => parameter.name.StartsWith("$")).Should().Be(7);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            // Define a route to a controller class that contains functions
            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Product5>("Product5s");

            var productType = builder.EntityType<Product5>();

            // Function that has an [EnableQuery] attribute applied
            productType.Collection
                .Function("Top10")
                .ReturnsCollectionFromEntitySet<Product5>("Product5s");

            return builder.GetEdmModel();
        }
    }

    public class Product5
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class Product5sController : ODataController
    {
        private static readonly ConcurrentDictionary<string, Product5> Data;

        static Product5sController()
        {
            Data = new ConcurrentDictionary<string, Product5>();
            var rand = new Random();

            Enumerable.Range(0, 20).Select(i => new Product5
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Product " + i,
                Price = rand.NextDouble() * 1000
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        [HttpGet]
        [EnableQuery]
        [ResponseType(typeof(List<Product5>))]
        public IHttpActionResult Top10()
        {
            var retval = Data.Values.OrderByDescending(p => p.Price).Take(10).ToList();

            return Ok(retval);
        }
    }
}