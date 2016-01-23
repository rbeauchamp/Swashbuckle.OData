using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.ServiceModel.Description;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData;
using Swashbuckle.OData.Descriptions;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal static class SchemaRegistryExtensions
    {
        public static Schema GetOrRegisterParameterType(this SchemaRegistry registry, HttpParameterDescriptor parameterDescriptor)
        {
            if (IsODataActionParameter(parameterDescriptor))
            {
                return ((ODataActionParameterDescriptor) parameterDescriptor).Schema;
            }
            if (IsAGenericODataTypeThatShouldBeUnwrapped(parameterDescriptor.ParameterType, MessageDirection.Input))
            {
                var genericArguments = parameterDescriptor.ParameterType.GetGenericArguments();
                Contract.Assume(genericArguments != null);
                return registry.GetOrRegister(genericArguments[0]);
            }
            return registry.GetOrRegister(parameterDescriptor.ParameterType);
        }

        private static bool IsODataActionParameter(HttpParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor is ODataActionParameterDescriptor;
        }

        public static Schema GetOrRegisterResponseType(this SchemaRegistry registry, Type type)
        {
            Contract.Requires(registry != null);
            Contract.Requires(type != null);

            if (IsAGenericODataTypeThatShouldBeUnwrapped(type, MessageDirection.Output))
            {
                var genericArguments = type.GetGenericArguments();
                Contract.Assume(genericArguments != null);
                return registry.GetOrRegister(genericArguments[0]);
            }
            Type elementType;
            if (IsResponseCollection(type, MessageDirection.Output, out elementType))
            {
                var openListType = typeof (List<>);
                var listType = openListType.MakeGenericType(elementType);
                var openOdataType = typeof (ODataResponse<>);
                var odataType = openOdataType.MakeGenericType(listType);
                return registry.GetOrRegister(odataType);
            }
            if (IsResponseWithPrimiveTypeNotSupportedByJson(type, MessageDirection.Output))
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