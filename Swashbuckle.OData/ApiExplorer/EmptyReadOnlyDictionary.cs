// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Swashbuckle.OData.ApiExplorer
{
    internal class EmptyReadOnlyDictionary<TKey, TValue>
    {
        private static readonly ReadOnlyDictionary<TKey, TValue> _value = new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());

        public static IDictionary<TKey, TValue> Value
        {
            get { return _value; }
        }
    }
}