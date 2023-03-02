// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Spatial;

namespace System.Web.OData.Formatter
{
    internal static class EdmLibHelpers
    {
        private static readonly EdmCoreModel CoreModel = EdmCoreModel.Instance;        
                
        private static readonly Dictionary<Type, IEdmPrimitiveType> BuiltInTypesMapping = new[]
        {
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (string), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (bool), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (bool?), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (byte), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (byte?), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (decimal), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (decimal?), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (double), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (double?), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (Guid), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (Guid?), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (short), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (short?), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (int), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (int?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (long), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (long?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (sbyte), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (sbyte?), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (float), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (float?), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (byte[]), GetPrimitiveType(EdmPrimitiveTypeKind.Binary)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (Stream), GetPrimitiveType(EdmPrimitiveTypeKind.Stream)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (Geography), GetPrimitiveType(EdmPrimitiveTypeKind.Geography)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeographyPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeographyLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyLineString)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeographyPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPolygon)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeographyCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeographyMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiLineString)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeographyMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPoint)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeographyMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPolygon)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (Geometry), GetPrimitiveType(EdmPrimitiveTypeKind.Geometry)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeometryPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeometryLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeometryPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeometryCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeometryMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeometryMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (GeometryMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (DateTimeOffset), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (DateTimeOffset?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (TimeSpan), GetPrimitiveType(EdmPrimitiveTypeKind.Duration)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (TimeSpan?), GetPrimitiveType(EdmPrimitiveTypeKind.Duration)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (Date), GetPrimitiveType(EdmPrimitiveTypeKind.Date)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (Date?), GetPrimitiveType(EdmPrimitiveTypeKind.Date)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (TimeOfDay), GetPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (TimeOfDay?), GetPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (ushort), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (ushort?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (uint), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (uint?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (ulong), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (ulong?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (char[]), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (char), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (char?), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (DateTime), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
            new KeyValuePair<Type, IEdmPrimitiveType>(typeof (DateTime?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset))
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public static IEnumerable<IEdmStructuralProperty> GetKey(this IEdmEntityType edmEntityType)
        {
            Contract.Requires(edmEntityType != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdmStructuralProperty>>() != null);

            var result = edmEntityType.Key();
            Contract.Assume(result != null);
            return result;
        }

        public static IEdmEntityType GetEntityType(this IEdmNavigationSource navigationSource)
        {
            Contract.Requires(navigationSource != null);
            Contract.Ensures(Contract.Result<IEdmEntityType>() != null);

            var result = navigationSource.EntityType();
            Contract.Assume(result != null);
            return result;
        }

        public static IEdmType GetDefinition(this IEdmTypeReference edmTypeReference)
        {
            Contract.Requires(edmTypeReference != null);
            Contract.Ensures(Contract.Result<IEdmType>() != null);

            var result = edmTypeReference.Definition;
            Contract.Assume(result != null);
            return result;
        }

        public static IEdmTypeReference GetPropertyType(this IEdmProperty edmProperty)
        {
            Contract.Requires(edmProperty != null);
            Contract.Ensures(Contract.Result<IEdmTypeReference>() != null);

            var result = edmProperty.Type;
            Contract.Assume(result != null);
            return result;
        }

        public static IEdmTypeReference GetOperationType(this IEdmOperationParameter edmOperationParameter)
        {
            Contract.Requires(edmOperationParameter != null);
            Contract.Ensures(Contract.Result<IEdmTypeReference>() != null);

            var result = edmOperationParameter.Type;
            Contract.Assume(result != null);
            return result;
        }

        public static IEdmType GetEdmType(this IEdmModel edmModel, Type clrType)
        {
            Contract.Requires(edmModel != null);
            Contract.Requires(clrType != null);

            return GetEdmType(edmModel, clrType, true);
        }

        private static IEdmType GetEdmType(IEdmModel edmModel, Type clrType, bool testCollections)
        {
            Contract.Requires(edmModel != null);
            Contract.Requires(clrType != null);

            var primitiveType = GetEdmPrimitiveTypeOrNull(clrType);
            if (primitiveType != null)
            {
                return primitiveType;
            }
            if (testCollections)
            {
                var enumerableOfT = ExtractGenericInterface(clrType, typeof (IEnumerable<>));
                if (enumerableOfT != null)
                {
                    var genericArguments = enumerableOfT.GetGenericArguments();
                    Contract.Assume(genericArguments != null);
                    var elementClrType = genericArguments[0];
                    Contract.Assume(elementClrType != null);
                    var elementType = GetEdmType(edmModel, elementClrType, false);
                    if (elementType != null)
                    {
                        return new EdmCollectionType(elementType.ToEdmTypeReference(IsNullable(elementClrType)));
                    }
                }
            }

            var underlyingType = TypeHelper.GetUnderlyingTypeOrSelf(clrType);

            if (underlyingType.IsEnum)
            {
                clrType = underlyingType;
            }

            // search for the ClrTypeAnnotation and return it if present
            var returnType = edmModel.SchemaElements.OfType<IEdmType>().Select(edmType => new
            {
                EdmType = edmType,
                Annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmType)
            }).Where(tuple => tuple.Annotation != null && tuple.Annotation.ClrType == clrType).Select(tuple => tuple.EdmType).SingleOrDefault();

            // default to the EdmType with the same name as the ClrType name 
            returnType = returnType ?? edmModel.FindType(clrType.EdmFullName());

            if (clrType.BaseType != null)
            {
                // go up the inheritance tree to see if we have a mapping defined for the base type.
                returnType = returnType ?? GetEdmType(edmModel, clrType.BaseType, testCollections);
            }
            return returnType;
        }

        public static IEdmTypeReference ToEdmTypeReference(this IEdmType edmType, bool isNullable)
        {
            Contract.Requires(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Collection:
                    return new EdmCollectionTypeReference(edmType as IEdmCollectionType);
                case EdmTypeKind.Complex:
                    return new EdmComplexTypeReference(edmType as IEdmComplexType, isNullable);
                case EdmTypeKind.Entity:
                    return new EdmEntityTypeReference(edmType as IEdmEntityType, isNullable);
                case EdmTypeKind.EntityReference:
                    return new EdmEntityReferenceTypeReference(edmType as IEdmEntityReferenceType, isNullable);
                case EdmTypeKind.Enum:
                    return new EdmEnumTypeReference(edmType as IEdmEnumType, isNullable);
                case EdmTypeKind.Primitive:
                    return CoreModel.GetPrimitive((edmType as IEdmPrimitiveType).PrimitiveKind, isNullable);
                default:
                    throw new ArgumentOutOfRangeException(nameof(edmType));
            }
        }

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel)
        {
            Contract.Requires(edmTypeReference != null);
            Contract.Requires(edmModel != null);

            var primitiveClrType = BuiltInTypesMapping.Where(kvp => edmTypeReference.GetDefinition().IsEquivalentTo(kvp.Value) && (!edmTypeReference.IsNullable || IsNullable(kvp.Key))).Select(kvp => kvp.Key).FirstOrDefault();

            if (primitiveClrType != null)
            {
                return primitiveClrType;
            }
            var edmType = edmTypeReference.GetDefinition();
            Contract.Assume(edmType is IEdmSchemaType);
            var clrType = GetClrType(edmType, edmModel);
            if (clrType != null && clrType.IsEnum && edmTypeReference.IsNullable)
            {
                return clrType.ToNullable();
            }

            return clrType;
        }

        public static Type GetClrType(IEdmType edmType, IEdmModel edmModel)
        {
            Contract.Requires(edmType is IEdmSchemaType);
            Contract.Requires(edmModel != null);

            var edmSchemaType = (IEdmSchemaType) edmType;

            var annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmSchemaType);
            if (annotation != null)
            {
                return annotation.ClrType;
            }

            var typeName = edmSchemaType.FullName();
            var matchingTypes = GetMatchingTypes(typeName);

            var matchingTypesList = matchingTypes as IList<Type> ?? matchingTypes.ToList();
            if (matchingTypesList.Count > 1)
            {
                throw new Exception("Multiple Matching ClrTypes For EdmType");
            }

            edmModel.SetAnnotationValue(edmSchemaType, new ClrTypeAnnotation(matchingTypesList.SingleOrDefault()));

            return matchingTypesList.SingleOrDefault();
        }

        public static IEdmPrimitiveType GetEdmPrimitiveTypeOrNull(Type clrType)
        {
            Contract.Requires(clrType != null);

            IEdmPrimitiveType primitiveType;
            return BuiltInTypesMapping.TryGetValue(clrType, out primitiveType) ? primitiveType : null;
        }

        public static IEdmPrimitiveTypeReference GetEdmPrimitiveTypeReferenceOrNull(Type clrType)
        {
            Contract.Requires(clrType != null);

            var primitiveType = GetEdmPrimitiveTypeOrNull(clrType);
            return primitiveType != null ? CoreModel.GetPrimitive(primitiveType.PrimitiveKind, IsNullable(clrType)) : null;
        }

        // Mangle the invalid EDM literal Type.FullName (System.Collections.Generic.IEnumerable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]) 
        // to a valid EDM literal (the C# type name IEnumerable<int>).
        public static string EdmName(this Type clrType)
        {
            Contract.Requires(clrType != null);

            // We cannot use just Type.Name here as it doesn't work for generic types.
            return MangleClrTypeName(clrType);
        }

        public static string EdmFullName(this Type clrType)
        {
            Contract.Requires(clrType != null);

            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", clrType.Namespace, clrType.EdmName());
        }

        private static IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind primitiveKind)
        {
            return CoreModel.GetPrimitiveType(primitiveKind);
        }

        public static bool IsNullable(Type type)
        {
            Contract.Requires(type != null);

            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        private static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            Contract.Requires(queryType != null);

            Func<Type, bool> matchesInterface = t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType;
            return matchesInterface(queryType) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }
        
        private static IEnumerable<Type> GetMatchingTypes(string edmFullName)
        {   
            return TypeHelper.GetLoadedTypes().Where(t => t.IsPublic && t.EdmFullName() == edmFullName);
        }

        // TODO (workitem 336): Support nested types and anonymous types.
        private static string MangleClrTypeName(Type type)
        {
            Contract.Requires(type != null);

            return !type.IsGenericType ? type.Name : string.Format(CultureInfo.InvariantCulture, "{0}Of{1}", type.Name.Replace('`', '_'), string.Join("_", type.GetGenericArguments().Select(MangleClrTypeName)));
        }
    }
}