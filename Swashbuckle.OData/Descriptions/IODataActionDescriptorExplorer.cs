using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;

namespace Swashbuckle.OData.Descriptions
{
    [ContractClass(typeof(ODataActionDescriptorExplorerContract))]
    internal interface IODataActionDescriptorExplorer
    {
        /// <summary>
        /// Retrieve a set of <see cref="ODataActionDescriptor"/>s from the API
        /// </summary>
        /// <param name="httpConfig">The HTTP configuration.</param>
        IEnumerable<ODataActionDescriptor> Generate(HttpConfiguration httpConfig);
    }

    [ContractClassFor(typeof(IODataActionDescriptorExplorer))]
    internal abstract class ODataActionDescriptorExplorerContract : IODataActionDescriptorExplorer
    {
        public IEnumerable<ODataActionDescriptor> Generate(HttpConfiguration httpConfig)
        {
            Contract.Requires(httpConfig != null);
            Contract.Ensures(Contract.Result<IEnumerable<ODataActionDescriptor>>() != null);

            throw new NotImplementedException();
        }
    }
}