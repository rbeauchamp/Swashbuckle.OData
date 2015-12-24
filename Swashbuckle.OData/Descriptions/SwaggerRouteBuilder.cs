using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public class SwaggerRouteBuilder
    {
        public SwaggerRouteBuilder(SwaggerRoute swaggerRoute)
        {
            Contract.Requires(swaggerRoute != null);

            SwaggerRoute = swaggerRoute;
        }

        public SwaggerRoute SwaggerRoute { get; }

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
                    return SwaggerRoute.PathItem.patch;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpMethod));
            }
        }
    }
}