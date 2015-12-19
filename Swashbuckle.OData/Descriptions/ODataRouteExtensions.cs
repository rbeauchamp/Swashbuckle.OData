using System.Linq;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace Swashbuckle.OData.Descriptions
{
    public static class ODataRouteExtensions
    {
        public static IEdmModel GetEdmModel(this ODataRoute oDataRoute)
        {
            return oDataRoute.GetODataPathRouteConstraint().EdmModel;
        }

        public static ODataPathRouteConstraint GetODataPathRouteConstraint(this ODataRoute oDataRoute)
        {
            return oDataRoute.Constraints.Values.SingleOrDefault(value => value is ODataPathRouteConstraint) as ODataPathRouteConstraint;
        }
    }
}