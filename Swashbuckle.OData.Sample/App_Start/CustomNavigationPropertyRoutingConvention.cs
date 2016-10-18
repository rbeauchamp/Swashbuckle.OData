using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.UriParser;
using SwashbuckleODataSample.ODataControllers;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace SwashbuckleODataSample
{
    public class CustomNavigationPropertyRoutingConvention : EntitySetRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            var controllerType = controllerContext.ControllerDescriptor.ControllerType;

            if (typeof(CustomersController) == controllerType)
            {
                if (odataPath.PathTemplate.Equals("~/entityset/key/navigation")) //POST OR GET
                {
                    controllerContext.RouteData.Values["orderID"] = ((KeySegment)odataPath.Segments[1]).Keys.Single().Value;
                    return controllerContext.Request.Method.ToString();
                }
            }
            else if (typeof(OrdersController) == controllerType)
            {
                if (odataPath.PathTemplate.Equals("~/entityset/key/navigation")) //POST OR GET
                {
                    controllerContext.RouteData.Values["customerID"] = ((KeySegment)odataPath.Segments[1]).Keys.Single().Value;
                    return controllerContext.Request.Method.ToString();
                }
                if (odataPath.PathTemplate.Equals("~/entityset/key/navigation/key")) //PATCH OR DELETE
                {
                    controllerContext.RouteData.Values["customerID"] = ((KeySegment)odataPath.Segments[1]).Keys.Single().Value;

                    controllerContext.RouteData.Values["key"] = ((KeySegment)odataPath.Segments[3]).Keys.Single().Value;
                    return controllerContext.Request.Method.ToString();
                }
            }

            return base.SelectAction(odataPath, controllerContext, actionMap);
        }

        public override string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            // We use always use the last navigation as the controller vs. the initial entityset
            if (odataPath.PathTemplate.Contains("~/entityset/key/navigation"))
            {
                // Find controller.  Controller should be last navigation property
                return odataPath.Segments[odataPath.Segments.Count - 1] is NavigationPropertySegment
                    ? odataPath.Segments[odataPath.Segments.Count - 1].Identifier
                    : odataPath.Segments[odataPath.Segments.Count - 2].Identifier;
            }
            return base.SelectController(odataPath, request);
        }
    }
}