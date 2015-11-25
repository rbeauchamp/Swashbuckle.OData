using System.Web.Http;
using Owin;
using Swashbuckle.Application;
using SwashbuckleODataSample;

namespace Swashbuckle.OData.Tests.WebHost
{
    public class TestWebApiStartup
    {
        public const string BaseAddress = "http://localhost:8347/";

        /// <summary>
        /// This code configures Web API. 
        /// The TestWebApiStartup class is specified as a type parameter in the WebApp.Start method.
        /// </summary>
        /// <param name="appBuilder">The application builder.</param>
        public void Configuration(IAppBuilder appBuilder)
        {
            var httpConfiguration = new HttpConfiguration();

            WebApiConfig.Register(httpConfiguration);

            httpConfiguration
                .EnableSwagger(c =>
                {
                    // Use "SingleApiVersion" to describe a single version API. Swagger 2.0 includes an "Info" object to
                    // hold additional metadata for an API. Version and title are required but you can also provide
                    // additional fields by chaining methods off SingleApiVersion.
                    //
                    c.SingleApiVersion("v1", "A title for your API");

                    // Wrap the default SwaggerGenerator with additional behavior (e.g. caching) or provide an
                    // alternative implementation for ISwaggerProvider with the CustomProvider option.
                    //
                    c.CustomProvider(defaultProvider => new ODataSwaggerProvider(() => httpConfiguration));
                })
                .EnableSwaggerUi();

            appBuilder.UseWebApi(httpConfiguration);

            httpConfiguration.EnsureInitialized();
        }
    }
}