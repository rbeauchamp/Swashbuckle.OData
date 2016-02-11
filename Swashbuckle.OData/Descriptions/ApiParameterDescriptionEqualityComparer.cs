using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace Swashbuckle.OData.Descriptions
{
    internal class ApiParameterDescriptionEqualityComparer : IEqualityComparer<ApiParameterDescription>
    {
        public bool Equals(ApiParameterDescription x, ApiParameterDescription y)
        {
            return ReferenceEquals(x, y)
                || RootParameterDescriptorsAreEqual(x, y)
                || string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        private static bool RootParameterDescriptorsAreEqual(ApiParameterDescription x, ApiParameterDescription y)
        {
            var xParameterDescriptor = GetRootParameterDescriptor(x);
            var yParameterDescriptor = GetRootParameterDescriptor(y);

            return xParameterDescriptor != null && yParameterDescriptor != null && ReferenceEquals(xParameterDescriptor, yParameterDescriptor);
        }

        private static HttpParameterDescriptor GetRootParameterDescriptor(ApiParameterDescription apiParameterDescription)
        {
            var oDataParameterDescriptor = apiParameterDescription.ParameterDescriptor as ODataParameterDescriptor;

            return oDataParameterDescriptor != null 
                ? oDataParameterDescriptor.ReflectedHttpParameterDescriptor 
                : apiParameterDescription.ParameterDescriptor;
        }

        public int GetHashCode(ApiParameterDescription obj)
        {
            // Make all HashCodes the same to force 
            // an Equals(ApiParameterDescription x, ApiParameterDescription y) check
            return 1;
        }
    }
}