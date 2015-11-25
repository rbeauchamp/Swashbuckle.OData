using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Routing;
using Flurl;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public class ODataSwaggerProvider : ISwaggerProvider
    {
        private readonly ISwaggerProvider _defaultProvider;
        // Here for future use against the OData API...
        private readonly SwaggerDocsConfig _swaggerDocsConfig;
        private readonly Func<HttpConfiguration> _httpConfigurationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        /// </summary>
        /// <param name="defaultProvider">The default provider.</param>
        /// <param name="swaggerDocsConfig">The swagger docs configuration.</param>
        public ODataSwaggerProvider(ISwaggerProvider defaultProvider, SwaggerDocsConfig swaggerDocsConfig) 
            : this(defaultProvider, swaggerDocsConfig, () => GlobalConfiguration.Configuration)
        {
            Contract.Requires(defaultProvider != null);
            Contract.Requires(swaggerDocsConfig != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        /// Use this constructor for self-hosted scenarios.
        /// </summary>
        /// <param name="defaultProvider">The default provider.</param>
        /// <param name="swaggerDocsConfig">The swagger docs configuration.</param>
        /// <param name="httpConfigurationProvider">A function that will return the HttpConfiguration that contains the OData Edm Model.</param>
        public ODataSwaggerProvider(ISwaggerProvider defaultProvider, SwaggerDocsConfig swaggerDocsConfig, Func<HttpConfiguration> httpConfigurationProvider)
        {
            Contract.Requires(defaultProvider != null);
            Contract.Requires(swaggerDocsConfig != null);
            Contract.Requires(httpConfigurationProvider != null);

            _defaultProvider = defaultProvider;
            _swaggerDocsConfig = swaggerDocsConfig;
            _httpConfigurationProvider = httpConfigurationProvider;
        }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            var oDataRoute = _httpConfigurationProvider().Routes.SingleOrDefault(route => route is ODataRoute) as ODataRoute;

            if (oDataRoute != null)
            {
                var oDataPathRouteConstraint = oDataRoute.Constraints.Values.SingleOrDefault(value => value is ODataPathRouteConstraint) as ODataPathRouteConstraint;

                var edmModel = oDataPathRouteConstraint.EdmModel;
                var routePrefix = oDataRoute.RoutePrefix;

                var oDataSwaggerConverter = new ODataSwaggerConverter(edmModel);

                var rootUri = new Uri(rootUrl);

                var basePath = rootUri.AbsolutePath != "/" ? rootUri.AbsolutePath : "/" + routePrefix;

                oDataSwaggerConverter.MetadataUri = new Uri(rootUrl.AppendPathSegments(basePath, "$metadata"));

                var port = !rootUri.IsDefaultPort ? ":" + rootUri.Port : string.Empty;

                var edmSwaggerDocument = oDataSwaggerConverter.ConvertToSwaggerModel();
                edmSwaggerDocument.host = rootUri.Host + port;
                edmSwaggerDocument.basePath = basePath;
                edmSwaggerDocument.schemes = new[] { rootUri.Scheme }.ToList();

                return edmSwaggerDocument;
            }

            return _defaultProvider.GetSwagger(rootUrl, apiVersion);
        }
    }
}