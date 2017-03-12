using Newtonsoft.Json;
using System.Collections.Generic;

namespace Swashbuckle.OData
{
    public class ODataListResponse<TValue> : ODataResponse<List<TValue>>
    {
        [JsonProperty("@odata.nextLink")]
        public virtual string ODataNextLink { get; set; }

        [JsonProperty("@odata.count")]
        public virtual int? ODataCount { get; set; }
    }
}