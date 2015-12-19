using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;

namespace Swashbuckle.OData.Descriptions
{
    [ContractClass(typeof(ODataRouteGeneratorContract))]
    public interface IODataRouteGenerator
    {
        /// <summary>
        /// Generate a set of potential routes that will be verified against an OData API and used to generate ApiDescriptions.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="model">The entity data model.</param>
        List<SwaggerRoute> Generate(string routePrefix, IEdmModel model);
    }

    [ContractClassFor(typeof(IODataRouteGenerator))]
    public abstract class ODataRouteGeneratorContract : IODataRouteGenerator
    {
        public List<SwaggerRoute> Generate(string routePrefix, IEdmModel model)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(routePrefix));
            Contract.Requires(model != null);
            Contract.Ensures(Contract.Result<List<SwaggerRoute>>() != null);

            throw new System.NotImplementedException();
        }
    }
}