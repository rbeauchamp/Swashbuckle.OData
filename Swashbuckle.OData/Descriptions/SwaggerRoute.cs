using System.Diagnostics.Contracts;
using System.Web.OData.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public class SwaggerRoute
    {
        private readonly string _template;
        private readonly ODataRoute _oDataRoute;
        private readonly PathItem _pathItem;

        public SwaggerRoute(string template, ODataRoute oDataRoute, PathItem pathItem)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(template));
            Contract.Requires(pathItem != null);
            Contract.Requires(oDataRoute != null);

            _template = template;
            _oDataRoute = oDataRoute;
            _pathItem = pathItem;
        }

        public SwaggerRoute(string template, ODataRoute oDataRoute) :
            this(template, oDataRoute, new PathItem())
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(template));
            Contract.Requires(oDataRoute != null);
        }

        public string Template
        {
            get { return _template; }
        }

        public ODataRoute ODataRoute
        {
            get { return _oDataRoute; }
        }

        public PathItem PathItem
        {
            get { return _pathItem; }
        }

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(!string.IsNullOrWhiteSpace(Template));
            Contract.Invariant(ODataRoute != null);
            Contract.Invariant(PathItem != null);
        }
    }
}