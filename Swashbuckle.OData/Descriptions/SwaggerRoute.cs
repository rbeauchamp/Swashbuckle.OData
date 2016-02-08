using System.Diagnostics.Contracts;
using System.Web;
using System.Web.OData.Routing;
using Flurl;
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
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return _template;
            }
        }

        public string PrefixedTemplate
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return HttpUtility.UrlDecode(ODataRoute.GetRoutePrefix().AppendPathSegment(_template));
            }
        }

        public ODataRoute ODataRoute
        {
            get
            {
                Contract.Ensures(Contract.Result<ODataRoute>() != null);
                return _oDataRoute;
            }
        }

        public PathItem PathItem
        {
            get
            {
                Contract.Ensures(Contract.Result<PathItem>() != null);
                return _pathItem;
            }
        }
    }
}