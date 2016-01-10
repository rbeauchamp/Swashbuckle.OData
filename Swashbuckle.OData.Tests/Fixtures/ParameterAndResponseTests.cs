using System;
using System.Collections.Concurrent;
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
    public class ParameterAndResponseTests
    {
        [Test]
        public async Task It_supports_functions_with_a_long_parameter()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ParameterTestsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var result = await httpClient.GetJsonAsync<ODataResponse<long>>("/odata/ParameterTests/Default.Long(longParam=2147483648)");
                result.Value.Should().Be(2147483648);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/ParameterTests/Default.Long(longParam={longParam})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
                var getResponse = pathItem.get.responses.SingleOrDefault(response => response.Key == "200");
                getResponse.Should().NotBeNull();
                getResponse.Value.schema.@ref.Should().Be("#/definitions/ODataResponse[Int64]");
                swaggerDocument.definitions.Should().ContainKey("ODataResponse[Int64]");
                var responseSchema = swaggerDocument.definitions["ODataResponse[Int64]"];
                responseSchema.Should().NotBeNull();
                responseSchema.properties.Should().NotBeNull();
                responseSchema.properties.Should().ContainKey("@odata.context");
                responseSchema.properties["@odata.context"].type.Should().Be("string");
                responseSchema.properties["value"].type.Should().Be("integer");
                responseSchema.properties["value"].format.Should().Be("int64");
                responseSchema.properties["value"].items.Should().BeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_entity_with_a_long_key()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ParameterTestsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var result = await httpClient.GetJsonAsync<ParameterTest>("/odata/ParameterTests(1)");
                result.Should().NotBeNull();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/ParameterTests({Id})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

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

            builder.EntitySet<ParameterTest>("ParameterTests");

            var productType = builder.EntityType<ParameterTest>();

            productType.Collection.Function("Long").Returns<long>().Parameter<long>("longParam");

            return builder.GetEdmModel();
        }
    }

    public class ParameterTest
    {
        [Key]
        public long Id { get; set; }
    }

    public class ParameterTestsController : ODataController
    {
        private static readonly ConcurrentDictionary<long, ParameterTest> Data;

        static ParameterTestsController()
        {
            Data = new ConcurrentDictionary<long, ParameterTest>();

            Enumerable.Range(0, 5).Select(i => new ParameterTest
            {
                Id = i
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(Data.Values.AsQueryable());
        }

        [HttpGet]
        [ResponseType(typeof(long))]
        public IHttpActionResult Long(long longParam)
        {
            return Ok(longParam);
        }
    }
}