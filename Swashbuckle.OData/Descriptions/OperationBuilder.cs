using System.Diagnostics.Contracts;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    public class OperationBuilder
    {
        private readonly Operation _operation;
        private readonly SwaggerRouteBuilder _swaggerRouteBuilder;

        internal OperationBuilder(Operation operation, SwaggerRouteBuilder swaggerRouteBuilder)
        {
            _operation = operation;
            _swaggerRouteBuilder = swaggerRouteBuilder;
        }


        /// <summary>
        ///     Define a path parameter
        /// </summary>
        /// <typeparam name="T">The type of the parameter</typeparam>
        /// <param name="parameterName">The name of the parameter as it appears in the path</param>
        public OperationBuilder PathParameter<T>(string parameterName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(parameterName));
            Contract.Ensures(Contract.Result<OperationBuilder>() != null);

            var edmType = GetEdmModel().GetEdmType(typeof (T));
            Contract.Assume(edmType != null);
            _operation.Parameters().Parameter(parameterName, ParameterSource.Path.ToString().ToLower(), null, edmType, true);

            return this;
        }

        /// <summary>
        ///     Define a body parameter
        /// </summary>
        /// <typeparam name="T">The type of the parameter</typeparam>
        /// <param name="parameterName">The name of the parameter as it appears in the method signature</param>
        public OperationBuilder BodyParameter<T>(string parameterName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(parameterName));
            Contract.Ensures(Contract.Result<OperationBuilder>() != null);

            var edmType = GetEdmModel().GetEdmType(typeof(T));
            Contract.Assume(edmType != null);
            _operation.Parameters().Parameter(parameterName, ParameterSource.Body.ToString().ToLower(), null, edmType, true);

            return this;
        }

        private IEdmModel GetEdmModel()
        {
            return _swaggerRouteBuilder.SwaggerRoute.ODataRoute.GetEdmModel();
        }
    }
}