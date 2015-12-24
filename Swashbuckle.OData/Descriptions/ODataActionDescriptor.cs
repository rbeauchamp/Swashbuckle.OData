using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataActionDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionDescriptor"/> class.
        /// </summary>
        /// <param name="actionDescriptor">The HTTP action descriptor.</param>
        /// <param name="route">The OData route.</param>
        /// <param name="relativePathTemplate">The relative path template.</param>
        /// <param name="operation">Additional metadata based about the action.</param>
        public ODataActionDescriptor(HttpActionDescriptor actionDescriptor, ODataRoute route, string relativePathTemplate, Operation operation = null)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(route != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(relativePathTemplate));

            ActionDescriptor = actionDescriptor;
            Route = route;
            RelativePathTemplate = relativePathTemplate;
            Operation = operation;
        }

        public HttpActionDescriptor ActionDescriptor { get; }

        public ODataRoute Route { get; }

        public string RelativePathTemplate { get; }

        public Operation Operation { get; }
    }
}