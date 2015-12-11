using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Swashbuckle.OData
{
    internal static class CollectionExtentions
    {
        /// <summary>
        ///     Adds the elements of the specified collection to the end of the <see cref="Collection{T}" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="collection">
        ///     The collection whose elements should be added to the end of the <see cref="Collection{T}" />.
        ///     The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.
        /// </param>
        public static void AddRange<T>(this Collection<T> source, IEnumerable<T> collection)
        {
            Contract.Requires(source != null);
            Contract.Requires(collection != null);

            foreach (var item in collection)
            {
                source.Add(item);
            }
        }

        /// <summary>
        ///     Adds the given item to the collection if the item is not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="item">The item.</param>
        public static void AddIfNotNull<T>(this Collection<T> source, T item)
        {
            Contract.Requires(source != null);

            if (item != null)
            {
                source.Add(item);
            }
        }

        public static IEnumerable<T> UnionEvenIfNull<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            var nonNullSource = source ?? new List<T>();
            var nonNullOther = other ?? new List<T>();

            return nonNullSource.Union(nonNullOther);
        }
    }
}