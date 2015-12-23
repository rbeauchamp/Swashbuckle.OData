using System;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.OData.Descriptions
{
    public class ApiDescriptionEqualityComparer : IEqualityComparer<ApiDescription>
    {
        public bool Equals(ApiDescription x, ApiDescription y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            return x.HttpMethod.Equals(y.HttpMethod)
                && string.Equals(NormalizeRelativePath(x.RelativePath), NormalizeRelativePath(y.RelativePath), StringComparison.OrdinalIgnoreCase)
                && x.ActionDescriptor.Equals(y.ActionDescriptor);
        }

        public int GetHashCode(ApiDescription obj)
        {
            var hashCode = obj.HttpMethod.GetHashCode();
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizeRelativePath(obj.RelativePath));
            hashCode = (hashCode * 397) ^ obj.ActionDescriptor.GetHashCode();
            return hashCode;
        }

        private string NormalizeRelativePath(string path)
        {
            return path.Replace("()", string.Empty);
        }
    }
}