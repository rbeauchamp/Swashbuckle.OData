using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.OData.Tests.WebHost;
using Swashbuckle.Swagger;
using SwashbuckleODataSample;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class ODataSwaggerProviderTests
    {
        [Test]
        public async Task It_applies_document_filters()
        {
            // Arrange
            Action<SwaggerDocsConfig> config = c => c.DocumentFilter<ApplyDocumentVendorExtensions>();
            var httpClient = HttpClientUtils.GetHttpClient();

            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder, config)))
            {
                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.host.Should().Be("foo");
            }
        }

        [Test]
        public async Task It_supports_multiple_odata_routes()
        {
            using (WebApp.Start(TestWebApiStartup.BaseAddress, appBuilder => new TestWebApiStartup().Configuration(appBuilder)))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.Should().NotBeNull();
            }
        }
    }
}