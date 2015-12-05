using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Swashbuckle.OData.Tests.WebHost;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class PostTests
    {
        [Test]
        public async Task It_has_a_body_content_type_of_application_json()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient();

                // Act
                var swaggerDocument = await httpClient.GetAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/Customers", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.post.Should().NotBeNull();
                pathItem.post.consumes.Should().Contain("application/json");
            }
        }

        [Test]
        public async Task It_has_a_parameter_with_a_name_equal_to_the_path_name()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient();

                // Act
                var swaggerDocument = await httpClient.GetAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/Customers({Id})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
                pathItem.get.parameters.Should().Contain(parameter => parameter.name == "Id");
            }
        }
    }
}