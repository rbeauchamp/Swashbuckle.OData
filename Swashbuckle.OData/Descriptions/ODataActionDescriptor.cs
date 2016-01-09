using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataActionDescriptor
    {
        private readonly string _relativePathTemplate;
        private readonly HttpRequestMessage _request;
        private readonly ODataRoute _route;
        private readonly HttpActionDescriptor _actionDescriptor;
        private readonly Operation _operation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionDescriptor" /> class.
        /// </summary>
        /// <param name="actionDescriptor">The HTTP action descriptor.</param>
        /// <param name="route">The OData route.</param>
        /// <param name="relativePathTemplate">The relative path template.</param>
        /// <param name="request">The request.</param>
        /// <param name="operation">Additional metadata based about the action.</param>
        public ODataActionDescriptor(HttpActionDescriptor actionDescriptor, ODataRoute route, string relativePathTemplate, HttpRequestMessage request, Operation operation = null)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(route != null);
            Contract.Requires(relativePathTemplate != null);
            Contract.Requires(request != null);

            _actionDescriptor = actionDescriptor;
            _route = route;
            _relativePathTemplate = relativePathTemplate;
            _request = request;
            _operation = operation;
        }

        public HttpActionDescriptor ActionDescriptor
        {
            get { return _actionDescriptor; }
        }

        public ODataRoute Route
        {
            get { return _route; }
        }

        public string RelativePathTemplate
        {
            get { return _relativePathTemplate; }
        }

        public Operation Operation
        {
            get { return _operation; }
        }

        public HttpRequestMessage Request
        {
            get { return _request; }
        }

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(ActionDescriptor != null);
            Contract.Invariant(Route != null);
            Contract.Invariant(RelativePathTemplate != null);
            Contract.Invariant(Request != null);
        }
    }
}