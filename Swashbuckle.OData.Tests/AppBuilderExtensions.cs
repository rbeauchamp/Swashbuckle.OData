using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Owin;
using Swashbuckle.Application;
using SwashbuckleODataSample;

namespace Swashbuckle.OData.Tests
{
    public static class AppBuilderExtensions
    {
        public static HttpConfiguration GetStandardHttpConfig(this IAppBuilder appBuilder, params Type[] targetControllers)
        {
            return GetStandardHttpConfig(appBuilder, null, null, targetControllers);
        }

        public static HttpConfiguration GetStandardHttpConfig(this IAppBuilder appBuilder, Action<SwaggerDocsConfig> swaggerDocsConfig, Action<ODataSwaggerDocsConfig> odataSwaggerDocsConfig, params Type[] targetControllers)
        {
            var config = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };
            return ConfigureHttpConfig(appBuilder, config, swaggerDocsConfig, odataSwaggerDocsConfig, targetControllers);
        }

        public static HttpConfiguration ConfigureHttpConfig(this IAppBuilder appBuilder, HttpConfiguration config, Action<SwaggerDocsConfig> swaggerDocsConfig, Action<ODataSwaggerDocsConfig> odataSwaggerDocsConfig, params Type[] targetControllers)
        {
            var server = new HttpServer(config);
            appBuilder.UseWebApi(server);
            config.EnableSwagger(c =>
            {
                // Use "SingleApiVersion" to describe a single version API. Swagger 2.0 includes an "Info" object to
                // hold additional metadata for an API. Version and title are required but you can also provide
                // additional fields by chaining methods off SingleApiVersion.
                //
                c.SingleApiVersion("v1", "A title for your API");

                // Wrap the default SwaggerGenerator with additional behavior (e.g. caching) or provide an
                // alternative implementation for ISwaggerProvider with the CustomProvider option.
                //
                c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, config).Configure(odataSwaggerDocsConfig));

                // Apply test-specific configs
                swaggerDocsConfig?.Invoke(c);
            }).EnableSwaggerUi();

            FormatterConfig.Register(config);

            config.Services.Replace(typeof (IHttpControllerSelector), new UnitTestControllerSelector(config, targetControllers));

            return config;
        }
    }
}