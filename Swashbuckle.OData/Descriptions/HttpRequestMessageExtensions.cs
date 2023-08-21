using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace Swashbuckle.OData.Descriptions
{
    internal static class HttpRequestMessageExtensions
    {
        public static HttpActionDescriptor GetHttpActionDescriptor(this HttpRequestMessage request, HttpConfiguration httpConfig)
        {
            Contract.Ensures(Contract.Result<HttpActionDescriptor>() == null || Contract.Result<HttpActionDescriptor>().ControllerDescriptor != null);

            HttpActionDescriptor actionDescriptor = null;

            var controllerDescriptor = request.GetControllerDescriptor();

            if (controllerDescriptor != null)
            {
                actionDescriptor = controllerDescriptor.ControllerName == "Restier"
                    ? GetHttpActionDescriptorForRestierController(request, controllerDescriptor)
                    : GetHttpActionDescriptorForODataController(request, httpConfig, controllerDescriptor);
            }

            Contract.Assume(actionDescriptor == null || actionDescriptor.ControllerDescriptor != null);

            return actionDescriptor;
        }

        private static HttpActionDescriptor GetHttpActionDescriptorForODataController(HttpRequestMessage request, HttpConfiguration httpConfig, HttpControllerDescriptor controllerDescriptor)
        {
            HttpActionDescriptor actionDescriptor;

            var perControllerConfig = controllerDescriptor.Configuration;
            Contract.Assume(perControllerConfig != null);

            request.SetConfiguration(perControllerConfig);
            var requestContext = request.GetRequestContext();
            requestContext.Configuration = perControllerConfig;
            requestContext.RouteData = request.GetRouteData();
            requestContext.Url = new UrlHelper(request);
            requestContext.VirtualPathRoot = perControllerConfig.VirtualPathRoot;

            try
            {
                var controllerContext = new HttpControllerContext(httpConfig, request.GetRouteData(), request)
                {
                    ControllerDescriptor = controllerDescriptor,
                    RequestContext = requestContext
                };

                var actionSelector = perControllerConfig.Services?.GetActionSelector();
                Contract.Assume(actionSelector != null);
                actionDescriptor = actionSelector.SelectAction(controllerContext);
            }
            catch (HttpResponseException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound || ex.Response.StatusCode == HttpStatusCode.MethodNotAllowed)
                {
                    actionDescriptor = null;
                }
                else
                {
                    throw;
                }
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return actionDescriptor;
        }

        private static HttpActionDescriptor GetHttpActionDescriptorForRestierController(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor)
        {
            HttpActionDescriptor actionDescriptor;

            var perControllerConfig = controllerDescriptor.Configuration;
            request.SetConfiguration(perControllerConfig);
            var requestContext = request.GetRequestContext();
            requestContext.Configuration = perControllerConfig;
            requestContext.RouteData = request.GetRouteData();
            requestContext.Url = new UrlHelper(request);
            requestContext.VirtualPathRoot = perControllerConfig.VirtualPathRoot;

            var controller = controllerDescriptor.CreateController(request);

            using (controller as IDisposable)
            {
                var controllerContext = new HttpControllerContext(requestContext, request, controllerDescriptor, controller);
                try
                {
                    actionDescriptor = perControllerConfig.Services.GetActionSelector().SelectAction(controllerContext);
                }
                catch (HttpResponseException ex)
                {
                    if (ex.Response.StatusCode == HttpStatusCode.NotFound || ex.Response.StatusCode == HttpStatusCode.MethodNotAllowed)
                    {
                        actionDescriptor = null;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return actionDescriptor;
        }

        public static HttpControllerDescriptor GetControllerDescriptor(this HttpRequestMessage request)
        {
            Contract.Requires(request.GetConfiguration() != null);
            Contract.Requires(request.GetConfiguration().Services.GetHttpControllerSelector() != null);

            try
            {
                return request.GetConfiguration().Services.GetHttpControllerSelector().SelectController(request);
            }
            catch (HttpResponseException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }
    }
}