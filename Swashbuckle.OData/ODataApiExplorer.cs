using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public class ODataApiExplorer : IApiExplorer
    {
        private readonly HttpConfiguration _config;
        private readonly Lazy<Collection<ApiDescription>> _apiDescriptions;
        private const string ServiceRoot = "http://any/";

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApiExplorer" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ODataApiExplorer(HttpConfiguration configuration)
        {
            _config = configuration;
            _apiDescriptions = new Lazy<Collection<ApiDescription>>(InitializeApiDescriptions);
        }

        /// <summary>
        /// Gets the API descriptions. The descriptions are initialized on the first access.
        /// </summary>
        public Collection<ApiDescription> ApiDescriptions
        {
            get { return _apiDescriptions.Value; }
        }

        private Collection<ApiDescription> InitializeApiDescriptions()
        {
            var apiDescriptions = new Collection<ApiDescription>();
            var controllerSelector = _config.Services.GetHttpControllerSelector();
            var controllerMappings = controllerSelector.GetControllerMapping();
            if (controllerMappings != null)
            {
                var descriptionComparer = new ApiDescriptionComparer();
                foreach (var route in FlattenRoutes(_config.Routes))
                {
                    var odataRoute = route as ODataRoute;
                    if (odataRoute != null)
                    {
                        var descriptionsFromRoute = ExploreRoute(controllerMappings, odataRoute);

                        // Remove ApiDescription that will lead to ambiguous action matching.
                        // E.g. a controller with Post() and PostComment(). When the route template is {controller}, it produces POST /controller and POST /controller.
                        descriptionsFromRoute = RemoveInvalidApiDescriptions(descriptionsFromRoute);

                        foreach (var description in descriptionsFromRoute)
                        {
                            // Do not add the description if the previous route has a matching description with the same HTTP method and relative path.
                            // E.g. having two routes with the templates "api/Values/{id}" and "api/{controller}/{id}" can potentially produce the same
                            // relative path "api/Values/{id}" but only the first one matters.
                            if (!apiDescriptions.Contains(description, descriptionComparer))
                            {
                                apiDescriptions.Add(description);
                            }
                        }
                    }
                }
            }

            return apiDescriptions;
        }

        /// <summary>
        /// Explores the route.
        /// </summary>
        /// <param name="controllerMappings">The controller mappings.</param>
        /// <param name="oDataRoute">The route.</param>
        /// <returns></returns>
        private Collection<ApiDescription> ExploreRoute(IDictionary<string, HttpControllerDescriptor> controllerMappings, ODataRoute oDataRoute)
        {
            var apiDescriptions = new Collection<ApiDescription>();

            var edmSwaggerDocument = GetDefaultEdmSwaggerDocument(oDataRoute);

            foreach (var potentialPath in edmSwaggerDocument.paths)
            {
                apiDescriptions.AddRange(ExplorePath(controllerMappings, oDataRoute, potentialPath.Key, potentialPath.Value));
            }

            return apiDescriptions;
        }

        private Collection<ApiDescription> ExplorePath(IDictionary<string, HttpControllerDescriptor> controllerMappings, ODataRoute oDataRoute, string potentialPath, PathItem potentialOperations)
        {
            var apiDescriptions = new Collection<ApiDescription>();

            var edmModel = GetEdmModel(oDataRoute);

            HttpControllerDescriptor controllerDescriptor;
            if (TryGetControllerDesciptor(controllerMappings, oDataRoute, potentialPath, out controllerDescriptor))
            {
                apiDescriptions.AddIfNotNull(GetApiDescription(HttpMethod.Delete, oDataRoute, potentialPath, potentialOperations.delete, controllerDescriptor, edmModel));
                apiDescriptions.AddIfNotNull(GetApiDescription(HttpMethod.Get, oDataRoute, potentialPath, potentialOperations.get, controllerDescriptor, edmModel));
            }

            return apiDescriptions;
        }

        private ApiDescription GetApiDescription(HttpMethod httpMethod, ODataRoute oDataRoute, string potentialPath, Operation operation, HttpControllerDescriptor controllerDescriptor, IEdmModel edmModel)
        {
            if (operation != null)
            {
                var oDataPathRouteConstraint = GetODataPathRouteConstraint(oDataRoute);

                var entitySetPath = oDataPathRouteConstraint.PathHandler.Parse(edmModel, ServiceRoot, potentialPath);

                var controllerContext = new HttpControllerContext
                {
                    Request = new HttpRequestMessage(httpMethod, ServiceRoot)
                };

                var actionSelector = _config.Services.GetActionSelector();
                var actionMappings = actionSelector.GetActionMapping(controllerDescriptor);
                var action = SelectAction(oDataPathRouteConstraint, entitySetPath, controllerContext, actionMappings);

                return null;
            }
            return null;
        }

        private static bool TryGetControllerDesciptor(IDictionary<string, HttpControllerDescriptor> controllerMappings, ODataRoute oDataRoute, string potentialPath, out HttpControllerDescriptor controllerDescriptor)
        {
            var oDataPathRouteConstraint = GetODataPathRouteConstraint(oDataRoute);

            var model = GetEdmModel(oDataRoute);


            var oDataPathHandler = oDataPathRouteConstraint.PathHandler;

            var entitySetPath = oDataPathHandler.Parse(model, ServiceRoot, potentialPath);

            var controllerName = SelectControllerName(oDataPathRouteConstraint, entitySetPath);

            if (controllerName != null)
            {
                return controllerMappings.TryGetValue(controllerName, out controllerDescriptor);
            }
            controllerDescriptor = null;
            return false;
        }

        /// <summary>
        /// Selects the name of the controller to dispatch the request to.
        /// </summary>
        /// <param name="oDataPathRouteConstraint">The o data path route constraint.</param>
        /// <param name="path">The OData path of the request.</param>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The name of the controller to dispatch to, or <c>null</c> if one cannot be resolved.
        /// </returns>
        private static string SelectControllerName(ODataPathRouteConstraint oDataPathRouteConstraint, ODataPath path)
        {
            return oDataPathRouteConstraint.RoutingConventions
                .Select(routingConvention => routingConvention.SelectController(path, new HttpRequestMessage()))
                .FirstOrDefault(controllerName => controllerName != null);
        }

        private static string SelectAction(ODataPathRouteConstraint oDataPathRouteConstraint, ODataPath path, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            return oDataPathRouteConstraint.RoutingConventions
                .Select(routingConvention => routingConvention.SelectAction(path, controllerContext, actionMap))
                .FirstOrDefault(action => action != null);
        }

        private static SwaggerDocument GetDefaultEdmSwaggerDocument(ODataRoute oDataRoute)
        {
            var edmModel = GetEdmModel(oDataRoute);
            var oDataSwaggerConverter = new ODataSwaggerConverter(edmModel);
            return oDataSwaggerConverter.ConvertToSwaggerModel();
        }

        private static IEdmModel GetEdmModel(ODataRoute oDataRoute)
        {
            var oDataPathRouteConstraint = GetODataPathRouteConstraint(oDataRoute);
            var edmModel = oDataPathRouteConstraint.EdmModel;
            return edmModel;
        }

        private static ODataPathRouteConstraint GetODataPathRouteConstraint(ODataRoute oDataRoute)
        {
            return oDataRoute.Constraints.Values.SingleOrDefault(value => value is ODataPathRouteConstraint) as ODataPathRouteConstraint;
        }

        // remove ApiDescription that will lead to ambiguous action matching.
        private static Collection<ApiDescription> RemoveInvalidApiDescriptions(Collection<ApiDescription> apiDescriptions)
        {
            var duplicateApiDescriptionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visitedApiDescriptionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var description in apiDescriptions)
            {
                var apiDescriptionId = description.ID;
                if (visitedApiDescriptionIds.Contains(apiDescriptionId))
                {
                    duplicateApiDescriptionIds.Add(apiDescriptionId);
                }
                else
                {
                    visitedApiDescriptionIds.Add(apiDescriptionId);
                }
            }

            var filteredApiDescriptions = new Collection<ApiDescription>();
            foreach (var apiDescription in apiDescriptions)
            {
                var apiDescriptionId = apiDescription.ID;
                if (!duplicateApiDescriptionIds.Contains(apiDescriptionId))
                {
                    filteredApiDescriptions.Add(apiDescription);
                }
            }

            return filteredApiDescriptions;
        }

        private static IEnumerable<IHttpRoute> FlattenRoutes(IEnumerable<IHttpRoute> routes)
        {
            foreach (var route in routes)
            {
                var nested = route as IEnumerable<IHttpRoute>;
                if (nested != null)
                {
                    foreach (var subRoute in FlattenRoutes(nested))
                    {
                        yield return subRoute;
                    }
                }
                else
                {
                    yield return route;
                }
            }
        }

        private sealed class ApiDescriptionComparer : IEqualityComparer<ApiDescription>
        {
            public bool Equals(ApiDescription x, ApiDescription y)
            {
                return string.Equals(x.ID, y.ID, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(ApiDescription obj)
            {
                return obj.ID.ToUpperInvariant().GetHashCode();
            }
        }
    }
}