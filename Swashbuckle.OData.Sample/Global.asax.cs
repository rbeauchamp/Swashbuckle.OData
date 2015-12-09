using System.Web;
using System.Web.Http;

namespace SwashbuckleODataSample
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(config =>
            {
                WebApiConfig.Register(config);
                ODataConfig.Register(config);
                FormatterConfig.Register(config);
            });
        }
    }
}