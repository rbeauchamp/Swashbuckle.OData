using System.Collections.Generic;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal class TagComparer : IEqualityComparer<Tag>
    {
        public bool Equals(Tag x, Tag y)
        {
            return ReferenceEquals(x, y) || string.Equals(x.name, y.name);
        }

        public int GetHashCode(Tag obj)
        {
            return obj.name?.GetHashCode() ?? 0;
        }
    }
}