using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Swashbuckle.OData.Tests.WebHost;
using Swashbuckle.Swagger;

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
                var httpClient = HttpClientUtils.GetHttpClient();

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
                var httpClient = HttpClientUtils.GetHttpClient();

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
                var httpClient = HttpClientUtils.GetHttpClient();

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
    }
}