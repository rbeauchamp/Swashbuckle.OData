using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;
using System.Web.Http.Description;

namespace Swashbuckle.OData.Tests
{
    /// <summary>
    /// Tests the scenario where developer adds EnableQuery globally.
    /// </summary>
    [TestFixture]
    public class GlobalEnableQueryTests
    {
        [Test]
        public async Task It_Finds_Global_Enable_Query_AndReturn_Swagger_Doc_With_OData_Standard_Parameters()
        {
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(Customer1Controller))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Verify that the OData route in the test controller is valid
                var customers = await httpClient.GetJsonAsync<ODataResponse<List<Customer1>>>("/odata/Customer1/Default.GetSomeCustomers()");
                customers.Should().NotBeNull();

                var number = await httpClient.GetJsonAsync<ODataResponse<int>>("/odata/Customer1/Default.GetSomeNumber()");
                customers.Should().NotBeNull();

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");

                // Assert
                PathItem getCustomersPathItem;
                swaggerDocument.paths.TryGetValue("/odata/Customer1/Default.GetSomeCustomers()", out getCustomersPathItem);
                getCustomersPathItem.Should().NotBeNull();
                getCustomersPathItem.get.Should().NotBeNull();
                Assert.IsTrue(getCustomersPathItem.get.parameters.Any(p => p.name == "$filter"));

                PathItem getSomeNumber;
                swaggerDocument.paths.TryGetValue("/odata/Customer1/Default.GetSomeNumber()", out getSomeNumber);
                getSomeNumber.Should().NotBeNull();
                getSomeNumber.get.Should().NotBeNull();
                Assert.IsTrue(getSomeNumber.get.parameters == null);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private void Configuration(IAppBuilder appBuilder, Type targetController)
        {
            var config = appBuilder.GetStandardHttpConfig(targetController);

            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());

            config.AddODataQueryFilter();
            config.Filters.Add(new EnableQueryAttribute { });

            config.EnsureInitialized();
        }

        private IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            var customers = builder.EntitySet<Customer1>("Customer1");

            var getSomeCustomers = customers.EntityType.Collection.Function("GetSomeCustomers");
            getSomeCustomers.ReturnsCollectionFromEntitySet<Customer1>("Customer1");

            var getSomeNumber = customers.EntityType.Collection.Function("GetSomeNumber");
            getSomeNumber.Returns<int>();

            return builder.GetEdmModel();
        }
    }

    public class Customer1
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class Customer1Controller : ODataController
    {
        [HttpGet]
        [ResponseType(typeof(List<Customer1>))]
        public IHttpActionResult GetSomeCustomers()
        {
            return Ok(Enumerable.Empty<Customer1>());
        }

        [HttpGet]
        [ResponseType(typeof(int))]
        public IHttpActionResult GetSomeNumber()
        {
            return Ok(0);
        }
    }
}