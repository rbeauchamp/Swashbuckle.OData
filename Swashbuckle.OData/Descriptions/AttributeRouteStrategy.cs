using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.Extensions.DependencyInjection;

namespace Swashbuckle.OData.Descriptions
{
    /// <summary>
    /// Creates ODataActionDescriptors from the set of ODataRoute attributes in the API.
    /// </summary>
    internal class AttributeRouteStrategy : IODataActionDescriptorExplorer
    {
        public IEnumerable<ODataActionDescriptor> Generate(HttpConfiguration httpConfig)
        {
            return httpConfig.GetODataRoutes().SelectMany(oDataRoute => GetODataActionDescriptorsFromAttributeRoutes(oDataRoute, httpConfig));
        }

        private static IEnumerable<ODataActionDescriptor> GetODataActionDescriptorsFromAttributeRoutes(ODataRoute oDataRoute, HttpConfiguration httpConfig)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Requires(oDataRoute.Constraints != null);

            var rootContainer = httpConfig.GetODataRootContainer(oDataRoute);
            var routingConventions = rootContainer.GetServices<IODataRoutingConvention>();
            var attributeRoutingConvention = routingConventions.OfType<AttributeRoutingConvention>().SingleOrDefault();

            if (attributeRoutingConvention != null)
            {
                // AttributeRoutingConvention's private _attributeMapping field is not accessible any more due
                // to IWebApiActionDescriptor being internal - which rules out reflection. 

                //return attributeRoutingConvention
                //  .GetInstanceField<IDictionary<ODataPathTemplate, HttpActionDescriptor>>("_attributeMappings", true)
                return GetAttributeRoutingActionMap(httpConfig, attributeRoutingConvention)
                    .Select(pair => GetODataActionDescriptorFromAttributeRoute(pair.Value, oDataRoute, httpConfig))
                    .Where(descriptor => descriptor != null);
            }

            return new List<ODataActionDescriptor>();
        }

        // changes where taken from gitrepo https://github.com/andyward/Swashbuckle.OData Where Swashbuckle was extended with OData v4 support
        #region Copied Code from AttributeRoutingConvention
        private static IDictionary<ODataPathTemplate, HttpActionDescriptor> GetAttributeRoutingActionMap(HttpConfiguration httpConfig,
            AttributeRoutingConvention routingConvention)
        {
            IHttpControllerSelector controllerSelector = httpConfig.Services.GetHttpControllerSelector();
            return BuildAttributeMappings(controllerSelector.GetControllerMapping().Values, routingConvention);
        }

        private static IDictionary<ODataPathTemplate, HttpActionDescriptor> BuildAttributeMappings(ICollection<HttpControllerDescriptor> controllers,
            AttributeRoutingConvention routingConvention)
        {
            IDictionary<ODataPathTemplate, HttpActionDescriptor> attributeMappings =
                new Dictionary<ODataPathTemplate, HttpActionDescriptor>();

            foreach (HttpControllerDescriptor controller in controllers)
            {
                if (IsODataController(controller) && ShouldMapController(controller))
                {
                    IHttpActionSelector actionSelector = controller.Configuration.Services.GetActionSelector();
                    ILookup<string, HttpActionDescriptor> actionMapping = actionSelector.GetActionMapping(controller);
                    HttpActionDescriptor[] actions = actionMapping.SelectMany(a => a).ToArray();

                    foreach (string prefix in GetODataRoutePrefixes(controller))
                    {
                        foreach (HttpActionDescriptor action in actions)
                        {
                            IEnumerable<ODataPathTemplate> pathTemplates = // Invoke private method
                                routingConvention.InvokeFunction<IEnumerable<ODataPathTemplate>>("GetODataPathTemplates", prefix, action);
                            foreach (ODataPathTemplate pathTemplate in pathTemplates)
                            {
                                //attributeMappings.Add(pathTemplate, new WebApiActionDescriptor(action));
                                attributeMappings.Add(pathTemplate, action);
                            }
                        }
                    }
                }
            }

            return attributeMappings;
        }

        private static bool IsODataController(HttpControllerDescriptor controller)
        {
            return typeof(ODataController).IsAssignableFrom(controller.ControllerType);
        }

        public static bool ShouldMapController(HttpControllerDescriptor controller)
        {
            return true;
        }

        private static IEnumerable<string> GetODataRoutePrefixes(HttpControllerDescriptor controllerDescriptor)
        {
            Contract.Assert(controllerDescriptor != null);

            var prefixAttributes = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>(inherit: false);

            return GetODataRoutePrefixes(prefixAttributes, controllerDescriptor.ControllerType.FullName);
        }

        private static IEnumerable<string> GetODataRoutePrefixes(IEnumerable<ODataRoutePrefixAttribute> prefixAttributes, string controllerName)
        {
            Contract.Assert(prefixAttributes != null);

            if (!prefixAttributes.Any())
            {
                yield return null;
            }
            else
            {
                foreach (ODataRoutePrefixAttribute prefixAttribute in prefixAttributes)
                {
                    string prefix = prefixAttribute.Prefix;

                    if (prefix != null && prefix.StartsWith("/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("Route should not start with a /");//Error.InvalidOperation(SRResources.RoutePrefixStartsWithSlash, prefix, controllerName);
                    }

                    if (prefix != null && prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        prefix = prefix.TrimEnd('/');
                    }

                    yield return prefix;
                }
            }
        }

        #endregion  // Cloned from OData

        private static ODataActionDescriptor GetODataActionDescriptorFromAttributeRoute(HttpActionDescriptor actionDescriptor, ODataRoute oDataRoute,
            HttpConfiguration httpConfig)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(oDataRoute != null);
            Contract.Ensures(Contract.Result<ODataActionDescriptor>() != null);

            var odataRoutePrefixAttribute = actionDescriptor.ControllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>()?.FirstOrDefault();
            var odataRouteAttribute = actionDescriptor.GetCustomAttributes<ODataRouteAttribute>()?.FirstOrDefault();

            Contract.Assume(odataRouteAttribute != null);
            var pathTemplate = HttpUtility.UrlDecode(oDataRoute.GetRoutePrefix().AppendUriSegment(GetODataPathTemplate(odataRoutePrefixAttribute?.Prefix, odataRouteAttribute.PathTemplate)));
            Contract.Assume(pathTemplate != null);

            return new ODataActionDescriptor(actionDescriptor, oDataRoute, pathTemplate, CreateHttpRequestMessage(actionDescriptor, oDataRoute, httpConfig));
        }

        private static string GetODataPathTemplate(string prefix, string pathTemplate)
        {
            if (pathTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                return pathTemplate.Substring(1);
            }

            if (string.IsNullOrEmpty(prefix))
            {
                return pathTemplate;
            }

            if (prefix.StartsWith("/", StringComparison.Ordinal))
            {
                prefix = prefix.Substring(1);
            }

            if (string.IsNullOrEmpty(pathTemplate))
            {
                return prefix;
            }

            if (pathTemplate.StartsWith("(", StringComparison.Ordinal))
            {
                return prefix + pathTemplate;
            }

            return prefix + "/" + pathTemplate;
        }


        private static HttpRequestMessage CreateHttpRequestMessage(HttpActionDescriptor actionDescriptor, ODataRoute oDataRoute, HttpConfiguration httpConfig)
        {
            Contract.Requires(httpConfig != null);
            Contract.Requires(oDataRoute != null);
            Contract.Requires(httpConfig != null);
            Contract.Ensures(Contract.Result<HttpRequestMessage>() != null);

            Contract.Assume(oDataRoute.Constraints != null);

            var httpRequestMessage = new HttpRequestMessage(actionDescriptor.SupportedHttpMethods.First(), "http://any/");

            var requestContext = new HttpRequestContext
            {
                Configuration = httpConfig
            };
            httpRequestMessage.SetConfiguration(httpConfig);
            httpRequestMessage.SetRequestContext(requestContext);

            var httpRequestMessageProperties = httpRequestMessage.ODataProperties();
            Contract.Assume(httpRequestMessageProperties != null);
            httpRequestMessage.CreateRequestContainer(oDataRoute.PathRouteConstraint.RouteName);
            return httpRequestMessage;
        }
    }
}