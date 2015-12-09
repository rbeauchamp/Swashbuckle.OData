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
    public class GetTests
    {
        [Test]
        public async Task It_includes_the_filter_parameter()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers", out pathItem);
                pathItem.Should().NotBeNull();
                var filterParameter = pathItem.get.parameters.SingleOrDefault(parameter => parameter.name == "$filter");
                filterParameter.Should().NotBeNull();
                filterParameter.description.Should().NotBeNullOrWhiteSpace();
                filterParameter.type.ShouldBeEquivalentTo("string");
                filterParameter.@in.ShouldBeEquivalentTo("query");
            }
        }

        [Test]
        public async Task It_has_all_optional_odata_query_parameters()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers", out pathItem);
                pathItem.get.parameters.Where(parameter => parameter.name.StartsWith("$")).Should().OnlyContain(parameter => parameter.required == false);
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
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers({Id})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
                pathItem.get.parameters.Should().Contain(parameter => parameter.name == "Id");
            }
        }

        [Test]
        public async Task It_supports_types_with_a_guid_primary_key()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Orders({OrderId})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
            }
        }
    }
}