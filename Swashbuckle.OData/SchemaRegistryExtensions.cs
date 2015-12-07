using System;
using System.Web.Http;
using System.Web.OData;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public static class SchemaRegistryExtensions
    {
        public static Schema GetOrRegisterODataType(this SchemaRegistry registry, Type type)
        {
            var isAGenericODataType = IsAGenericODataType(type);
            return isAGenericODataType
                ? registry.GetOrRegister(type.GetGenericArguments()[0]) 
                : registry.GetOrRegister(type);
        }

        private static bool IsAGenericODataType(Type type)
        {
            var isDelta = type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Delta<>);
            var isSingleResult = type.IsGenericType && type.GetGenericTypeDefinition() == typeof (SingleResult<>);

            return isDelta || isSingleResult;
        }
    }
}