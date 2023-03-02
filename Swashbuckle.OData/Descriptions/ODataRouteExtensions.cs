using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Web.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Swashbuckle.OData.Descriptions
{
    internal static class ODataRouteExtensions
    {
        private static readonly ConditionalWeakTable<ODataRoute, HttpConfiguration> RouteConfigurationTable = new ConditionalWeakTable<ODataRoute, HttpConfiguration>();

        public static void SetHttpConfiguration(this ODataRoute oDataRoute, HttpConfiguration httpConfig)
        {
            Contract.Requires(oDataRoute != null);
            HttpConfiguration registeredConfiguration;
            if (RouteConfigurationTable.TryGetValue(oDataRoute, out registeredConfiguration))
            {
                Contract.Assert(Equals(httpConfig, registeredConfiguration));
            }
            else
            {
                RouteConfigurationTable.Add(oDataRoute, httpConfig);
            }
        }

        private static HttpConfiguration GetHttpConfiguration(this ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Ensures(Contract.Result<HttpConfiguration>() != null);

            var result = RouteConfigurationTable.GetValue(oDataRoute, key => null);
            Contract.Assume(result != null);
            return result;
        }

        private static IServiceProvider GetRootContainer(this ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            return oDataRoute.GetHttpConfiguration().GetODataRootContainer(oDataRoute);
        }

        public static IEdmModel GetEdmModel(this ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            return oDataRoute.GetRootContainer().GetRequiredService<IEdmModel>();
        }

        public static bool IsEnumPrefixFree(this ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            var uriResolver = oDataRoute.GetRootContainer().GetRequiredService<ODataUriResolver>();
            return uriResolver is StringAsEnumResolver;
        }

        public static string GetRoutePrefix(this ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return oDataRoute.RoutePrefix ?? string.Empty;
        }
    }
}