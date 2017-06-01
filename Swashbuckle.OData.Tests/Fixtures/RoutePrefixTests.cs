﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
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

        [TestCase("/Pins")]
        [TestCase("/Pins({key})")]
        [TestCase("/Pins({key})/Default.Archive()")]
        [TestCase("/Pins/Default.Archived()")]
        [TestCase("/Foo()")]
        public async Task It_handles_an_odata_route_prefix_attribute(string path)
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(RoutePrefixedPinsController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue(path, out pathItem);
                pathItem.Should().NotBeNull();

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

            builder.Function("Foo").Returns<int>();

            builder.EntityType<Pin>().Function("Archive").Returns<Pin>();

            builder.EntityType<Pin>().Collection.Function("Archived").ReturnsCollection<Pin>();

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

    [ODataRoutePrefix("Pins")]
    public class RoutePrefixedPinsController : ODataController
    {
        [EnableQuery]
        [ODataRoute]
        public IQueryable<Pin> GetPins()
        {
            return Enumerable.Empty<Pin>().AsQueryable();
        }

        [EnableQuery]
        [ODataRoute("({key})")]
        public SingleResult<Pin> GetPin([FromODataUri] long key)
        {
            return SingleResult.Create(Enumerable.Empty<Pin>().AsQueryable());
        }

        [ODataRoute("({key})/Default.Archive")]
        public SingleResult<Pin> GetArchive([FromODataUri] long key)
        {
            return SingleResult.Create(Enumerable.Empty<Pin>().AsQueryable());
        }

        [EnableQuery]
        [ODataRoute("Default.Archived")]
        public IQueryable<Pin> GetArchived()
        {
            return Enumerable.Empty<Pin>().AsQueryable();
        }

        [ODataRoute("/Foo")]
        public int GetFoo()
        {
            return 0;
        }
    }
}