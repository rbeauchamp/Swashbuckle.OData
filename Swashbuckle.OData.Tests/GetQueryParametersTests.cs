using System.Linq;
using System.Web.OData.Builder;
using FluentAssertions;
using NUnit.Framework;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class GetQueryParametersTests
    {
        [Test]
        public void It_includes_the_filter_parameter()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            var edmModel = builder.GetEdmModel();
            var provider = new ODataSwaggerProvider(edmModel);

            // Act
            var swaggerDocument = provider.GetSwagger("http://localhost/", "1.0");

            // Assert
            PathItem pathItem;
            swaggerDocument.paths.TryGetValue("/Customers", out pathItem);
            pathItem.Should().NotBeNull();
            var filterParameter = pathItem.get.parameters.SingleOrDefault(parameter => parameter.name == "$filter");
            filterParameter.Should().NotBeNull();
            filterParameter.description.Should().NotBeNullOrWhiteSpace();
            filterParameter.type.ShouldBeEquivalentTo("string");
            filterParameter.@in.ShouldBeEquivalentTo("query");
        }
    }
}