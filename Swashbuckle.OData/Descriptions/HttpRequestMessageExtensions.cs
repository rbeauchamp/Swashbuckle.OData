using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

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
                var perControllerConfig = controllerDescriptor.Configuration;
                Contract.Assume(perControllerConfig != null);

                request.SetConfiguration(perControllerConfig);

                var controllerContext = new HttpControllerContext(httpConfig, request.GetRouteData(), request)
                {
                    ControllerDescriptor = controllerDescriptor
                };

                try
                {
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
            }

            Contract.Assume(actionDescriptor == null || actionDescriptor.ControllerDescriptor != null);

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