using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
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
    public class FunctionTests
    {
        [Test]
        public async Task It_supports_functions_with_enum_parameters()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var products = await httpClient.GetJsonAsync<ODataResponse<List<Product1>>>("/odata/v1/Products/Default.EnumParam(Id=3,EnumValue=SwashbuckleODataSample.Models.MyEnum'ValueOne')");
                products.Should().NotBeNull();
                products.Value.Count.Should().Be(2);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products/Default.EnumParam(Id={Id},EnumValue=SwashbuckleODataSample.Models.MyEnum'{EnumValue}')", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_a_function_bound_to_an_entity_with_enum_parameters()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var match = await httpClient.GetJsonAsync<ODataResponse<bool>>("/odata/v1/Products(3)/Default.IsEnumValueMatch(EnumValue=SwashbuckleODataSample.Models.MyEnum'ValueOne')");
                match.Should().NotBeNull();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products({Id})/Default.IsEnumValueMatch(EnumValue=SwashbuckleODataSample.Models.MyEnum'{EnumValue}')", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_functions_with_multiple_parameters()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var products = await httpClient.GetJsonAsync<ODataResponse<List<Product1>>>("/odata/v1/Products/Default.MultipleParams(Id=3,Year=2015)");
                products.Should().NotBeNull();
                products.Value.Count.Should().Be(2);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products/Default.MultipleParams(Id={Id},Year={Year})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_a_parameterless_function_bound_to_a_collection()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products/Default.MostExpensive()", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_a_function_bound_to_an_entity_set_that_returns_a_collection()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products/Default.Top10()", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_a_function_bound_to_an_entity()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products({Id})/Default.GetPriceRank()", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_a_function_that_accepts_a_string_parameter()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products({Id})/Default.CalculateGeneralSalesTax(state='{state}')", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_unbound_functions()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/GetSalesTaxRate(state='{state}')", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            var controllerSelector = new UnitTestODataVersionControllerSelector(config, targetController);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            // Define a route to a controller class that contains functions
            config.MapODataServiceRoute("FunctionsODataRoute", "odata/v1", GetFunctionsEdmModel());
            controllerSelector.RouteVersionSuffixMapping.Add("FunctionsODataRoute", "V1");

            config.EnsureInitialized();
        }

        private static IEdmModel GetFunctionsEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Product>("Products");

            var productType = builder.EntityType<Product>();

            // Function bound to a collection that accepts an enum parameter
            var enumParamFunction = productType.Collection.Function("EnumParam");
            enumParamFunction.Parameter<int>("Id");
            enumParamFunction.Parameter<MyEnum>("EnumValue");
            enumParamFunction.ReturnsCollectionFromEntitySet<Product>("Products");

            // Function bound to an entity that accepts an enum parameter
            var enumParamEntityFunction = productType.Function("IsEnumValueMatch");
            enumParamEntityFunction.Parameter<MyEnum>("EnumValue");
            enumParamEntityFunction.Returns<bool>();

            // Function bound to a collection that accepts multiple parameters
            var multiParamFunction = productType.Collection.Function("MultipleParams");
            multiParamFunction.Parameter<int>("Id");
            multiParamFunction.Parameter<int>("Year");
            multiParamFunction.ReturnsCollectionFromEntitySet<Product>("Products");

            // Function bound to a collection
            // Returns the most expensive product, a single entity
            productType.Collection
                .Function("MostExpensive")
                .Returns<double>();

            // Function bound to a collection
            // Returns the top 10 product, a collection
            productType.Collection
                .Function("Top10")
                .ReturnsCollectionFromEntitySet<Product>("Products");

            // Function bound to a single entity
            // Returns the instance's price rank among all products
            productType
                .Function("GetPriceRank")
                .Returns<int>();

            // Function bound to a single entity
            // Accept a string as parameter and return a double
            // This function calculate the general sales tax base on the 
            // state
            productType
                .Function("CalculateGeneralSalesTax")
                .Returns<double>()
                .Parameter<string>("state");

            // Unbound Function
            builder.Function("GetSalesTaxRate")
                .Returns<double>()
                .Parameter<string>("state");

            return builder.GetEdmModel();
        }
    }
}