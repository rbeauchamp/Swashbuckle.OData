using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public class SwaggerRouteBuilder
    {
        private readonly SwaggerRoute _swaggerRoute;

        internal SwaggerRouteBuilder(SwaggerRoute swaggerRoute)
        {
            Contract.Requires(swaggerRoute != null);

            _swaggerRoute = swaggerRoute;
        }

        public SwaggerRoute SwaggerRoute
        {
            get
            {
                Contract.Ensures(Contract.Result<SwaggerRoute>() != null);
                return _swaggerRoute;
            }
        }

        public OperationBuilder Operation(HttpMethod httpMethod)
        {
            Contract.Requires(GetOperation(httpMethod) == null);
            Contract.Ensures(Contract.Result<OperationBuilder>() != null);

            var operation = new Operation();

            switch (httpMethod.Method.ToUpper())
            {
                case "GET":
                    SwaggerRoute.PathItem.get = operation;
                    break;
                case "PUT":
                    SwaggerRoute.PathItem.put = operation;
                    break;
                case "POST":
                    SwaggerRoute.PathItem.post = operation;
                    break;
                case "DELETE":
                    SwaggerRoute.PathItem.delete = operation;
                    break;
                case "PATCH":
                case "MERGE":
                    SwaggerRoute.PathItem.patch = operation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpMethod));
            }

            return new OperationBuilder(operation, this);
        }

        [Pure]
        public Operation GetOperation(HttpMethod httpMethod)
        {
            Contract.Requires(httpMethod != null);
            Contract.Requires(httpMethod.Method != null);
            Contract.Requires(httpMethod.Method.ToUpper() == @"GET" || httpMethod.Method.ToUpper() == @"PUT" || httpMethod.Method.ToUpper() == @"POST" || httpMethod.Method.ToUpper() != @"DELETE" || httpMethod.Method.ToUpper() != @"PATCH" || httpMethod.Method.ToUpper() != @"MERGE");

            switch (httpMethod.Method.ToUpper())
            {
                case "GET":
                    return SwaggerRoute.PathItem.get;
                case "PUT":
                    return SwaggerRoute.PathItem.put;
                case "POST":
                    return SwaggerRoute.PathItem.post;
                case "DELETE":
                    return SwaggerRoute.PathItem.delete;
                case "PATCH":
                case "MERGE":
                    return SwaggerRoute.PathItem.patch;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpMethod));
            }
        }
    }
}