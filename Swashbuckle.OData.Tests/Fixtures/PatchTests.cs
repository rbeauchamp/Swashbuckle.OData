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
    public class PatchTests
    {
        [Test]
        public async Task It_has_a_body_parameter_with_a_schema()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/Orders({OrderId})", out pathItem);
                pathItem.patch.parameters.Single(parameter => parameter.@in == "body").schema.Should().NotBeNull();
                pathItem.patch.parameters.Single(parameter => parameter.@in == "body").schema.@ref.Should().Be("#/definitions/Order");
            }
        }
    }
}