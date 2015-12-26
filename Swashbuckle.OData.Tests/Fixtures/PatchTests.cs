using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.ODataControllers;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class PatchTests
    {
        [Test]
        public async Task It_has_a_body_parameter_with_a_schema()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(OrdersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Orders({OrderId})", out pathItem);
                pathItem.patch.parameters.Single(parameter => parameter.@in == "body").schema.Should().NotBeNull();
                pathItem.patch.parameters.Single(parameter => parameter.@in == "body").schema.@ref.Should().Be("#/definitions/Order");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            // Define a default non- versioned route(default route should be at the end as a last catch-all)
            config.MapODataServiceRoute("DefaultODataRoute", "odata", GetDefaultModel());

            config.EnsureInitialized();
        }

        private static IEdmModel GetDefaultModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }
    }
}