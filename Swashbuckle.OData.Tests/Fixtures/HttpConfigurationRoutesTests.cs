using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Swashbuckle.OData.Tests.WebHost;
using Swashbuckle.Swagger;
using SwashbuckleODataSample;

namespace Swashbuckle.OData.Tests
{
    /// <summary>
    /// Verifies that the ODataSwaggerProvider can get
    /// settings via GlobalConfiguration.HttpConfiguration
    /// </summary>
    public class HttpConfigurationTests
    {
        [Test]
        public async Task It_gets_the_route_prefix_from_config_MapODataServiceRoute()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.basePath.ShouldBeEquivalentTo("/" + WebApiConfig.ODataRoutePrefix);
            }
        }
    }
}