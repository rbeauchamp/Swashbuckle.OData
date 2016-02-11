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
    public class GetTests
    {
        [Test]
        public async Task It_includes_the_filter_parameter()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(CustomersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

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

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_has_all_optional_odata_query_parameters()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(CustomersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers", out pathItem);
                pathItem.get.parameters.Where(parameter => parameter.name.StartsWith("$")).Should().OnlyContain(parameter => parameter.required == false);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_has_collection_odata_query_parameters()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(CustomersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers", out pathItem);
                pathItem.get.parameters.Where(parameter => parameter.name.StartsWith("$")).Should().HaveCount(7);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_has_single_entity_odata_query_parameters()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(CustomersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers({Id})", out pathItem);
                pathItem.get.parameters.Where(parameter => parameter.name.StartsWith("$")).Should().HaveCount(2);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }
        
        [Test]
        public async Task It_has_a_parameter_with_a_name_equal_to_the_path_name()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(CustomersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customers({Id})", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();
                pathItem.get.parameters.Should().Contain(parameter => parameter.name == "Id");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_types_with_a_guid_primary_key()
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
                pathItem.Should().NotBeNull();
                pathItem.get.Should().NotBeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

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