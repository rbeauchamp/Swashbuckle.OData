using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Flurl;

namespace Swashbuckle.OData.Descriptions
{
    /// <summary>
    /// Creates ODataActionDescriptors from the set of ODataRoute attributes in the API.
    /// </summary>
    internal class AttributeRouteStrategy : IODataActionDescriptorExplorer
    {
        public IEnumerable<ODataActionDescriptor> Generate(HttpConfiguration httpConfig)
        {
            return httpConfig.GetODataRoutes().SelectMany(GetODataActionDescriptorsFromAttributeRoutes);
        }

        private static IEnumerable<ODataActionDescriptor> GetODataActionDescriptorsFromAttributeRoutes(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Requires(oDataRoute.Constraints != null);

            var attributeRoutingConvention = (AttributeRoutingConvention)oDataRoute
                .GetODataPathRouteConstraint()
                .RoutingConventions?
                .SingleOrDefault(convention => convention is AttributeRoutingConvention);

            if (attributeRoutingConvention != null)
            {
                return attributeRoutingConvention
                    .GetInstanceField<IDictionary<ODataPathTemplate, HttpActionDescriptor>>("_attributeMappings", true)
                    .Select(pair => GetODataActionDescriptorFromAttributeRoute(pair.Value, oDataRoute))
                    .Where(descriptor => descriptor != null);
            }

            return new List<ODataActionDescriptor>();
        }

        private static ODataActionDescriptor GetODataActionDescriptorFromAttributeRoute(HttpActionDescriptor actionDescriptor, ODataRoute oDataRoute)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(oDataRoute != null);
            Contract.Ensures(Contract.Result<ODataActionDescriptor>() != null);

            var odataRouteAttribute = actionDescriptor.GetCustomAttributes<ODataRouteAttribute>()?.FirstOrDefault();
            Contract.Assume(odataRouteAttribute != null);
            var pathTemplate = HttpUtility.UrlDecode(oDataRoute.RoutePrefix.AppendPathSegment(odataRouteAttribute.PathTemplate));
            Contract.Assume(pathTemplate != null);
            return new ODataActionDescriptor(actionDescriptor, oDataRoute, pathTemplate);
        }
    }
}