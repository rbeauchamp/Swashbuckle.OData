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
        public static HttpActionDescriptor GetHttpActionDescriptor(this HttpRequestMessage request, HttpConfiguration httpConfig)
        {
            HttpActionDescriptor actionDescriptor = null;

            var controllerDescriptor = request.GetControllerDescriptor();

            if (controllerDescriptor != null)
            {
                var perControllerConfig = controllerDescriptor.Configuration;
                request.SetConfiguration(perControllerConfig);

                var controllerContext = new HttpControllerContext(httpConfig, request.GetRouteData(), request)
                {
                    ControllerDescriptor = controllerDescriptor
                };

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