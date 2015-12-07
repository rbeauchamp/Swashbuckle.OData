using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.OData.Tests.WebHost;
using Swashbuckle.Swagger;

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
    }
}