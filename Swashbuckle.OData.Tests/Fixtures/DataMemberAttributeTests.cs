using FluentAssertions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Routing;

namespace Swashbuckle.OData.Tests
{
    public class DataMemberAttributeTests
    {
        [Test]
        public async Task It_generates_a_valid_swagger_json_from_a_model_with_DataMemberAttribute()
        {
            Action<SwaggerDocsConfig> config = c => c.DocumentFilter<ApplyDataMemberAttributesDocumentation>();
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder =>
                        DataMemberAttributeModelSetup.ConfigurationWithFormatters(appBuilder,
                            config,
                            typeof(DataMemberAttributeModelSetup.DataMemberAttributeModelsController))))
            {
                // Access swagger doc first
                await ValidationUtils.ValidateSwaggerJson();

            }

        }
    }

    public class ApplyDataMemberAttributesDocumentation : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.tags = new List<Tag>
            {
                new Tag { name = "DataMemberAttributeModels", description = "A Simple Model with a DataMemberAttribute on in." }
            };
        }
    }

    public class DataMemberAttributeModelSetup
    {
        public static void Configuration(IAppBuilder appBuilder, Action<SwaggerDocsConfig> unitTestConfigs, params Type[] targetControllers)
        {
            var config = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };

            config = ConfigureWebApi(config);

            ConfigureOData(appBuilder, targetControllers, config, unitTestConfigs);

            config.EnsureInitialized();
        }

        public static void ConfigurationWithFormatters(IAppBuilder appBuilder, Action<SwaggerDocsConfig> unitTestConfigs, params Type[] targetControllers)
        {
            var config = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };

            config = ConfigureWebApi(config);

            var oDataRoute = ConfigureOData(appBuilder, targetControllers, config, unitTestConfigs);
            var rootContainer = config.GetODataRootContainer(oDataRoute);

            config.Formatters.InsertRange(0, ODataMediaTypeFormatters.Create(new DefaultODataSerializerProvider(rootContainer), new DefaultODataDeserializerProvider(rootContainer)));

            config.EnsureInitialized();
        }

        public static HttpConfiguration ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            return config;
        }

        private static ODataRoute ConfigureOData(IAppBuilder appBuilder, Type[] targetController, HttpConfiguration config, Action<SwaggerDocsConfig> swaggerDocsConfig)
        {
            config = appBuilder.ConfigureHttpConfig(config, swaggerDocsConfig, null, targetController);

            return config.MapODataServiceRoute("odata", "odata", GetEdmModel());
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<DataMemberAttributeModel>("DataMemberAttributeModels");

            return builder.GetEdmModel();
        }
        [DataContract]
        public class DataMemberAttributeModel
        {
            [DataMember(Name = "id")]
            [Key]
            public int Id { get; set; }
            [DataMember(Name = "differentName")]
            public string Variation { get; set; }
        }

        public class DataMemberAttributeModelsController : ODataController
        {
            [HttpGet]
            [ODataRoute("DataMemberAttributeModels")]
            [EnableQuery]
            public IQueryable<DataMemberAttributeModel> DataMemberAttributeModels()
            {
                IEnumerable<DataMemberAttributeModel> dataMemberAttributeModels = new[]
                {
                new DataMemberAttributeModel { Id=1, Variation = "a"},
                new DataMemberAttributeModel { Id=2, Variation = "b"},
                new DataMemberAttributeModel { Id=3, Variation = "c"},
                new DataMemberAttributeModel { Id=4, Variation = "d"}
                };
                return dataMemberAttributeModels.AsQueryable();
            }
        }
    }
}