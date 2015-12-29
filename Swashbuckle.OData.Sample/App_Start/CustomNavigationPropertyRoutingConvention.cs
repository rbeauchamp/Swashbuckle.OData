using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using SwashbuckleODataSample.ODataControllers;

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
                    controllerContext.RouteData.Values["orderID"] = (odataPath.Segments[1] as KeyValuePathSegment).Value;
                    return controllerContext.Request.Method.ToString();
                }
            }
            else if (typeof(OrdersController) == controllerType)
            {
                if (odataPath.PathTemplate.Equals("~/entityset/key/navigation")) //POST OR GET
                {
                    controllerContext.RouteData.Values["customerID"] = (odataPath.Segments[1] as KeyValuePathSegment).Value;
                    return controllerContext.Request.Method.ToString();
                }
                if (odataPath.PathTemplate.Equals("~/entityset/key/navigation/key")) //PATCH OR DELETE
                {
                    controllerContext.RouteData.Values["customerID"] = (odataPath.Segments[1] as KeyValuePathSegment).Value;

                    controllerContext.RouteData.Values["key"] = (odataPath.Segments[3] as KeyValuePathSegment).Value;
                    return controllerContext.Request.Method.ToString();
                }
            }

            return base.SelectAction(odataPath, controllerContext, actionMap);
        }

        public override string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            // We use always use the last naviation as the controller vs. the initial entityset
            if (odataPath.PathTemplate.Contains("~/entityset/key/navigation"))
            {
                // Find controller.  Controller should be last navigation property
                return ODataSegmentKinds.Navigation == odataPath.Segments[odataPath.Segments.Count - 1].SegmentKind
                    ? odataPath.Segments[odataPath.Segments.Count - 1].ToString()
                    : odataPath.Segments[odataPath.Segments.Count - 2].ToString();
            }
            return base.SelectController(odataPath, request);
        }
    }
}