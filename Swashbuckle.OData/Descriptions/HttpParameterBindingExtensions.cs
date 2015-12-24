using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace Swashbuckle.OData.Descriptions
{
    internal static class HttpParameterBindingExtensions
    {
        public static bool WillReadUri(this HttpParameterBinding parameterBinding)
        {
            Contract.Requires(parameterBinding != null);

            var valueProviderParameterBinding = parameterBinding as IValueProviderParameterBinding;
            if (valueProviderParameterBinding != null)
            {
                var valueProviderFactories = valueProviderParameterBinding.ValueProviderFactories;
                var providerFactories = valueProviderFactories as IList<ValueProviderFactory> ?? valueProviderFactories.ToList();
                if (providerFactories.Any() && providerFactories.All(factory => factory is IUriValueProviderFactory))
                {
                    return true;
                }
            }

            return false;
        }
    }
}