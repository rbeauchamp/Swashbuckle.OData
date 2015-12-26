using System.Collections.Generic;

namespace Swashbuckle.OData.Tests
{
    internal class ODataResponse<T>
    {
        public List<T> Value { get; set; }
    }
}