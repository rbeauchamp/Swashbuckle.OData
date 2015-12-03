// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.Http.Routing;

namespace Swashbuckle.OData.ApiExplorer
{
    public static class HttpRouteDataExtensions
    {
        /// <summary>
        ///     Remove all optional parameters that do not have a value from the route data.
        /// </summary>
        /// <param name="routeData">route data, to be mutated in-place.</param>
        public static void RemoveOptionalRoutingParameters(this IHttpRouteData routeData)
        {
            RemoveOptionalRoutingParameters(routeData.Values);

            var subRouteData = routeData.GetSubRoutes();
            if (subRouteData != null)
            {
                foreach (var sub in subRouteData)
                {
                    RemoveOptionalRoutingParameters(sub);
                }
            }
        }

        private static void RemoveOptionalRoutingParameters(IDictionary<string, object> routeValueDictionary)
        {
            Contract.Assert(routeValueDictionary != null);

            // Get all keys for which the corresponding value is 'Optional'.
            // Having a separate array is necessary so that we don't manipulate the dictionary while enumerating.
            // This is on a hot-path and linq expressions are showing up on the profile, so do array manipulation.
            var max = routeValueDictionary.Count;
            var i = 0;
            var matching = new string[max];
            foreach (var kv in routeValueDictionary)
            {
                if (kv.Value == RouteParameter.Optional)
                {
                    matching[i] = kv.Key;
                    i++;
                }
            }
            for (var j = 0; j < i; j++)
            {
                var key = matching[j];
                routeValueDictionary.Remove(key);
            }
        }

        /// <summary>
        ///     If a route is really a union of other routes, return the set of sub routes.
        /// </summary>
        /// <param name="routeData">a union route data</param>
        /// <returns>set of sub soutes contained within this route</returns>
        public static IEnumerable<IHttpRouteData> GetSubRoutes(this IHttpRouteData routeData)
        {
            IHttpRouteData[] subRoutes = null;
            if (routeData.Values.TryGetValue(RouteCollectionRoute.SubRouteDataKey, out subRoutes))
            {
                return subRoutes;
            }
            return null;
        }

        // If routeData is from an attribute route, get the action descriptors, order and precedence that it may match
        // to. Caller still needs to run action selection to pick the specific action.
        // Else return null.
        internal static CandidateAction[] GetDirectRouteCandidates(this IHttpRouteData routeData)
        {
            Contract.Assert(routeData != null);
            var subRoutes = routeData.GetSubRoutes();

            // Possible this is being called on a subroute. This can happen after ElevateRouteData. Just chain. 
            if (subRoutes == null)
            {
                if (routeData.Route == null)
                {
                    // If the matched route is a System.Web.Routing.Route (in web host) then routeData.Route
                    // will be null. Normally a System.Web.Routing.Route match would go through an MVC handler
                    // but we can get here through HttpRoutingDispatcher in WebAPI batching. If that happens, 
                    // then obviously it's not a WebAPI attribute routing match.
                    return null;
                }
                return routeData.Route.GetDirectRouteCandidates();
            }

            var list = new List<CandidateAction>();

            foreach (var subData in subRoutes)
            {
                var candidates = subData.Route.GetDirectRouteCandidates();
                if (candidates != null)
                {
                    list.AddRange(candidates);
                }
            }
            return list.ToArray();
        }
    }
}