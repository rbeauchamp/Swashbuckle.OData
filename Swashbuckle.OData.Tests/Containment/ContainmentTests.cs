using System;
using System.Threading.Tasks;
using System.Web.OData.Extensions;
using FluentAssertions;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;
using SwashbuckleODataSample;

namespace Swashbuckle.OData.Tests.Containment
{
    [TestFixture]
    public class ContainmentTests
    {
        [Test]
        public async Task It_supports_models_that_use_containment()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(AccountsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Accounts", out pathItem);
                pathItem.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_odata_attribute_routing()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(AccountsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Accounts({accountId})/PayinPIs({paymentInstrumentId})", out pathItem);
                pathItem.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            config.MapODataServiceRoute("odata", "odata", ODataModels.GetModel());

            config.EnsureInitialized();
        }
    }
}