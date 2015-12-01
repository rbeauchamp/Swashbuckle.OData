using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;

namespace Swashbuckle.OData
{
    public class ODataApiExplorer : ApiExplorer
    {
        public ODataApiExplorer(HttpConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        ///     Determines whether the controller should be considered for <see cref="ApiExplorer.ApiDescriptions" /> generation.
        ///     Called when initializing the <see cref="ApiExplorer.ApiDescriptions" />.
        /// </summary>
        /// <param name="controllerVariableValue">The controller variable value from the route.</param>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <param name="route">The route.</param>
        /// <returns>
        ///     <c>true</c> if the controller should be considered for <see cref="ApiExplorer.ApiDescriptions" /> generation,
        ///     <c>false</c> otherwise.
        /// </returns>
        public override bool ShouldExploreController(string controllerVariableValue, HttpControllerDescriptor controllerDescriptor, IHttpRoute route)
        {
            Contract.Requires(controllerDescriptor != null);
            Contract.Requires(route != null);

            //var setting = controllerDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();

            //return (setting == null || !setting.IgnoreApi) && MatchRegexConstraint(route, RouteValueKeys.Controller, controllerVariableValue);
            return MatchRegexConstraint(route, RouteValueKeys.Controller, controllerVariableValue);
        }

        private static bool MatchRegexConstraint(IHttpRoute route, string parameterName, string parameterValue)
        {
            var constraints = route.Constraints;
            if (constraints != null)
            {
                object constraint;
                if (constraints.TryGetValue(parameterName, out constraint))
                {
                    // treat the constraint as a string which represents a Regex.
                    // note that we don't support custom constraint (IHttpRouteConstraint) because it might rely on the request and some runtime states
                    var constraintsRule = constraint as string;
                    if (constraintsRule != null)
                    {
                        var constraintsRegEx = "^(" + constraintsRule + ")$";
                        return parameterValue != null && Regex.IsMatch(parameterValue, constraintsRegEx, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    }
                }
            }

            return true;
        }
    }
}