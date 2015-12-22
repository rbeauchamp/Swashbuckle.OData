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
        public static HttpConfiguration GetStandardHttpConfig(this IAppBuilder appBuilder, Type targetController, Action<SwaggerDocsConfig> unitTestConfigs = null)
        {
            var config = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };
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
                c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, config));

                // Apply test-specific configs
                unitTestConfigs?.Invoke(c);

            }).EnableSwaggerUi();

            FormatterConfig.Register(config);

            config.Services.Replace(typeof (IHttpControllerSelector), new UnitTestControllerSelector(config, targetController));

            return config;
        }
    }
}