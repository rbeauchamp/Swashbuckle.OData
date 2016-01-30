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
    public class DecimalParameterAndResponseTests
    {
        [Test]
        public async Task It_supports_entity_with_a_decimal_key()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(DecimalParametersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var result = await httpClient.GetJsonAsync<DecimalParameter>("/odata/DecimalParameters(2.3m)");
                result.Should().NotBeNull();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/DecimalParameters({Id})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_functions_with_a_decimal_parameter()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(DecimalParametersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var result = await httpClient.GetJsonAsync<ODataResponse<decimal>>("/odata/DecimalParameters/Default.ResponseTest(param=2.5m)");
                result.Value.Should().Be(2.5m);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/DecimalParameters/Default.ResponseTest(param={param})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
                var getResponse = pathItem.get.responses.SingleOrDefault(response => response.Key == "200");
                getResponse.Should().NotBeNull();
                getResponse.Value.schema.@ref.Should().Be("#/definitions/ODataResponse[Decimal]");
                swaggerDocument.definitions.Should().ContainKey("ODataResponse[Decimal]");
                var responseSchema = swaggerDocument.definitions["ODataResponse[Decimal]"];
                responseSchema.Should().NotBeNull();
                responseSchema.properties.Should().NotBeNull();
                responseSchema.properties.Should().ContainKey("@odata.context");
                responseSchema.properties["@odata.context"].type.Should().Be("string");
                responseSchema.properties["value"].type.Should().Be("number");
                responseSchema.properties["value"].format.Should().Be("decimal");
                responseSchema.properties["value"].items.Should().BeNull();

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

            builder.EntitySet<DecimalParameter>("DecimalParameters");

            var testType = builder.EntityType<DecimalParameter>();

            testType.Collection.Function("ResponseTest").Returns<decimal>().Parameter<decimal>("param");

            return builder.GetEdmModel();
        }
    }

    public class DecimalParameter
    {
        [Key]
        public decimal Id { get; set; }
    }

    public class DecimalParametersController : ODataController
    {
        private static readonly ConcurrentDictionary<decimal, DecimalParameter> Data;

        static DecimalParametersController()
        {
            Data = new ConcurrentDictionary<decimal, DecimalParameter>();

            var instance = new DecimalParameter
            {
                Id = 2.3m
            };

            Data.TryAdd(instance.Id, instance);
        }

        [EnableQuery]
        public SingleResult<DecimalParameter> GetDecimalParameter([FromODataUri] decimal key)
        {
            return SingleResult.Create(Data.Values.AsQueryable().Where(value => value.Id == key));
        }

        [HttpGet]
        [ResponseType(typeof(decimal))]
        public IHttpActionResult ResponseTest(decimal param)
        {
            return Ok(param);
        }
    }
}