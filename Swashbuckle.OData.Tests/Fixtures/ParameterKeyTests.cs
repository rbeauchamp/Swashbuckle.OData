using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing.Conventions;
using FluentAssertions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;
using SwashbuckleODataSample.Models;


namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class ParameterKeyTests
    {
        private const string ODataRoutePrefix = "odata";
        private const string EnumKeyEndpointName = "ProductWithEnumKeysTest";
        private const string CompositeKeyEndpointName = "ProductWithCompositeEnumIntKeysTest";

        private const string EnumNamespace = "SwashbuckleODataSample.Models.MyEnum";
        private const string EnumKeyName = "enumValue";
        private const string IdKeyName = "id";

        [Test]
        public async Task It_supports_enum_key_issue_108()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress,
                                appBuilder => ConfigurationEnumKey(appBuilder, true))
            )
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Verify that the OData route in the test controller is valid
                var response = await httpClient.GetAsync($"/{ODataRoutePrefix}/{EnumKeyEndpointName}");
                await response.ValidateSuccessAsync();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                var testEndpoint = $"/{ODataRoutePrefix}/{EnumKeyEndpointName}('{{{EnumKeyName}}}')";
                swaggerDocument.paths.TryGetValue(testEndpoint, out pathItem);
                pathItem.Should().NotBeNull();

                // Assert Enum Pararemter
                var enumParamter = pathItem?.@get.parameters.SingleOrDefault(p => p.name == EnumKeyName);
                enumParamter.Should().NotBeNull();
                enumParamter?.@enum.Should().NotBeEmpty();
                enumParamter?.@in.Should().Be("path");
                enumParamter?.@type.Should().Be("string");
                enumParamter?.required.ShouldBeEquivalentTo(true);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_enum_as_key_with_enum_prefix_issue_108()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress,
                                appBuilder => ConfigurationEnumKey(appBuilder, false))
            )
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Verify that the OData route in the test controller is valid
                var response = await httpClient.GetAsync($"/{ODataRoutePrefix}/{EnumKeyEndpointName}");
                await response.ValidateSuccessAsync();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                var testEndpoint = $"/{ODataRoutePrefix}/{EnumKeyEndpointName}({EnumNamespace}'{{{EnumKeyName}}}')";
                swaggerDocument.paths.TryGetValue(testEndpoint, out pathItem);
                pathItem.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_composite_key_with_enum_issue_108()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress,
                                appBuilder => ConfigurationCompositeKey(appBuilder, true))
            )
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Verify that the OData route in the test controller is valid
                var response = await httpClient.GetAsync($"/{ODataRoutePrefix}/{CompositeKeyEndpointName}");
                await response.ValidateSuccessAsync();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                var testEndpoint = $"/{ODataRoutePrefix}/{CompositeKeyEndpointName}({IdKeyName}={{{IdKeyName}}},{EnumKeyName}='{{{EnumKeyName}}}')";
                swaggerDocument.paths.TryGetValue(testEndpoint, out pathItem);
                pathItem.Should().NotBeNull();

                // Assert Enum Pararemter
                var enumParamter = pathItem?.@get.parameters.SingleOrDefault(p => p.name == EnumKeyName);
                enumParamter.Should().NotBeNull();
                enumParamter?.@enum.Should().NotBeEmpty();
                enumParamter?.@in.Should().Be("path");
                enumParamter?.@type.Should().Be("string");
                enumParamter?.required.ShouldBeEquivalentTo(true);

                // Assert Id Pararemter
                var idParamter = pathItem?.@get.parameters.SingleOrDefault(p => p.name == IdKeyName);
                idParamter.Should().NotBeNull();
                idParamter?.@in.Should().Be("path");
                idParamter?.@type.Should().Be("integer");
                idParamter?.required.ShouldBeEquivalentTo(true);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_composite_key_with_enum_no_enum_prefix_issue_108()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress,
                                appBuilder => ConfigurationCompositeKey(appBuilder, false))
            )
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Verify that the OData route in the test controller is valid
                var response = await httpClient.GetAsync($"/{ODataRoutePrefix}/{CompositeKeyEndpointName}");
                await response.ValidateSuccessAsync();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                var testEndpoint = $"/{ODataRoutePrefix}/{CompositeKeyEndpointName}({IdKeyName}={{{IdKeyName}}},{EnumKeyName}={EnumNamespace}'{{{EnumKeyName}}}')";
                swaggerDocument.paths.TryGetValue(testEndpoint, out pathItem);
                pathItem.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void ConfigurationEnumKey(IAppBuilder appBuilder, bool isEnumPrefixFree)
        {
            var config = appBuilder.GetStandardHttpConfig(typeof(ProductWithEnumKeysTestController));

            const string routeName = "EnumKeyODataRoute";
            var model = GetProductWithEnumKeyModel();
            var routingConventions = (IEnumerable<IODataRoutingConvention>)ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, config);
            var uriResolver = isEnumPrefixFree ? new StringAsEnumResolver() : new ODataUriResolver();
            config.MapODataServiceRoute(routeName,
                                        ODataRoutePrefix,
                                        builder => builder
                                            .AddService(ServiceLifetime.Singleton, sp => model)
                                            .AddService(ServiceLifetime.Singleton, sp => routingConventions)
                                            .AddService(ServiceLifetime.Singleton, sp => uriResolver));

            config.EnsureInitialized();
        }

        private static void ConfigurationCompositeKey(IAppBuilder appBuilder, bool isEnumPrefixFree)
        {
            var config = appBuilder.GetStandardHttpConfig(typeof(ProductWithCompositeEnumIntKeysTestController));

            const string routeName = "CompositeKeyODataRoute";
            var model = GetProductWithCompositeEnumIntKeyModel();
            var routingConventions = (IEnumerable<IODataRoutingConvention>)ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, config);
            var uriResolver = isEnumPrefixFree ? new StringAsEnumResolver() : new ODataUriResolver();
            config.MapODataServiceRoute(routeName,
                                        ODataRoutePrefix,
                                        builder => builder
                                            .AddService(ServiceLifetime.Singleton, sp => model)
                                            .AddService(ServiceLifetime.Singleton, sp => routingConventions)
                                            .AddService(ServiceLifetime.Singleton, sp => uriResolver));

            config.EnsureInitialized();
        }

        private static IEdmModel GetProductWithEnumKeyModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EnableLowerCamelCase();

            builder.EntitySet<ProductWithEnumKey>("ProductWithEnumKeysTest");

            return builder.GetEdmModel();
        }

        private static IEdmModel GetProductWithCompositeEnumIntKeyModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EnableLowerCamelCase();

            builder.EntitySet<ProductWithCompositeEnumIntKey>
                                            ("ProductWithCompositeEnumIntKeysTest");

            return builder.GetEdmModel();
        }
    }

    #region Models
    public class ProductWithEnumKey
    {
        [Key]
        public SwashbuckleODataSample.Models.MyEnum EnumValue { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class ProductWithCompositeEnumIntKey
    {
        [Key]
        public MyEnum EnumValue { get; set; }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }
    #endregion

    #region ODataControllers
    public class ProductWithEnumKeysTestController : ODataController
    {
        private static readonly Dictionary<MyEnum,
                                    ProductWithEnumKey> DataEnumAsKey;

        static ProductWithEnumKeysTestController()
        {
            DataEnumAsKey = new Dictionary<MyEnum, ProductWithEnumKey>()
            {
                {
                    MyEnum.ValueOne,
                    new ProductWithEnumKey {
                        EnumValue = MyEnum.ValueOne,
                        Name = "ValueOneName",
                        Price = 101
                    }
                },
                {
                    MyEnum.ValueTwo,
                    new ProductWithEnumKey
                    {
                        EnumValue = MyEnum.ValueTwo,
                        Name = "ValueTwoName",
                        Price = 102
                    }
                }
            };
        }

        /// <summary>
        /// Query products
        /// </summary>
        [HttpGet]
        [EnableQuery]
        public IQueryable<ProductWithEnumKey> Get()
        {
            return DataEnumAsKey.Values.AsQueryable();
        }

        /// <summary>
        /// Query product by enum key
        /// </summary>
        /// <param name="Key">key enum value</param>
        /// <returns>project enum model</returns>
        [HttpGet]
        public IHttpActionResult Get([FromODataUri]MyEnum Key)
        {
            return Ok(DataEnumAsKey[Key]);
        }
    }

    public class ProductWithCompositeEnumIntKeysTestController : ODataController
    {
        private static readonly List<ProductWithCompositeEnumIntKey> DataCompositeKey;

        static ProductWithCompositeEnumIntKeysTestController()
        {
            DataCompositeKey = new List<ProductWithCompositeEnumIntKey>()
            {
                {
                    new ProductWithCompositeEnumIntKey
                    {
                        EnumValue = MyEnum.ValueOne,
                        Id = 1,
                        Name = "ValueOneName",
                        Price = 101
                    }
                },
                {
                    new ProductWithCompositeEnumIntKey
                    {
                        EnumValue = MyEnum.ValueTwo,
                        Id = 2,
                        Name = "ValueTwoName",
                        Price = 102
                    }
                }
            };
        }

        /// <summary>
        /// Query products
        /// </summary>
        [EnableQuery]
        public IQueryable<ProductWithCompositeEnumIntKey> Get()
        {
            return DataCompositeKey.AsQueryable();
        }

        /// <summary>
        /// Query products by keys
        /// </summary>
        /// <param name="keyenumValue">key enum value</param>
        /// <param name="keyid">key id</param>
        /// <returns>composite enum-int key model</returns>
        [EnableQuery]
        public IHttpActionResult Get([FromODataUri]MyEnum keyenumValue, [FromODataUri]int keyid)
        {
            return Ok(DataCompositeKey
                        .Where(x => x.EnumValue == keyenumValue
                                    && x.Id == keyid));
        }
    }
    #endregion
}