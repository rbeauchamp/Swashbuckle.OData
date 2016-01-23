using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;
using SwashbuckleODataSample.Models;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class ActionTests
    {
        [Test]
        public async Task It_supports_actions_with_only_body_paramters()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(SuppliersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var supplierDto = new SupplierDto
                {
                    Name = "SupplierName",
                    Code = "SDTO",
                    Description = "SupplierDescription"
                };
                var result = await httpClient.PostAsJsonAsync("/odata/Suppliers/Default.Create", supplierDto);
                result.IsSuccessStatusCode.Should().BeTrue();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Suppliers/Default.Create", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.post.Should().NotBeNull();
                pathItem.post.parameters.Count.Should().Be(1);
                pathItem.post.parameters.Single().@in.Should().Be("body");
                pathItem.post.parameters.Single().schema.Should().NotBeNull();
                pathItem.post.parameters.Single().schema.properties.Should().NotBeNull();
                pathItem.post.parameters.Single().schema.properties.Count.Should().Be(3);
                pathItem.post.parameters.Single().schema.properties.Should().ContainKey("code");
                pathItem.post.parameters.Single().schema.properties.Should().ContainKey("name");
                pathItem.post.parameters.Single().schema.properties.Single(pair => pair.Key == "name").Value.type.Should().Be("string");
                pathItem.post.parameters.Single().schema.properties.Should().ContainKey("description");
                pathItem.post.parameters.Single().schema.properties.Single(pair => pair.Key == "description").Value.type.Should().Be("string");
                pathItem.post.parameters.Single().schema.required.Should().NotBeNull();
                pathItem.post.parameters.Single().schema.required.Count.Should().Be(2);
                pathItem.post.parameters.Single().schema.required.Should().Contain("code");
                pathItem.post.parameters.Single().schema.required.Should().Contain("name");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_actions_with_an_optional_enum_parameter()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(SuppliersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var supplierDto = new SupplierWithEnumDto
                {
                    EnumValue = MyEnum.ValueOne
                };
                var result = await httpClient.PostAsJsonAsync("/odata/Suppliers/Default.CreateWithEnum", supplierDto);
                result.IsSuccessStatusCode.Should().BeTrue();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Suppliers/Default.CreateWithEnum", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.post.Should().NotBeNull();
                pathItem.post.parameters.Count.Should().Be(1);
                pathItem.post.parameters.Single().@in.Should().Be("body");
                pathItem.post.parameters.Single().schema.Should().NotBeNull();
                pathItem.post.parameters.Single().schema.type.Should().Be("object");
                pathItem.post.parameters.Single().schema.properties.Should().NotBeNull();
                pathItem.post.parameters.Single().schema.properties.Count.Should().Be(1);
                pathItem.post.parameters.Single().schema.properties.Should().ContainKey("EnumValue");
                pathItem.post.parameters.Single().schema.properties.Single(pair => pair.Key == "EnumValue").Value.type.Should().Be("string");
                pathItem.post.parameters.Single().schema.properties.Single(pair => pair.Key == "EnumValue").Value.@enum.Should().NotBeNull();
                pathItem.post.parameters.Single().schema.properties.Single(pair => pair.Key == "EnumValue").Value.@enum.Count.Should().Be(2);
                pathItem.post.parameters.Single().schema.properties.Single(pair => pair.Key == "EnumValue").Value.@enum.First().Should().Be(MyEnum.ValueOne.ToString());
                pathItem.post.parameters.Single().schema.properties.Single(pair => pair.Key == "EnumValue").Value.@enum.Skip(1).First().Should().Be(MyEnum.ValueTwo.ToString());
                pathItem.post.parameters.Single().schema.required.Should().BeNull();

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_supports_actions_against_an_entity()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(SuppliersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var rating = new RatingDto
                {
                    Rating = 1
                };
                var result = await httpClient.PostAsJsonAsync("/odata/Suppliers(1)/Default.Rate", rating);
                result.IsSuccessStatusCode.Should().BeTrue();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem pathItem;
                swaggerDocument.paths.TryGetValue("/odata/Suppliers({Id})/Default.Rate", out pathItem);
                pathItem.Should().NotBeNull();
                pathItem.post.Should().NotBeNull();
                pathItem.post.parameters.Count.Should().Be(2);

                var idParameter = pathItem.post.parameters.SingleOrDefault(parameter => parameter.@in == "path");
                idParameter.Should().NotBeNull();
                idParameter.type.Should().Be("integer");
                idParameter.format.Should().Be("int32");
                idParameter.name.Should().Be("Id");

                var bodyParameter = pathItem.post.parameters.SingleOrDefault(parameter => parameter.@in == "body");
                bodyParameter.Should().NotBeNull();
                bodyParameter.@in.Should().Be("body");
                bodyParameter.schema.Should().NotBeNull();
                bodyParameter.schema.type.Should().Be("object");
                bodyParameter.schema.properties.Should().NotBeNull();
                bodyParameter.schema.properties.Count.Should().Be(1);
                bodyParameter.schema.properties.Should().ContainKey("Rating");
                bodyParameter.schema.properties.Single(pair => pair.Key == "Rating").Value.type.Should().Be("integer");
                bodyParameter.schema.properties.Single(pair => pair.Key == "Rating").Value.format.Should().Be("int32");
                bodyParameter.schema.required.Should().NotBeNull();
                bodyParameter.schema.required.Count.Should().Be(1);
                bodyParameter.schema.required.Should().Contain("Rating");

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            // Define a route to a controller class that contains functions
            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Supplier>("Suppliers");
            var entityType = builder.EntityType<Supplier>();

            var create = entityType.Collection.Action("Create");
            create.ReturnsFromEntitySet<Supplier>("Suppliers");
            create.Parameter<string>("code").OptionalParameter = false;
            create.Parameter<string>("name").OptionalParameter = false;
            create.Parameter<string>("description");
            
            var createWithEnum = entityType.Collection.Action("CreateWithEnum");
            createWithEnum.ReturnsFromEntitySet<Supplier>("Suppliers");
            createWithEnum.Parameter<MyEnum?>("EnumValue");

            entityType.Action("Rate")
                .Parameter<int>("Rating");

            return builder.GetEdmModel();
        }
    }

    public class SupplierDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class SupplierWithEnumDto
    {
        public MyEnum EnumValue { get; set; }
    }

    public class RatingDto
    {
        public int Rating { get; set; }
    }

    public class Supplier
    {
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class SuppliersController : ODataController
    {
        [HttpPost]
        [ResponseType(typeof(Supplier))]
        public IHttpActionResult Create(ODataActionParameters parameters)
        {
            return Created(new Supplier {Id = 1});
        }

        [HttpPost]
        [ResponseType(typeof(Supplier))]
        public IHttpActionResult CreateWithEnum(ODataActionParameters parameters)
        {
            return Created(new Supplier { Id = 1 });
        }

        [HttpPost]
        public IHttpActionResult Rate([FromODataUri] int key, ODataActionParameters parameters)
        {
            parameters.Should().ContainKey("Rating");

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}