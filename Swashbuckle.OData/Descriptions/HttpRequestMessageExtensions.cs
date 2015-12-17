using System;
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
            HttpActionDescriptor actionDescriptor = null;

            var controllerDescriptor = request.GetControllerDesciptor();

            if (controllerDescriptor != null)
            {

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
            }

            return actionDescriptor;
        }

        public static HttpControllerDescriptor GetControllerDesciptor(this HttpRequestMessage request)
        {
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