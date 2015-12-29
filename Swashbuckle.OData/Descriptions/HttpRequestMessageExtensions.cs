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
        public static HttpActionDescriptor GetHttpActionDescriptor(this HttpRequestMessage request)
        {
            Contract.Requires(request.GetRequestContext() != null);
            Contract.Ensures(Contract.Result<HttpActionDescriptor>() == null || Contract.Result<HttpActionDescriptor>().ControllerDescriptor != null);

            HttpActionDescriptor actionDescriptor = null;

            var controllerDescriptor = request.GetControllerDesciptor();

            if (controllerDescriptor != null)
            {

                var perControllerConfig = controllerDescriptor.Configuration;
                request.SetConfiguration(perControllerConfig);
                var requestContext = request.GetRequestContext();
                Contract.Assume(requestContext != null);
                requestContext.Configuration = perControllerConfig;
                requestContext.RouteData = request.GetRouteData();
                requestContext.Url = new UrlHelper(request);
                Contract.Assume(perControllerConfig != null);
                requestContext.VirtualPathRoot = perControllerConfig.VirtualPathRoot;

                var controller = controllerDescriptor.CreateController(request);

                using (controller as IDisposable)
                {
                    var controllerContext = new HttpControllerContext(requestContext, request, controllerDescriptor, controller);
                    try
                    {
                        actionDescriptor = perControllerConfig.Services.GetActionSelector()?.SelectAction(controllerContext);
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
            }

            Contract.Assume(actionDescriptor == null || actionDescriptor.ControllerDescriptor != null);

            return actionDescriptor;
        }

        public static HttpControllerDescriptor GetControllerDesciptor(this HttpRequestMessage request)
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