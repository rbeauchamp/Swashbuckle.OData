using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal static class SchemaRegistryExtensions
    {
        public static Schema GetOrRegisterODataType(this SchemaRegistry registry, Type type)
        {
            Contract.Requires(registry != null);
            Contract.Requires(type != null);

            if (IsAGenericODataTypeThatShouldBeUnwrapped(type))
            {
                var genericArguments = type.GetGenericArguments();
                Contract.Assume(genericArguments != null);
                return registry.GetOrRegister(genericArguments[0]);
            }
            Type elementType;
            if (type.IsCollection(out elementType) && !elementType.IsPrimitive)
            {
                var openOdataType = typeof (ODataResponse<>);
                var odataType = openOdataType.MakeGenericType(elementType);
                return registry.GetOrRegister(odataType);
            }
            return registry.GetOrRegister(type);
        }

        private static bool IsAGenericODataTypeThatShouldBeUnwrapped(Type type)
        {
            Contract.Requires(type != null);

            var isDelta = type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Delta<>);
            var isSingleResult = type.IsGenericType && type.GetGenericTypeDefinition() == typeof (SingleResult<>);

            return isDelta || isSingleResult;
        }
    }
}