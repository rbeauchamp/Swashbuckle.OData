using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace SwashbuckleODataSample
{
    /// <summary>
    /// </summary>
    public static class FormatterConfig
    {
        /// <summary>
        /// Registers the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void Register(HttpConfiguration config)
        {
            var formatters = config.Formatters;

            formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            };

            formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
        }
    }
}