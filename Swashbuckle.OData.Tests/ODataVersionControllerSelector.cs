using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;

namespace Swashbuckle.OData.Tests
{
    /// <summary>
    /// Controllerselector that figures out the version suffix from the route name.
    /// For example: request from route V1 can be dispatched to ProductsV1Controller.
    /// </summary>
    public class UnitTestODataVersionControllerSelector : UnitTestControllerSelector
    {
        public UnitTestODataVersionControllerSelector(HttpConfiguration configuration, params Type[] targetControllers)
            : base(configuration, targetControllers)
        {
        }

        public Dictionary<string, string> RouteVersionSuffixMapping { get; } = new Dictionary<string, string>();

        public override string GetControllerName(HttpRequestMessage request)
        {
            var controllerName = base.GetControllerName(request);
            if (string.IsNullOrEmpty(controllerName))
            {
                return controllerName;
            }

            var routeName = request.ODataProperties().RouteName;
            if (string.IsNullOrEmpty(routeName))
            {
                return controllerName;
            }

            var mapping = GetControllerMapping();

            if (!RouteVersionSuffixMapping.ContainsKey(routeName))
            {
                return controllerName;
            }

            var versionControllerName = controllerName + RouteVersionSuffixMapping[routeName];
            return mapping.ContainsKey(versionControllerName)
                ? versionControllerName
                : controllerName;
        }
    }
}