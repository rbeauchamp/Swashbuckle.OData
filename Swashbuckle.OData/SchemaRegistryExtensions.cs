using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;
using Swashbuckle.OData.Descriptions;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal static class SchemaRegistryExtensions
    {
        public static Schema GetOrRegisterParameterType(this SchemaRegistry registry, IEdmModel edmModel, HttpParameterDescriptor parameterDescriptor)
        {
            if (IsODataActionParameter(parameterDescriptor))
            {
                var schema = ((ODataActionParameterDescriptor) parameterDescriptor).Schema;
                RegisterReferencedTypes(registry, edmModel, schema);
                return schema;
            }
            if (IsAGenericODataTypeThatShouldBeUnwrapped(parameterDescriptor.ParameterType, MessageDirection.Input))
            {
                return HandleGenericODataTypeThatShouldBeUnwrapped(registry, edmModel, parameterDescriptor.ParameterType);
            }
            var schema1 = registry.GetOrRegister(parameterDescriptor.ParameterType);
            ApplyEdmModelPropertyNamesToSchema(registry, edmModel, parameterDescriptor.ParameterType);
            return schema1;
        }

        private static void RegisterReferencedTypes(SchemaRegistry registry, IEdmModel edmModel, Schema schema)
        {
            Contract.Requires(registry != null);
            Contract.Requires(schema != null);

            while (true)
            {
                Contract.Assume(schema != null);

                var referencedType = schema.GetReferencedType();

                if (referencedType != null)
                {
                    registry.GetOrRegister(referencedType);
                    FixSchemaReference(registry, schema, referencedType);
                    ApplyEdmModelPropertyNamesToSchema(registry, edmModel, referencedType);
                    return;
                }

                if (schema.properties != null && schema.properties.Any())
                {
                    foreach (var property in schema.properties)
                    {
                        RegisterReferencedTypes(registry, edmModel, property.Value);
                    }
                    return;
                }

                if (schema.items != null)
                {
                    schema = schema.items;
                    continue;
                }
                break;
            }
        }

        private static void FixSchemaReference(SchemaRegistry registry, Schema schema, Type referencedType)
        {
            Contract.Requires(schema.@ref != null);

            var schemaIdSelector = registry.GetInstanceField<Func<Type, string>>("_schemaIdSelector", true);

            schema.@ref = "#/definitions/" + schemaIdSelector(referencedType);
        }

        private static Schema HandleGenericODataTypeThatShouldBeUnwrapped(SchemaRegistry registry, IEdmModel edmModel, Type type)
        {
            var genericArguments = type.GetGenericArguments();
            Contract.Assume(genericArguments != null);
            var schema = registry.GetOrRegister(genericArguments[0]);
            ApplyEdmModelPropertyNamesToSchema(registry, edmModel, genericArguments[0]);
            return schema;
        }

        private static bool IsODataActionParameter(HttpParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor is ODataActionParameterDescriptor;
        }

        public static Schema GetOrRegisterResponseType(this SchemaRegistry registry, IEdmModel edmModel, Type type)
        {
            Contract.Requires(registry != null);
            Contract.Requires(type != null);

            if (IsAGenericODataTypeThatShouldBeUnwrapped(type, MessageDirection.Output))
            {
                return HandleGenericODataTypeThatShouldBeUnwrapped(registry, edmModel, type);
            }
            Type elementType;
            if (IsResponseCollection(type, MessageDirection.Output, out elementType))
            {
                var openListType = typeof (List<>);
                var listType = openListType.MakeGenericType(elementType);
                var openOdataType = typeof (ODataResponse<>);
                var odataType = openOdataType.MakeGenericType(listType);
                var schema = registry.GetOrRegister(odataType);
                ApplyEdmModelPropertyNamesToSchema(registry, edmModel, elementType);
                return schema;
            }
            if (IsResponseWithPrimiveTypeNotSupportedByJson(type, MessageDirection.Output))
            {
                var openOdataType = typeof(ODataResponse<>);
                var odataType = openOdataType.MakeGenericType(type);
                var schema = registry.GetOrRegister(odataType);
                return schema;
            }
            var schema1 = registry.GetOrRegister(type);
            ApplyEdmModelPropertyNamesToSchema(registry, edmModel, type);
            return schema1;
        }

        private static void ApplyEdmModelPropertyNamesToSchema(SchemaRegistry registry, IEdmModel edmModel, Type type)
        {
            var entityReference = registry.GetOrRegister(type);
            if (entityReference.@ref != null)
            {
                var definitionKey = entityReference.@ref.Replace("#/definitions/", string.Empty);
                var schemaDefinition = registry.Definitions[definitionKey];
                var edmType = edmModel.GetEdmType(type) as IEdmStructuredType;
                if (edmType != null)
                {
                    var edmProperties = new Dictionary<string, Schema>();
                    foreach (var property in schemaDefinition.properties)
                    {
                        var currentProperty = type.GetProperty(property.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        var edmPropertyName = GetEdmPropertyName(currentProperty, edmType);
                        if (edmPropertyName != null)
                        {
                            edmProperties.Add(edmPropertyName, property.Value);
                        }
                    }
                    schemaDefinition.properties = edmProperties;
                }
            }
        }

        private static string GetEdmPropertyName(MemberInfo currentProperty, IEdmStructuredType edmType)
        {
            Contract.Requires(currentProperty != null);
            Contract.Requires(edmType != null);

            var edmProperty = edmType.Properties().SingleOrDefault(property => property.Name.Equals(currentProperty.Name, StringComparison.CurrentCultureIgnoreCase));

            return edmProperty != null ? GetPropertyNameForEdmModel(currentProperty, edmProperty) : null;
        }

        private static string GetPropertyNameForEdmModel(MemberInfo currentProperty, IEdmNamedElement edmProperty)
        {
            Contract.Requires(currentProperty != null);
            Contract.Requires(edmProperty != null);

            var dataMemberAttribute = currentProperty.GetCustomAttributes<DataMemberAttribute>()?.SingleOrDefault();

            return !string.IsNullOrWhiteSpace(dataMemberAttribute?.Name) ? dataMemberAttribute.Name : edmProperty.Name;
        }

        private static bool IsResponseWithPrimiveTypeNotSupportedByJson(Type type, MessageDirection messageDirection)
        {
            if (messageDirection == MessageDirection.Output)
            {
                if (type == typeof (long))
                {
                    return true;
                }
                if (type == typeof(decimal))
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