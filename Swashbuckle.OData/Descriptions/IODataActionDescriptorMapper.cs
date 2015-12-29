using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.Description;

namespace Swashbuckle.OData.Descriptions
{
    [ContractClass(typeof(ODataActionDescriptorMapperContract))]
    internal interface IODataActionDescriptorMapper
    {
        /// <summary>
        /// Map the ODataActionDescriptor to an ApiDescription.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor to map.</param>
        IEnumerable<ApiDescription> Map(ODataActionDescriptor actionDescriptor);
    }

    [ContractClassFor(typeof(IODataActionDescriptorMapper))]
    internal abstract class ODataActionDescriptorMapperContract : IODataActionDescriptorMapper
    {
        public IEnumerable<ApiDescription> Map(ODataActionDescriptor actionDescriptor)
        {
            Contract.Requires(actionDescriptor != null);
            Contract.Ensures(Contract.Result<IEnumerable<ApiDescription>>() != null);

            throw new NotImplementedException();
        }
    }
}