using System.Web.Http;
using Newtonsoft.Json;

namespace Swashbuckle.OData
{
    public static class HttpConfigurationExtensions
    {
        internal static JsonSerializerSettings SerializerSettingsOrDefault(this HttpConfiguration httpConfig)
        {
            var formatter = httpConfig.Formatters.JsonFormatter;
            return formatter != null 
                ? formatter.SerializerSettings 
                : new JsonSerializerSettings();
        }
    }
}