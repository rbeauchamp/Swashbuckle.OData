using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
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
using SwashbuckleODataSample;
using SwashbuckleODataSample.Models;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class FunctionTests
    {
        [Test]
        public async Task It_supports_a_parameterless_function_bound_to_a_collection()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products/Default.MostExpensive()", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
            }
        }

        [Test]
        public async Task It_supports_a_function_bound_to_an_entity_set_that_returns_a_collection()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products/Default.Top10()", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
            }
        }

        [Test]
        public async Task It_supports_a_function_bound_to_an_entity()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products({Id})/Default.GetPriceRank()", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
            }
        }

        [Test]
        public async Task It_supports_a_function_that_accepts_a_string_parameter()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/Products({Id})/Default.CalculateGeneralSalesTax(state={state})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
            }
        }

        [Test]
        public async Task It_supports_unbound_functions()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(ProductsV1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/v1/GetSalesTaxRate(state={state})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
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

    public class ProductsV1Controller : ODataController
    {
        private static readonly ConcurrentDictionary<int, Product> Data;

        static ProductsV1Controller()
        {
            Data = new ConcurrentDictionary<int, Product>();
            var rand = new Random();

            Enumerable.Range(0, 100).Select(i => new Product
            {
                Id = i,
                Name = "Product " + i,
                Price = rand.NextDouble() * 1000
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        /// <summary>
        /// Get the most expensive product. This is a function bound to a collection.
        /// </summary>
        [HttpGet]
        [ResponseType(typeof(double))]
        public IHttpActionResult MostExpensive()
        {
            var retval = Data.Max(pair => pair.Value.Price);

            return Ok(retval);
        }

        /// <summary>
        /// Get the top 10 most expensive products. This is a function bound to a collection that returns a collection.
        /// </summary>
        [HttpGet]
        [ResponseType(typeof(List<Product>))]
        public IHttpActionResult Top10()
        {
            var retval = Data.Values.OrderByDescending(p => p.Price).Take(10).ToList();

            return Ok(retval);
        }

        /// <summary>
        /// Get the rank of the product price. This is a function bound to an entity.
        /// </summary>
        /// <param name="key">The product id</param>
        [HttpGet]
        [ResponseType(typeof(int))]
        public IHttpActionResult GetPriceRank(int key)
        {
            Product p;
            if (Data.TryGetValue(key, out p))
            {
                // NOTE: Use where clause to get the rank of the price may not
                // offer the good time complexity. The following code is intended
                // for demostration only.
                return Ok(Data.Values.Count(one => one.Price > p.Price));
            }
            return NotFound();
        }

        /// <summary>
        /// Get the sales tax for a product in a given state. This is a function which accepts a parameter.
        /// </summary>
        /// <param name="key">The product id</param>
        /// <param name="state">The state</param>
        [HttpGet]
        [ResponseType(typeof(double))]
        public IHttpActionResult CalculateGeneralSalesTax(int key, string state)
        {
            var taxRate = GetRate(state);

            Product product;
            if (Data.TryGetValue(key, out product))
            {
                var tax = product.Price * taxRate / 100;
                return Ok(tax);
            }
            return NotFound();
        }

        /// <summary>
        /// Get the sales tax rate for a state. This is an unbound function.
        /// </summary>
        /// <param name="state">The state</param>
        [HttpGet]
        [ResponseType(typeof(double))]
        [ODataRoute("GetSalesTaxRate(state={state})")]
        public IHttpActionResult GetSalesTaxRate([FromODataUri] string state)
        {
            return Ok(GetRate(state));
        }

        private static double GetRate(string state)
        {
            double taxRate;
            switch (state)
            {
                case "AZ":
                    taxRate = 5.6;
                    break;
                case "CA":
                    taxRate = 7.5;
                    break;
                case "CT":
                    taxRate = 6.35;
                    break;
                case "GA":
                    taxRate = 4;
                    break;
                case "IN":
                    taxRate = 7;
                    break;
                case "KS":
                    taxRate = 6.15;
                    break;
                case "KY":
                    taxRate = 6;
                    break;
                case "MA":
                    taxRate = 6.25;
                    break;
                case "NV":
                    taxRate = 6.85;
                    break;
                case "NJ":
                    taxRate = 7;
                    break;
                case "NY":
                    taxRate = 4;
                    break;
                case "NC":
                    taxRate = 4.75;
                    break;
                case "ND":
                    taxRate = 5;
                    break;
                case "PA":
                    taxRate = 6;
                    break;
                case "TN":
                    taxRate = 7;
                    break;
                case "TX":
                    taxRate = 6.25;
                    break;
                case "VA":
                    taxRate = 4.3;
                    break;
                case "WA":
                    taxRate = 6.5;
                    break;
                case "WV":
                    taxRate = 6;
                    break;
                case "WI":
                    taxRate = 5;
                    break;

                default:
                    taxRate = 0;
                    break;
            }

            return taxRate;
        }
    }
}