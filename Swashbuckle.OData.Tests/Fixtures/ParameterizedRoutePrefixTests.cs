using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing.Constraints;
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
    public class ParameterizedRoutePrefixTests
    {
        [Test]
        public async Task It_supports_parameterized_route_prefixes_of_type_long()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                var longRoute = config.MapODataServiceRoute("longParam", "odata/{longParam}", GetEdmModel());
                longRoute.Constraints.Add("longParam", new LongRouteConstraint());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/2147483648/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/{longParam}/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_parameterized_route_prefixes_of_type_bool()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                var boolRoute = config.MapODataServiceRoute("boolParam", "odata/{boolParam}", GetEdmModel());
                boolRoute.Constraints.Add("boolParam", new BoolRouteConstraint());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/true/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/{boolParam}/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_parameterized_route_prefixes_of_type_dateTime()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                var dateTimeRoute = config.MapODataServiceRoute("dateTimeParam", "odata/{dateTimeParam}", GetEdmModel());
                dateTimeRoute.Constraints.Add("dateTimeParam", new DateTimeRouteConstraint());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/2015-10-10T17:00:00Z/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/{dateTimeParam}/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_parameterized_route_prefixes_of_type_decimal()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                var decimalRoute = config.MapODataServiceRoute("decimalParam", "odata/{decimalParam}", GetEdmModel());
                decimalRoute.Constraints.Add("decimalParam", new DecimalRouteConstraint());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/1.12/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/{decimalParam}/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_parameterized_route_prefixes_of_type_double()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                var doubleRoute = config.MapODataServiceRoute("doubleParam", "odata/{doubleParam}", GetEdmModel());
                doubleRoute.Constraints.Add("doubleParam", new DoubleRouteConstraint());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/2.34/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/{doubleParam}/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_parameterized_route_prefixes_of_type_float()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                var floatRoute = config.MapODataServiceRoute("floatParam", "odata/{floatParam}", GetEdmModel());
                floatRoute.Constraints.Add("floatParam", new FloatRouteConstraint());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/2.34/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/{floatParam}/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_parameterized_route_prefixes_of_type_guid()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                var guidRoute = config.MapODataServiceRoute("guidParam", "odata/{guidParam}", GetEdmModel());
                guidRoute.Constraints.Add("guidParam", new GuidRouteConstraint());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/8b3434cb-112e-494d-82d5-17021c928012/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/{guidParam}/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_parameterized_route_prefixes_of_type_int()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                var intRoute = config.MapODataServiceRoute("intParam", "odata/{intParam}", GetEdmModel());
                intRoute.Constraints.Add("intParam", new IntRouteConstraint());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/45/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/{intParam}/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_route_prefixes_with_multiple_parameters()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                var intRoute = config.MapODataServiceRoute("multiParam", "odata/{intParam}/{boolParam}", GetEdmModel());
                intRoute.Constraints.Add("intParam", new IntRouteConstraint());
                intRoute.Constraints.Add("boolParam", new BoolRouteConstraint());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/45/true/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/{intParam}/{boolParam}/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_parameterized_route_prefixes_of_type_string()
        {
            Action<HttpConfiguration> configAction = config =>
            {
                config.MapODataServiceRoute("stringParam", "odata/{stringParam}", GetEdmModel());
            };

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutesController), configAction)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var oDataResponse = await httpClient.GetJsonAsync<ODataResponse<List<Route>>>("/odata/'foo'/Routes");
                oDataResponse.Value.Should().NotBeNull();
                oDataResponse.Value.Count.Should().Be(20);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/'{stringParam}'/Routes", out pathItem);
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController, Action<HttpConfiguration> configAction)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            configAction(config);

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Route>("Routes");

            return builder.GetEdmModel();
        }
    }

    public class Route
    {
        [Key]
        public string Id { get; set; }
    }

    public class RoutesController : ODataController
    {
        private static readonly ConcurrentDictionary<string, Route> Data;

        static RoutesController()
        {
            Data = new ConcurrentDictionary<string, Route>();

            Enumerable.Range(0, 20).Select(i => new Route
            {
                Id = Guid.NewGuid().ToString()
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(long longParam)
        {
            return Data.Values.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(bool boolParam)
        {
            return Data.Values.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(DateTime dateTimeParam)
        {
            return Data.Values.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(decimal decimalParam)
        {
            return Data.Values.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(double doubleParam)
        {
            return Data.Values.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(float floatParam)
        {
            return Data.Values.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(Guid guidParam)
        {
            return Data.Values.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(int intParam)
        {
            return Data.Values.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(string stringParam)
        {
            return Data.Values.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<Route> GetRoutes(int intParam, bool boolParam)
        {
            return Data.Values.AsQueryable();
        }
    }
}