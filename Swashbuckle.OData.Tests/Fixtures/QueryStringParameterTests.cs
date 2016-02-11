using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class QueryStringParameterTests
    {
        [Test]
        public async Task It_displays_parameters_not_described_in_the_edm_model()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => FoobarsSetup.Configuration(appBuilder, typeof(FoobarsSetup.FoobarsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var results = await httpClient.GetJsonAsync<ODataResponse<List<FoobarsSetup.Foobar>>>("odata/Foobars?bar=true");
                results.Should().NotBeNull();
                results.Value.Count.Should().Be(2);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Foobars", out pathItem);
                pathItem.Should().NotBeNull();
                var barParameter = pathItem.get.parameters.SingleOrDefault(parameter => parameter.name == "bar");
                barParameter.Should().NotBeNull();
                barParameter.required.Should().BeFalse();
                barParameter.type.ShouldBeEquivalentTo("boolean");
                barParameter.@in.ShouldBeEquivalentTo("query");
                var filterParameter = pathItem.get.parameters.SingleOrDefault(parameter => parameter.name == "$filter");
                filterParameter.Should().NotBeNull();
                filterParameter.description.Should().NotBeNullOrWhiteSpace();
                filterParameter.type.ShouldBeEquivalentTo("string");
                filterParameter.@in.ShouldBeEquivalentTo("query");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_displays_parameters_not_defined_in_the_odata_route()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => WombatsSetup.Configuration(appBuilder, typeof(WombatsSetup.WombatsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var results = await httpClient.GetJsonAsync<ODataResponse<List<WombatsSetup.Wombat>>>("odata/Wombats?bat=true");
                results.Should().NotBeNull();
                results.Value.Count.Should().Be(2);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Wombats", out pathItem);
                pathItem.Should().NotBeNull();
                var barParameter = pathItem.get.parameters.SingleOrDefault(parameter => parameter.name == "bat");
                barParameter.Should().NotBeNull();
                barParameter.required.Should().BeFalse();
                barParameter.type.ShouldBeEquivalentTo("boolean");
                barParameter.@in.ShouldBeEquivalentTo("query");
                var filterParameter = pathItem.get.parameters.SingleOrDefault(parameter => parameter.name == "$filter");
                filterParameter.Should().NotBeNull();
                filterParameter.description.Should().NotBeNullOrWhiteSpace();
                filterParameter.type.ShouldBeEquivalentTo("string");
                filterParameter.@in.ShouldBeEquivalentTo("query");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }
    }

    public class FoobarsSetup
    {
        public static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            config.MapODataServiceRoute("odata", "odata", GetEdmModel());

            config.EnsureInitialized();
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<Foobar>("Foobars");

            return builder.GetEdmModel();
        }

        public class Foobar
        {
            [Key]
            public long Id { get; set; }
            public string Variation { get; set; }
        }

        public class FoobarsController : ODataController
        {
            [EnableQuery]
            public IQueryable<Foobar> GetFoobars([FromODataUri] bool? bar = null)
            {
                IEnumerable<Foobar> foobars = new[]
                {
                new Foobar { Id=1, Variation = "a"},
                new Foobar { Id=2, Variation = "b"},
                new Foobar { Id=3, Variation = "c"},
                new Foobar { Id=4, Variation = "d"}
            };
                if (bar != null && bar.Value) foobars = foobars.Where(fb => fb.Id >= 3);
                return foobars.AsQueryable();
            }
        }
    }

    public class WombatsSetup
    {
        public static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            config.MapODataServiceRoute("odata", "odata", GetEdmModel());

            config.EnsureInitialized();
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<Wombat>("Wombats");

            return builder.GetEdmModel();
        }

        public class Wombat
        {
            [Key]
            public long Id { get; set; }
            public string Variation { get; set; }
        }

        public class WombatsController : ODataController
        {
            [EnableQuery]
            [ODataRoute("Wombats")]
            public IQueryable<Wombat> GetWombats([FromODataUri] bool? bat = null)
            {
                IEnumerable<Wombat> wombats = new[]
                {
                new Wombat { Id=1, Variation = "a"},
                new Wombat { Id=2, Variation = "b"},
                new Wombat { Id=3, Variation = "c"},
                new Wombat { Id=4, Variation = "d"}
            };
                if (bat != null && bat.Value) wombats = wombats.Where(wb => wb.Id >= 3);
                return wombats.AsQueryable();
            }
        }
    }
}