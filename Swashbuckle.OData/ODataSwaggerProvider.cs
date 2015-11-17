using System;
using System.Linq;
using Microsoft.OData.Edm;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public class ODataSwaggerProvider : ISwaggerProvider
    {
        private readonly IEdmModel _edmModel;

        public ODataSwaggerProvider(IEdmModel edmModel)
        {
            _edmModel = edmModel;
        }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            var edmSwaggerDocument = new ODataSwaggerConverter(_edmModel).ConvertToSwaggerModel();

            var rootUri = new Uri(rootUrl);
            var port = !rootUri.IsDefaultPort ? ":" + rootUri.Port : string.Empty;
            edmSwaggerDocument.host = rootUri.Host + port;
            edmSwaggerDocument.basePath = rootUri.AbsolutePath != "/" ? rootUri.AbsolutePath : "/odata";
            edmSwaggerDocument.schemes = new[] { rootUri.Scheme }.ToList();

            return edmSwaggerDocument;
        }
    }
}