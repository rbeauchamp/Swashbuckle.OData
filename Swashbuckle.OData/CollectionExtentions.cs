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
        public static void AddIfNotNull<T>(this List<T> source, T item)
        {
            Contract.Requires(source != null);

            if (item != null)
            {
                source.Add(item);
            }
        }

        /// <summary>
        /// Creates a <see cref="Collection{T}" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Collections.Generic.List`1" /> from.</param>
        /// <returns>
        /// A <see cref="Collection{T}" /> that contains elements from the input sequence.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is null.</exception>
        public static Collection<T> ToCollection<T>(this IEnumerable<T> source)
        {
            Contract.Requires(source != null);

            return new Collection<T>(source.ToList());
        }

        public static IEnumerable<T> UnionEvenIfNull<T>(this IEnumerable<T> source, IEnumerable<T> other, IEqualityComparer<T> comparer = null)
        {
            var nonNullSource = source ?? new List<T>();
            var nonNullOther = other ?? new List<T>();

            return nonNullSource.Union(nonNullOther, comparer ?? System.Collections.Generic.EqualityComparer<T>.Default);
        }
    }
}