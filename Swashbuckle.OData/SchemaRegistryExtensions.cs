using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.ServiceModel.Description;
using System.Web.Http;
using System.Web.OData;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal static class SchemaRegistryExtensions
    {
        public static Schema GetOrRegisterODataType(this SchemaRegistry registry, Type type, MessageDirection messageDirection)
        {
            Contract.Requires(registry != null);
            Contract.Requires(type != null);

            if (IsAGenericODataTypeThatShouldBeUnwrapped(type, messageDirection))
            {
                var genericArguments = type.GetGenericArguments();
                Contract.Assume(genericArguments != null);
                return registry.GetOrRegister(genericArguments[0]);
            }
            Type elementType;
            if (IsResponseCollection(type, messageDirection, out elementType))
            {
                var openListType = typeof (List<>);
                var listType = openListType.MakeGenericType(elementType);
                var openOdataType = typeof (ODataResponse<>);
                var odataType = openOdataType.MakeGenericType(listType);
                return registry.GetOrRegister(odataType);
            }
            if (IsResponseWithPrimiveTypeNotSupportedByJson(type, messageDirection))
            {
                var openOdataType = typeof(ODataResponse<>);
                var odataType = openOdataType.MakeGenericType(type);
                return registry.GetOrRegister(odataType);
            }
            return registry.GetOrRegister(type);
        }

        private static bool IsResponseWithPrimiveTypeNotSupportedByJson(Type type, MessageDirection messageDirection)
        {
            if (messageDirection == MessageDirection.Output)
            {
                if (type == typeof (long))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsResponseCollection(Type type, MessageDirection messageDirection, out Type elementType)
        {
            return type.IsCollection(out elementType) && messageDirection == MessageDirection.Output;
        }

        private static bool IsAGenericODataTypeThatShouldBeUnwrapped(Type type, MessageDirection messageDirection)
        {
            Contract.Requires(type != null);

            var isDelta = type.IsGenericType 
                && type.GetGenericTypeDefinition() == typeof (Delta<>) 
                && messageDirection == MessageDirection.Input;
            var isSingleResult = type.IsGenericType 
                && type.GetGenericTypeDefinition() == typeof (SingleResult<>) 
                && messageDirection == MessageDirection.Output;

            return isDelta || isSingleResult;
        }
    }


}