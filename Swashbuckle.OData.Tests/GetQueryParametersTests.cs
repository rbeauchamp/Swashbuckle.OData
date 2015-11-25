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
    public class GetQueryParametersTests
    {
        [Test]
        public async Task It_includes_the_filter_parameter()
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
                var filterParameter = pathItem.get.parameters.SingleOrDefault(parameter => parameter.name == "$filter");
                filterParameter.Should().NotBeNull();
                filterParameter.description.Should().NotBeNullOrWhiteSpace();
                filterParameter.type.ShouldBeEquivalentTo("string");
                filterParameter.@in.ShouldBeEquivalentTo("query");
            }
        }
    }
}