using System.Web.Configuration;
using System.Web.Http;
using System.Web.OData.Extensions;

namespace SwashbuckleODataSample
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            bool isPrefixFreeEnabled = System.Convert.ToBoolean(
                        WebConfigurationManager.AppSettings["EnableEnumPrefixFree"]);
            config.EnableEnumPrefixFree(isPrefixFreeEnabled);
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });
        }
    }
}