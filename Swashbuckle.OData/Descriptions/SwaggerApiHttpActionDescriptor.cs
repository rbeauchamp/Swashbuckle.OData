using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Swashbuckle.OData
{
    public class SwaggerApiHttpActionDescriptor : HttpActionDescriptor
    {
        public SwaggerApiHttpActionDescriptor(string actionName, Type returnType, Collection<HttpMethod> supportedHttpMethods, string entitySetName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(entitySetName));

            SupportedHttpMethods = supportedHttpMethods;
            EntitySetName = entitySetName;
            ActionName = actionName;
            ReturnType = returnType;
        }

        public override string ActionName { get; }

        public override Type ReturnType { get; }

        public override Collection<HttpMethod> SupportedHttpMethods { get; }

        public string EntitySetName { get; }

        public override Collection<HttpParameterDescriptor> GetParameters()
        {
            return new Collection<HttpParameterDescriptor>();
        }

        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> dictionary, CancellationToken cancellationToken)
        {
            return Task.FromResult(new object());
        }
    }
}