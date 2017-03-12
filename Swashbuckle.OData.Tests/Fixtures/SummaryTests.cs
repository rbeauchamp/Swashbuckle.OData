using System;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Swagger;
using SwashbuckleODataSample.Models;
using System.Xml;
using System.IO;
using SwashbuckleODataSample.ODataControllers;
using System.Net.Http;
using System.Linq;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class SummaryTests
    {
        [Test]
        public async Task It_gets_the_method_summary_from_xml_comments()
        {
            var xmlCommentsFilePath = GetXMLFileNameIfExists(typeof(Customer).Assembly);
            xmlCommentsFilePath.Should().NotBeNullOrEmpty("The xml comments file of the sample project should be generated to test this behaviour");
            var xmlCommentsDoc = new XmlDocument();
            xmlCommentsDoc.Load(xmlCommentsFilePath);

            XmlNode xmlGetCustomersSummaryNode = xmlCommentsDoc.SelectSingleNode(
                "//member[contains(@name, 'CustomersController.GetCustomers')]/summary");

            var getCustomersSummaryText = xmlGetCustomersSummaryNode.InnerText.Trim();
            
            //var xmlSummaryText = " asdas ";
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
                var summary = pathItem.get.summary;
                summary.Should().NotBeNullOrEmpty();
                summary.Trim().ShouldBeEquivalentTo(getCustomersSummaryText);
                                
                await ValidationUtils.ValidateSwaggerJson();
            }            
        }

        [Test]
        public async Task It_gets_the_properties_summary_from_xml_comments()
        {
            var xmlCommentsFilePath = GetXMLFileNameIfExists(typeof(Customer).Assembly);
            xmlCommentsFilePath.Should().NotBeNullOrEmpty("The xml comments file of the sample project should be generated to test this behaviour");
            var xmlCommentsDoc = new XmlDocument();
            xmlCommentsDoc.Load(xmlCommentsFilePath);

            XmlNode xmlCustommerIdSummaryNode = xmlCommentsDoc.SelectSingleNode(
                "//member[contains(@name, 'Models.Customer.Id')]/summary");

            var customerIdSummaryText = xmlCustommerIdSummaryNode.InnerText.Trim();

            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => Configuration(appBuilder, typeof(CustomersController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

                // Act
                var swaggerDocument = await httpClient.GetJsonAsync<SwaggerDocument>("swagger/docs/v1");
                
                // Assert
                swaggerDocument.definitions.Should().ContainKey("Customer");
                var customerSchema = swaggerDocument.definitions["Customer"];
                customerSchema.Should().NotBeNull();
                customerSchema.properties.Should().NotBeNull();
                customerSchema.properties.Should().ContainKey("Id");
                customerSchema.properties["Id"].description.Should().Be(customerIdSummaryText);

                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        private static string GetXMLFileNameIfExists(System.Reflection.Assembly assembly)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var commentsFileName = "SwashbuckleODataSample.XML";
            var xmlPath = Path.Combine(baseDirectory, commentsFileName);
            if (File.Exists(xmlPath))
                return xmlPath;

            return "";
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