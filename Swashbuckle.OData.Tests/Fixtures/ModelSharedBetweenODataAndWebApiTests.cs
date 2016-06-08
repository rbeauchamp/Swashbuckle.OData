using System;
using System.Collections.Generic;
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
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using FluentAssertions;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    [TestFixture]
    public class ModelSharedBetweenODataAndWebApiTests
    {
        [Test]
        public async Task It_consolidates_tags_in_final_swagger_model()
        {
            Action<SwaggerDocsConfig> config = c => c.DocumentFilter<ApplySharedModelsDocumentation>();
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => SharedModelsSetup.Configuration(appBuilder, config, typeof(SharedModelsSetup.SharedModelsController), typeof(SharedModelsSetup.SharedModelsWebApiController))))
            {
                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the OData route in the test controller is valid
                var results = await httpClient.GetJsonAsync<ODataResponse<List<SharedModelsSetup.SharedModel>>>("odata/SharedModels");
                results.Should().NotBeNull();
                results.Value.Count.Should().Be(4);

                // Verify that the WebApi route in the test controller is valid
                var webApiResults = await httpClient.GetJsonAsync<List<SharedModelsSetup.SharedModel>>("SharedModels");
                webApiResults.Should().NotBeNull();
                webApiResults.Count.Should().Be(4);

                // Act and Assert
                await ValidationUtils.ValidateSwaggerJson();
            }
        }

        [Test]
        public async Task It_serializes_web_api_model()
        {
            Action<SwaggerDocsConfig> config = c => c.DocumentFilter<ApplySharedModelsDocumentation>();
            using (WebApp.Start(HttpClientUtils.BaseAddress, appBuilder => SharedModelsSetup.ConfigurationWithFormatters(appBuilder, config, typeof(SharedModelsSetup.SharedModelsController), typeof(SharedModelsSetup.SharedModelsWebApiController))))
            {
                // Access swagger doc first
                await ValidationUtils.ValidateSwaggerJson();

                // Arrange
                var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);
                // Verify that the custom web api model can be serialized
                var webApiResults = await httpClient.GetJsonAsync<List<SharedModelsSetup.CustomApiModel>>("CustomApiModels");
                webApiResults.Should().NotBeNull();
                webApiResults.Count.Should().Be(2);
            }
        }
    }

    public class SharedModelsSetup
    {
        public static void Configuration(IAppBuilder appBuilder, Action<SwaggerDocsConfig> unitTestConfigs, params Type[] targetControllers)
        {
            var config = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };

            config = ConfigureWebApi(config);

            config = ConfigureOData(appBuilder, targetControllers, config, unitTestConfigs);

            config.EnsureInitialized();
        }

        public static void ConfigurationWithFormatters(IAppBuilder appBuilder, Action<SwaggerDocsConfig> unitTestConfigs, params Type[] targetControllers)
        {
            var config = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };

            config = ConfigureWebApi(config);

            config = ConfigureOData(appBuilder, targetControllers, config, unitTestConfigs);

            config.Formatters.InsertRange(0, ODataMediaTypeFormatters.Create(new NullSerializerProvider(), new DefaultODataDeserializerProvider()));

            config.EnsureInitialized();
        }

        public static HttpConfiguration ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            return config;
        }

        private static HttpConfiguration ConfigureOData(IAppBuilder appBuilder, Type[] targetController, HttpConfiguration config, Action<SwaggerDocsConfig> swaggerDocsConfig)
        {
            config = appBuilder.ConfigureHttpConfig(config, swaggerDocsConfig, null, targetController);

            config.MapODataServiceRoute("odata", "odata", GetEdmModel());

            return config;
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<SharedModel>("SharedModels");

            return builder.GetEdmModel();
        }

        public class NullSerializerProvider : DefaultODataSerializerProvider
        {
            private readonly NullEntityTypeSerializer _nullEntityTypeSerializer;

            public NullSerializerProvider()
            {
                _nullEntityTypeSerializer = new NullEntityTypeSerializer(this);
            }

            public override ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type, HttpRequestMessage request)
            {
                var serializer = base.GetODataPayloadSerializer(model, type, request);
                if (serializer == null)
                {
                    var functions = model.SchemaElements.Where(s => s.SchemaElementKind == EdmSchemaElementKind.Function
                                                                    || s.SchemaElementKind == EdmSchemaElementKind.Action);
                    var isFunctionCall = false;
                    foreach (var f in functions)
                    {
                        // ReSharper disable once UseStringInterpolation
                        var fname = string.Format("{0}.{1}", f.Namespace, f.Name);
                        if (request.RequestUri.OriginalString.Contains(fname))
                        {
                            isFunctionCall = true;
                            break;
                        }
                    }
                    // only, if it is not a function call
                    if (!isFunctionCall)
                    {
                        var response = request.GetOwinContext()?.Response;
                        response?.OnSendingHeaders(state =>
                        {
                            ((IOwinResponse)state).StatusCode = (int)HttpStatusCode.NotFound;
                        }, response);
                        // in case you are NOT using Owin, uncomment the following and comment everything above
                        // HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                    return _nullEntityTypeSerializer;
                }
                return serializer;
            }
        }

        public class NullEntityTypeSerializer : ODataEntityTypeSerializer
        {
            public NullEntityTypeSerializer(ODataSerializerProvider serializerProvider)
                : base(serializerProvider)
            { }
            public override void WriteObjectInline(object graph, IEdmTypeReference expectedType, ODataWriter writer, ODataSerializerContext writeContext)
            {
                if (graph != null)
                {
                    base.WriteObjectInline(graph, expectedType, writer, writeContext);
                }
            }
        }

        public class SharedModel
        {
            [Key]
            public int Id { get; set; }
            public string Variation { get; set; }
        }

        public class CustomApiModel
        {
            public int PropertyA { get; set; }
        }

        public class SharedModelsController : ODataController
        {
            [EnableQuery]
            public IQueryable<SharedModel> GetSharedModels()
            {
                IEnumerable<SharedModel> sharedModels = new[]
                {
                new SharedModel { Id=1, Variation = "a"},
                new SharedModel { Id=2, Variation = "b"},
                new SharedModel { Id=3, Variation = "c"},
                new SharedModel { Id=4, Variation = "d"}
                };
                return sharedModels.AsQueryable();
            }
        }

        public class SharedModelsWebApiController : ApiController
        {
            [Route("SharedModels")]
            public List<SharedModel> Get()
            {
                var sharedModels = new List<SharedModel>
                {
                new SharedModel { Id=1, Variation = "a"},
                new SharedModel { Id=2, Variation = "b"},
                new SharedModel { Id=3, Variation = "c"},
                new SharedModel { Id=4, Variation = "d"}
                };
                return sharedModels;
            }

            [Route("CustomApiModels")]
            public List<CustomApiModel> GetCustomApiModels()
            {
                var customApiModels = new List<CustomApiModel>
                {
                    new CustomApiModel {PropertyA = 1},
                    new CustomApiModel {PropertyA = 2}
                };
                return customApiModels;
            }
        }
    }

    /// <summary>
    /// Applies top-level Swagger documentation to the resources.
    /// </summary>
    public class ApplySharedModelsDocumentation : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.tags = new List<Tag>
            {
                new Tag { name = "SharedModels", description = "A resource shared between OData and WebApi controllers" }
            };
        }
    }
}