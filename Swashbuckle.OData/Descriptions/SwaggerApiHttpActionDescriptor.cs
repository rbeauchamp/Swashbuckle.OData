using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Swashbuckle.OData
{
    public class SwaggerApiHttpActionDescriptor : HttpActionDescriptor
    {
        public SwaggerApiHttpActionDescriptor(string actionName, Type returnType, Collection<HttpMethod> supportedHttpMethods)
        {
            SupportedHttpMethods = supportedHttpMethods;
            ActionName = actionName;
            ReturnType = returnType;
        }

        public override string ActionName { get; }

        public override Type ReturnType { get; }

        public override Collection<HttpMethod> SupportedHttpMethods { get; }

        public override Collection<HttpParameterDescriptor> GetParameters()
        {
            throw new NotImplementedException();
        }

        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> dictionary, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}