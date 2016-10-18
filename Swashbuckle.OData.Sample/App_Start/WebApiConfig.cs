using System.Web.Configuration;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace SwashbuckleODataSample
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            bool isPrefixFreeEnabled = System.Convert.ToBoolean(
                        WebConfigurationManager.AppSettings["EnableEnumPrefixFree"]);
            config.EnableDependencyInjection(builder => builder.AddService(ServiceLifetime.Singleton, sp => isPrefixFreeEnabled ? new StringAsEnumResolver() : new ODataUriResolver()));
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });
        }
    }
}