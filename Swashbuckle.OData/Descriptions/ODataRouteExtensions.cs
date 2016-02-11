using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace Swashbuckle.OData.Descriptions
{
    internal static class ODataRouteExtensions
    {
        public static IEdmModel GetEdmModel(this ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Ensures(Contract.Result<IEdmModel>() != null);

            var result = oDataRoute.GetODataPathRouteConstraint().EdmModel;
            Contract.Assume(result != null);
            return result;
        }

        public static string GetRoutePrefix(this ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return oDataRoute.RoutePrefix ?? string.Empty;
        }

        public static ODataPathRouteConstraint GetODataPathRouteConstraint(this ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Ensures(Contract.Result<ODataPathRouteConstraint>() != null);

            Contract.Assume(oDataRoute.Constraints != null);
            Contract.Assume(oDataRoute.Constraints.Values.Count > 0);
            var result = (ODataPathRouteConstraint)oDataRoute.Constraints.Values.Single(value => value is ODataPathRouteConstraint);
            Contract.Assume(result != null);
            return result;
        }
    }
}