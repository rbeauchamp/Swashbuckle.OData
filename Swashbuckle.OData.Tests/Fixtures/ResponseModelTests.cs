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
    public class ResponseModelTests
    {
        [Test]
        public async Task It_produces_an_accurate_odata_response_model_for_iqueryable_return_type()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductResponsesController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<ProductResponse>>("/odata/ProductResponses");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/ProductResponses", out pathItem);
                pathItem.get.Should().NotBeNull();
                pathItem.get.produces.Should().NotBeNull();
                pathItem.get.produces.Count.Should().Be(1);
                pathItem.get.produces.First().Should().Be("application/json");
                var getResponse = pathItem.get.responses.SingleOrDefault(response => response.Key == "200");
                getResponse.Should().NotBeNull();
                getResponse.Value.schema.@ref.Should().Be("#/definitions/ODataResponse[ProductResponse]");
                swaggerDocument.definitions.Should().ContainKey("ODataResponse[ProductResponse]");
                var responseSchema = swaggerDocument.definitions["ODataResponse[ProductResponse]"];
                responseSchema.Should().NotBeNull();
                responseSchema.properties.Should().NotBeNull();
                responseSchema.properties.Should().ContainKey("@odata.context");
                responseSchema.properties["@odata.context"].type.Should().Be("string");
                responseSchema.properties["value"].type.Should().Be("array");
                responseSchema.properties["value"].items.Should().NotBeNull();
                responseSchema.properties["value"].items.@ref.Should().Be("#/definitions/ProductResponse");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_produces_an_accurate_odata_response_model_for_list_return_type()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductResponsesController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var top10Response = await httpClient.GetJsonAsync<ODataResponse<Product5>>("/odata/ProductResponses/Default.Top10()");
                top10Response.Value.Should().NotBeNull();
                top10Response.Value.Count.Should().Be(10);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/ProductResponses/Default.Top10()", out pathItem);
                var getResponse = pathItem.get.responses.SingleOrDefault(response => response.Key == "200");
                getResponse.Should().NotBeNull();
                getResponse.Value.schema.@ref.Should().Be("#/definitions/ODataResponse[ProductResponse]");
                swaggerDocument.definitions.Should().ContainKey("ODataResponse[ProductResponse]");
                var responseSchema = swaggerDocument.definitions["ODataResponse[ProductResponse]"];
                responseSchema.Should().NotBeNull();
                responseSchema.properties.Should().NotBeNull();
                responseSchema.properties.Should().ContainKey("@odata.context");
                responseSchema.properties["@odata.context"].type.Should().Be("string");
                responseSchema.properties["value"].type.Should().Be("array");
                responseSchema.properties["value"].items.Should().NotBeNull();
                responseSchema.properties["value"].items.@ref.Should().Be("#/definitions/ProductResponse");

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

            builder.EntitySet<ProductResponse>("ProductResponses");

            var productType = builder.EntityType<ProductResponse>();

            // A function that returns a list
            productType.Collection
                .Function("Top10")
                .ReturnsCollectionFromEntitySet<ProductResponse>("ProductResponses");

            return builder.GetEdmModel();
        }
    }

    public class ProductResponse
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class ProductResponsesController : ODataController
    {
        private static readonly ConcurrentDictionary<string, ProductResponse> Data;

        static ProductResponsesController()
        {
            Data = new ConcurrentDictionary<string, ProductResponse>();
            var rand = new Random();

            Enumerable.Range(0, 20).Select(i => new ProductResponse
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Product " + i,
                Price = rand.NextDouble() * 1000
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        /// <summary>
        /// An action that returns an IQueryable[]
        /// </summary>
        [EnableQuery]
        public IQueryable<ProductResponse> Get()
        {
            return Data.Values.AsQueryable();
        }

        /// <summary>
        /// A function that returns a List[]
        /// </summary>
        [HttpGet]
        [EnableQuery]
        [ResponseType(typeof(List<ProductResponse>))]
        public IHttpActionResult Top10()
        {
            var retval = Data.Values.OrderByDescending(p => p.Price).Take(10).ToList();

            return Ok(retval);
        }
    }
}