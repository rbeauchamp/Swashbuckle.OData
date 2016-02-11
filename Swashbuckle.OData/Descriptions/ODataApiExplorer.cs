using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataApiExplorer : IApiExplorer
    {
        private readonly Lazy<Collection<ApiDescription>> _apiDescriptions;
        private readonly HttpConfiguration _httpConfig;
        private readonly IEnumerable<IODataActionDescriptorExplorer> _actionDescriptorExplorers;
        private readonly IEnumerable<IODataActionDescriptorMapper> _actionDescriptorMappers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorer" /> class.
        /// </summary>
        /// <param name="httpConfig">The HTTP configuration.</param>
        /// <param name="actionDescriptorExplorers">A set of strategies to gather <see cref="ODataActionDescriptor"/>s from the given <paramref name="httpConfig"/>.</param>
        /// <param name="actionDescriptorMappers">A set of strategires to map a <see cref="ODataActionDescriptor"/> to an <see cref="ApiDescription"/></param>
        public ODataApiExplorer(HttpConfiguration httpConfig, IEnumerable<IODataActionDescriptorExplorer> actionDescriptorExplorers, IEnumerable<IODataActionDescriptorMapper> actionDescriptorMappers)
        {
            Contract.Requires(httpConfig != null);
            Contract.Requires(actionDescriptorExplorers != null);
            Contract.Requires(actionDescriptorMappers != null);

            _httpConfig = httpConfig;
            _actionDescriptorExplorers = actionDescriptorExplorers;
            _actionDescriptorMappers = actionDescriptorMappers;
            _apiDescriptions = new Lazy<Collection<ApiDescription>>(GetApiDescriptions);
        }

        /// <summary>
        /// Gets the API descriptions. The descriptions are initialized on the first access.
        /// </summary>
        public Collection<ApiDescription> ApiDescriptions => _apiDescriptions.Value;

        private Collection<ApiDescription> GetApiDescriptions()
        {
            return _actionDescriptorExplorers
                // Gather ODataActionDescriptors from the API
                .SelectMany(explorer => explorer.Generate(_httpConfig))
                // Remove Duplicates
                .Distinct(new ODataActionDescriptorEqualityComparer())
                // Map them to ApiDescriptors
                .SelectMany(oDataActionDescriptor => _actionDescriptorMappers.Select(mapper => mapper.Map(oDataActionDescriptor))
                                                                             .FirstOrDefault(apiDescriptions => apiDescriptions.Any()) ?? new List<ApiDescription>())
                .ToCollection();
        }
    }
}