using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;

namespace Swashbuckle.OData.Descriptions
{
    internal static class HttpParameterDescriptorExtensions
    {
        public static bool IsODataQueryOptions(this HttpParameterDescriptor parameterDescriptor)
        {
            Contract.Requires(parameterDescriptor != null);

            var parameterType = parameterDescriptor.ParameterType;
            Contract.Assume(parameterType != null);

            return parameterType.Name == "ODataQueryOptions`1";
        }
    }
}