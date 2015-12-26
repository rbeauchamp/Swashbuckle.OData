using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using FluentAssertions;
using Microsoft.Owin.Hosting;
using Microsoft.Restier.EntityFramework;
using Microsoft.Restier.WebApi;
using Microsoft.Restier.WebApi.Batch;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NorthwindAPI.Models;
using NUnit.Framework;
using Owin;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using SwashbuckleODataSample;
using SwashbuckleODataSample.Models;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class RestierTests
    {
        [Test]
        public async Task It_supports_restier()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, Configuration))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem restierPath;
                swaggerDocument.paths.TryGetValue("/restier/Users({Id})", out restierPath);
                restierPath.Should().NotBeNull();
                restierPath.get.Should().NotBeNull();
                restierPath.get.parameters.Single(parameter => parameter.name == "Id").type.Should().NotBeNullOrEmpty();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_has_a_restier_get_with_all_optional_query_parameters()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, Configuration))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/restier/Users({Id})", out pathItem);
                pathItem.get.parameters.Where(parameter => parameter.name.StartsWith("$")).Should().OnlyContain(parameter => parameter.required == false);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_has_a_restier_response_with_the_correct_edm_model_type()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, Configuration))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/restier/Users({Id})", out pathItem);
                var getByIdResponse = pathItem.get.responses.SingleOrDefault(response => response.Key == "200");
                getByIdResponse.Should().NotBeNull();
                getByIdResponse.Value.schema.@ref.Should().Be("#/definitions/User");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_groups_paths_by_entity_set()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, Configuration))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/restier/Users({Id})", out pathItem);
                pathItem.get.tags.First().Should().Be("Users");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_has_a_restier_get_users_response_of_type_array()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, Configuration))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/restier/Users", out pathItem);
                var getUsersResponse = pathItem.get.responses.SingleOrDefault(response => response.Key == "200");
                getUsersResponse.Should().NotBeNull();
                getUsersResponse.Value.schema.@ref.Should().BeNull();
                getUsersResponse.Value.schema.items.@ref.Should().Be("#/definitions/User");
                getUsersResponse.Value.schema.type.Should().Be("array");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_entities_with_multiple_keys()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, NorthwindConfiguration))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/restier/OrderDetails(OrderId={OrderId}, ProductId={ProductId})", out pathItem);
                pathItem.Should().NotBeNull();
                var getResponse = pathItem.get.responses.SingleOrDefault(response => response.Key == "200");
                getResponse.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_generates_valid_swagger_2_0_json_for_the_northwind_model()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, NorthwindConfiguration))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var response = await httpClient.GetAsync("swagger/docs/v1");

                // Assert
                await response.ValidateSuccessAsync();
                var swaggerJson = await response.Content.ReadAsStringAsync();

                var resolver = new JSchemaPreloadedResolver();
                resolver.Add(new Uri("http://json-schema.org/draft-04/schema"), File.ReadAllText(@"schema-draft-v4.json"));

                var swaggerSchema = File.ReadAllText(@"swagger-2.0-schema.json");
                var schema = JSchema.Parse(swaggerSchema, resolver);

                var swaggerJObject = JObject.Parse(swaggerJson);
                IList<string> messages;
                var isValid = swaggerJObject.IsValid(schema, out messages);
                isValid.Should().BeTrue();
            }
        }

        /// <summary>
        /// This code configures Web API.
        /// The TestWebApiStartup class is specified as a type parameter in the WebApp.Start method.
        /// </summary>
        /// <param name="appBuilder">The application builder.</param>
        public async void NorthwindConfiguration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };
            var server = new HttpServer(config);

            WebApiConfig.Register(config);
            appBuilder.UseWebApi(server);

            config
                .EnableSwagger(c =>
                {
                    // Use "SingleApiVersion" to describe a single version API. Swagger 2.0 includes an "Info" object to
                    // hold additional metadata for an API. Version and title are required but you can also provide
                    // additional fields by chaining methods off SingleApiVersion.
                    //
                    c.SingleApiVersion("v1", "A title for your API");

                    // Wrap the default SwaggerGenerator with additional behavior (e.g. caching) or provide an
                    // alternative implementation for ISwaggerProvider with the CustomProvider option.
                    //
                    c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, config));
                })
                .EnableSwaggerUi();

            FormatterConfig.Register(config);

            config.Services.Replace(typeof(IHttpControllerSelector), new RestierControllerSelector(config));

            await config.MapRestierRoute<DbApi<NorthwindContext>>("RESTierRoute", "restier", new RestierBatchHandler(server));

            config.EnsureInitialized();
        }

        private static async void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };
            var server = new HttpServer(config);
            appBuilder.UseWebApi(server);
            config.EnableSwagger(c =>
            {
                // Use "SingleApiVersion" to describe a single version API. Swagger 2.0 includes an "Info" object to
                // hold additional metadata for an API. Version and title are required but you can also provide
                // additional fields by chaining methods off SingleApiVersion.
                //
                c.SingleApiVersion("v1", "A title for your API");

                // Wrap the default SwaggerGenerator with additional behavior (e.g. caching) or provide an
                // alternative implementation for ISwaggerProvider with the CustomProvider option.
                //
                c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, config));
            }).EnableSwaggerUi();

            FormatterConfig.Register(config);

            config.Services.Replace(typeof(IHttpControllerSelector), new RestierControllerSelector(config));

            await config.MapRestierRoute<DbApi<TestRestierODataContext>>("RESTierRoute", "restier", new RestierBatchHandler(server));

            config.EnsureInitialized();
        }
    }

    public class RestierControllerSelector : DefaultHttpControllerSelector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestControllerSelector"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public RestierControllerSelector(HttpConfiguration configuration) : base(configuration)
        {
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return base.GetControllerMapping().Where(pair => pair.Value.ControllerName == "Restier").ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}