using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using Swashbuckle.OData.Tests.WebHost;
using Swashbuckle.Swagger;
using SwashbuckleODataSample;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class RestierTests
    {
        [Test]
        public async Task It_supports_restier()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem restierPath;
                swaggerDocument.paths.TryGetValue("/restier/Users({Id})", out restierPath);
                restierPath.Should().NotBeNull();
                restierPath.get.Should().NotBeNull();
                restierPath.get.parameters.Single(parameter => parameter.name == "Id").type.Should().NotBeNullOrEmpty();
            }
        }

        [Test]
        public async Task It_has_a_restier_get_with_all_optional_query_parameters()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/restier/Users({Id})", out pathItem);
                pathItem.get.parameters.Where(parameter => parameter.name.StartsWith("$")).Should().OnlyContain(parameter => parameter.required == false);
            }
        }

        [Test]
        public async Task It_has_a_restier_response_with_the_correct_edm_model_type()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/restier/Users({Id})", out pathItem);
                var getByIdResponse = pathItem.get.responses.SingleOrDefault(response => response.Key == "200");
                getByIdResponse.Should().NotBeNull();
                getByIdResponse.Value.schema.@ref.Should().Be("#/definitions/User");
            }
        }

        [Test]
        public async Task It_groups_paths_by_entity_set()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/restier/Users({Id})", out pathItem);
                pathItem.get.tags.First().Should().Be("Users");
            }
        }

        [Test]
        public async Task It_has_a_restier_get_users_response_of_type_array()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(TestWebApiStartup.BaseAddress, ODataConfig.ODataRoutePrefix);

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
            }
        }

        [Test]
        public async Task It_supports_entities_with_multiple_keys()
        {
            using (WebApp.Start(NorthwindApiStartup.BaseAddress, appBuilder => new NorthwindApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(NorthwindApiStartup.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/restier/Order_Details(OrderID={OrderID}, ProductID={ProductID})", out pathItem);
                var getResponse = pathItem.get.responses.SingleOrDefault(response => response.Key == "200");
                getResponse.Should().NotBeNull();
            }
        }

        [Test]
        public async Task It_generates_valid_swagger_2_0_json_for_the_northwind_model()
        {
            using (WebApp.Start(NorthwindApiStartup.BaseAddress, appBuilder => new NorthwindApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(NorthwindApiStartup.BaseAddress);

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
    }
}