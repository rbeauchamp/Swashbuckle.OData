using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class RoutePrefixTests
    {
        [Test]
        public async Task It_handles_a_null_route_prefix()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(PinsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var result = await httpClient.GetAsync("/Pins");
                result.IsSuccessStatusCode.Should().BeTrue();

                // Act and Assert
                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            config.MapODataServiceRoute("ODataRoute", null, GetEdmModel());

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<Pin>("Pins");

            return builder.GetEdmModel();
        }
    }

    public class Pin
    {
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class PinsController : ODataController
    {
        [EnableQuery]
        public IQueryable<Pin> GetPins()
        {
            return Enumerable.Empty<Pin>().AsQueryable();
        }
    }
}