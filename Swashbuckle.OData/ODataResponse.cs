using System.Collections.Generic;
using Newtonsoft.Json;

namespace Swashbuckle.OData
{
    public class ODataResponse<T>
    {
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }

        [JsonProperty("value")]
        public List<T> Value { get; set; }
    }
}