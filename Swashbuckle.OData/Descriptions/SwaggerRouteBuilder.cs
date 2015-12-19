using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.OData.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public class SwaggerRouteBuilder
    {
        private readonly SwaggerRoute _swaggerRoute;

        public SwaggerRouteBuilder(SwaggerRoute swaggerRoute, ODataRoute oDataRoute)
        {
            _swaggerRoute = swaggerRoute;
            ODataRoute = oDataRoute;
        }

        internal ODataRoute ODataRoute { get; }

        public OperationBuilder Operation(HttpMethod httpMethod)
        {
            Contract.Requires(GetOperation(httpMethod) == null);

            var operation = new Operation();

            switch (httpMethod.Method.ToUpper())
            {
                case "GET":
                    _swaggerRoute.PathItem.get = operation;
                    break;
                case "PUT":
                    _swaggerRoute.PathItem.put = operation;
                    break;
                case "POST":
                    _swaggerRoute.PathItem.post = operation;
                    break;
                case "DELETE":
                    _swaggerRoute.PathItem.delete = operation;
                    break;
                case "PATCH":
                    _swaggerRoute.PathItem.patch = operation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpMethod));
            }

            return new OperationBuilder(operation, this);
        }

        public Operation GetOperation(HttpMethod httpMethod)
        {
            switch (httpMethod.Method.ToUpper())
            {
                case "GET":
                    return _swaggerRoute.PathItem.get;
                case "PUT":
                    return _swaggerRoute.PathItem.put;
                case "POST":
                    return _swaggerRoute.PathItem.post;
                case "DELETE":
                    return _swaggerRoute.PathItem.delete;
                case "PATCH":
                    return _swaggerRoute.PathItem.patch;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpMethod));
            }
        }
    }
}