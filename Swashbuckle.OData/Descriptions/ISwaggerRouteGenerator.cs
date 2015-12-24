using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;

namespace Swashbuckle.OData.Descriptions
{
    [ContractClass(typeof(SwaggerRouteGeneratorContract))]
    internal interface ISwaggerRouteGenerator
    {
        /// <summary>
        /// Generate a set of potential routes that will be verified against an OData API and used to generate ApiDescriptions.
        /// </summary>
        /// <param name="httpConfig">The HTTP configuration.</param>
        IEnumerable<SwaggerRoute> Generate(HttpConfiguration httpConfig);
    }

    [ContractClassFor(typeof(ISwaggerRouteGenerator))]
    internal abstract class SwaggerRouteGeneratorContract : ISwaggerRouteGenerator
    {
        public IEnumerable<SwaggerRoute> Generate(HttpConfiguration httpConfig)
        {
            Contract.Requires(httpConfig != null);
            Contract.Ensures(Contract.Result<IEnumerable<SwaggerRoute>>() != null);

            throw new System.NotImplementedException();
        }
    }
}