using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Routing;
using Flurl;
using Microsoft.OData.Edm;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public class ODataSwaggerProvider : ISwaggerProvider
    {
        private readonly IEdmModel _edmModel;
        private readonly string _routePrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerProvider"/> class.
        /// Sets the OData route prefix to "odata".
        /// </summary>
        /// <param name="edmModel">The edm model.</param>
        public ODataSwaggerProvider(IEdmModel edmModel) : this(edmModel, "odata")
        {
            Contract.Requires(edmModel != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        /// </summary>
        /// <param name="edmModel">The edm model.</param>
        /// <param name="routePrefix">The OData route prefix.</param>
        public ODataSwaggerProvider(IEdmModel edmModel, string routePrefix)
        {
            Contract.Requires(edmModel != null);
            Contract.Requires(routePrefix != null);

            _edmModel = edmModel;
            _routePrefix = routePrefix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        /// This constructor requires that an IEdmModel has been set on the
        /// current HttpConfiguration, for example
        /// <code>
        /// public static void Register(HttpConfiguration config)
        /// {
        ///     var builder = new ODataConventionModelBuilder();
        ///     var edmModel = builder.GetEdmModel();
        ///     config.MapODataServiceRoute("odata", "odata", edmModel);
        /// }
        /// </code>
        /// </summary>
        /// <exception cref="Exception">This constructor requires that an IEdmModel has been set on the current HttpConfiguration, typically via a call to config.MapODataServiceRoute(string, string, IEdmModel)</exception>
        public ODataSwaggerProvider() : this(() => GlobalConfiguration.Configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerProvider" /> class.
        /// This constructor requires that an IEdmModel has been set on the
        /// current HttpConfiguration, for example
        /// <code>
        /// public static void Register(HttpConfiguration config)
        /// {
        ///     var builder = new ODataConventionModelBuilder();
        ///     var edmModel = builder.GetEdmModel();
        ///     config.MapODataServiceRoute("odata", "odata", edmModel);
        /// }
        /// </code>
        /// </summary>
        /// <param name="httpConfigurationProvider">A function that will return the HttpConfiguration that contains the OData Edm Model.</param>
        /// <exception cref="Exception">This constructor requires that an IEdmModel has been set on the current HttpConfiguration, typically via a call to config.MapODataServiceRoute(string, string, IEdmModel)</exception>
        public ODataSwaggerProvider(Func<HttpConfiguration> httpConfigurationProvider)
        {
            var oDataRoute = httpConfigurationProvider().Routes.SingleOrDefault(route => route is ODataRoute) as ODataRoute;

            if (oDataRoute == null)
            {
                throw new Exception("This constructor requires that an IEdmModel has been set on the current HttpConfiguration, typically via a call to config.MapODataServiceRoute(string, string, IEdmModel)");
            }

            var oDataPathRouteConstraint = oDataRoute.Constraints.Values.SingleOrDefault(value => value is ODataPathRouteConstraint) as ODataPathRouteConstraint;

            _edmModel = oDataPathRouteConstraint.EdmModel;
            _routePrefix = oDataRoute.RoutePrefix;
        }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            var oDataSwaggerConverter = new ODataSwaggerConverter(_edmModel);

            var rootUri = new Uri(rootUrl);

            var basePath = rootUri.AbsolutePath != "/" ? rootUri.AbsolutePath : "/" + _routePrefix;

            oDataSwaggerConverter.MetadataUri = new Uri(rootUrl.AppendPathSegments(basePath, "$metadata"));

            var port = !rootUri.IsDefaultPort ? ":" + rootUri.Port : string.Empty;

            var edmSwaggerDocument = oDataSwaggerConverter.ConvertToSwaggerModel();
            edmSwaggerDocument.host = rootUri.Host + port;
            edmSwaggerDocument.basePath = basePath;
            edmSwaggerDocument.schemes = new[] { rootUri.Scheme }.ToList();

            return edmSwaggerDocument;
        }
    }
}