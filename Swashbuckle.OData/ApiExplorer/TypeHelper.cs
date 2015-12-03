// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Web.Http;

namespace Swashbuckle.OData.ApiExplorer
{
    /// <summary>
    ///     A static class that provides various <see cref="Type" /> related helpers.
    /// </summary>
    internal static class TypeHelper
    {
        private static readonly Type TaskGenericType = typeof (Task<>);

        internal static readonly Type ApiControllerType = typeof (ApiController);

        internal static Type GetTaskInnerTypeOrNull(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if (TaskGenericType == genericTypeDefinition)
                {
                    return type.GetGenericArguments()[0];
                }
            }

            return null;
        }

        internal static Type[] GetTypeArgumentsIfMatch(Type closedType, Type matchingOpenType)
        {
            if (!closedType.IsGenericType)
            {
                return null;
            }

            var openType = closedType.GetGenericTypeDefinition();
            return matchingOpenType == openType ? closedType.GetGenericArguments() : null;
        }

        internal static bool IsCompatibleObject(Type type, object value)
        {
            return (value == null && TypeAllowsNullValue(type)) || type.IsInstanceOfType(value);
        }

        internal static bool IsNullableValueType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        internal static bool TypeAllowsNullValue(Type type)
        {
            return !type.IsValueType || IsNullableValueType(type);
        }

        internal static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.Equals(typeof (string)) || type.Equals(typeof (DateTime)) || type.Equals(typeof (decimal)) || type.Equals(typeof (Guid)) || type.Equals(typeof (DateTimeOffset)) || type.Equals(typeof (TimeSpan));
        }

        internal static bool IsSimpleUnderlyingType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            return IsSimpleType(type);
        }

        internal static bool CanConvertFromString(Type type)
        {
            return IsSimpleUnderlyingType(type) || HasStringConverter(type);
        }

        internal static bool HasStringConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof (string));
        }

        /// <summary>
        ///     Fast implementation to get the subset of a given type.
        /// </summary>
        /// <typeparam name="T">type to search for</typeparam>
        /// <returns>subset of objects that can be assigned to T</returns>
        internal static ReadOnlyCollection<T> OfType<T>(object[] objects) where T : class
        {
            var max = objects.Length;
            var list = new List<T>(max);
            var idx = 0;
            for (var i = 0; i < max; i++)
            {
                var attr = objects[i] as T;
                if (attr != null)
                {
                    list.Add(attr);
                    idx++;
                }
            }
            list.Capacity = idx;

            return new ReadOnlyCollection<T>(list);
        }
    }
}