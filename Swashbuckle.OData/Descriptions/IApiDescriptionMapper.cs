using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.OData.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    [ContractClass(typeof(ApiDescriptionMapperContract))]
    public interface IApiDescriptionMapper
    {
        /// <summary>
        /// Map the HttpActionDescriptor to an ApiDescription.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <param name="route">The route.</param>
        /// <param name="relativePathTemplate">The relative path template.</param>
        /// <param name="operation">The operation.</param>
        IEnumerable<ApiDescription> Map(HttpActionDescriptor actionDescriptor, ODataRoute route, string relativePathTemplate, Operation operation = null);
    }

    [ContractClassFor(typeof(IApiDescriptionMapper))]
    public abstract class ApiDescriptionMapperContract : IApiDescriptionMapper
    {
        public IEnumerable<ApiDescription> Map(HttpActionDescriptor actionDescriptor, ODataRoute route, string relativePathTemplate, Operation operation)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Requires(route != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(relativePathTemplate));
            Contract.Ensures(Contract.Result<IEnumerable<ApiDescription>>() != null);

            throw new System.NotImplementedException();
        }
    }
}