using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataActionDescriptorEqualityComparer : IEqualityComparer<ODataActionDescriptor>
    {
        public bool Equals(ODataActionDescriptor x, ODataActionDescriptor y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            return x.Request.Method.Equals(y.Request.Method)
                   && string.Equals(NormalizeRelativePath(x.RelativePathTemplate), NormalizeRelativePath(y.RelativePathTemplate), StringComparison.OrdinalIgnoreCase)
                   && x.ActionDescriptor.Equals(y.ActionDescriptor);
        }

        public int GetHashCode(ODataActionDescriptor obj)
        {
            var hashCode = obj.Request.Method.GetHashCode();
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizeRelativePath(obj.RelativePathTemplate));
            hashCode = (hashCode * 397) ^ obj.ActionDescriptor.GetHashCode();
            return hashCode;
        }

        private static string NormalizeRelativePath(string path)
        {
            Contract.Requires(path != null);

            return path.Replace("()", string.Empty);
        }
    }
}