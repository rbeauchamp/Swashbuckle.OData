using System.Diagnostics.Contracts;
using System.Web.OData.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public class SwaggerRoute
    {
        public SwaggerRoute(string template, ODataRoute oDataRoute, PathItem pathItem)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(template));
            Contract.Requires(pathItem != null);
            Contract.Requires(oDataRoute != null);

            Template = template;
            ODataRoute = oDataRoute;
            PathItem = pathItem;
        }

        public SwaggerRoute(string template, ODataRoute oDataRoute) :
            this(template, oDataRoute, new PathItem())
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(template));
            Contract.Requires(oDataRoute != null);
        }

        public string Template { get; }

        public ODataRoute ODataRoute { get; }

        public PathItem PathItem { get; }
    }
}