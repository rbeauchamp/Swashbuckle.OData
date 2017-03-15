using Newtonsoft.Json;

namespace Swashbuckle.OData
{
    public class ODataResponse<TValue>
    {
        [JsonProperty("@odata.context")]
        public virtual string ODataContext { get; set; }

        [JsonProperty("value")]
        public TValue Value { get; set; }        
    }
}