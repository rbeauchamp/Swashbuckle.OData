using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class ModelSchemaTests
    {
        [Test]
        public async Task The_model_schema_matches_the_edm_model()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(BrandsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var result = await httpClient.GetAsync("/odata/Brands");
                result.IsSuccessStatusCode.Should().BeTrue();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                swaggerDocument.definitions.Should().ContainKey("Brand");
                var brandSchema = swaggerDocument.definitions["Brand"];

                brandSchema.properties.Should().ContainKey("id");
                brandSchema.properties.Should().ContainKey("code");
                brandSchema.properties.Should().ContainKey("name");
                brandSchema.properties.Should().ContainKey("Something");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<Brand>("Brands");

            builder.EnableLowerCamelCase(NameResolverOptions.ProcessReflectedPropertyNames | NameResolverOptions.ProcessExplicitPropertyNames);

            return builder.GetEdmModel();
        }
    }

    public class Brand
    {
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        [DataMember(Name = "Something")]
        public string Description { get; set; }
    }

    public class BrandsController : ODataController
    {
        [EnableQuery]
        public IQueryable<Brand> GetBrands()
        {
            return Enumerable.Empty<Brand>().AsQueryable();
        }
    }
}