// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            Contract.Requires(type != null);

            if (type.IsValueType)
            {
                // value types are only nullable if they are Nullable<T>
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            // reference types are always nullable
            return true;
        }
    }
}
