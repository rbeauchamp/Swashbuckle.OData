using System;
using System.Collections.Generic;

namespace Swashbuckle.OData
{
    /// <summary>
    ///     Non-generic class to produce instances of the generic class,
    ///     optionally using type inference.
    /// </summary>
    public static class EqualityComparer
    {
        /// <summary>
        ///     Creates an instance of ProjectionEqualityComparer using the specified projection.
        /// </summary>
        /// <typeparam name="TSource">Type parameter for the elements to be compared</typeparam>
        /// <typeparam name="TKey">
        ///     Type parameter for the keys to be compared,
        ///     after being projected from the elements
        /// </typeparam>
        /// <param name="projection">Projection to use when determining the key of an element</param>
        /// <returns>
        ///     A comparer which will compare elements by projecting
        ///     each element to its key, and comparing keys
        /// </returns>
        public static EqualityComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> projection)
        {
            return new EqualityComparer<TSource, TKey>(projection);
        }

        /// <summary>
        ///     Creates an instance of ProjectionEqualityComparer using the specified projection.
        ///     The ignored parameter is solely present to aid type inference.
        /// </summary>
        /// <typeparam name="TSource">Type parameter for the elements to be compared</typeparam>
        /// <typeparam name="TKey">
        ///     Type parameter for the keys to be compared,
        ///     after being projected from the elements
        /// </typeparam>
        /// <param name="ignored">Value is ignored - type may be used by type inference</param>
        /// <param name="projection">Projection to use when determining the key of an element</param>
        /// <returns>
        ///     A comparer which will compare elements by projecting
        ///     each element to its key, and comparing keys
        /// </returns>
        public static EqualityComparer<TSource, TKey> Create<TSource, TKey>(TSource ignored, Func<TSource, TKey> projection)
        {
            return new EqualityComparer<TSource, TKey>(projection);
        }
    }

    /// <summary>
    ///     Class generic in the source only to produce instances of the
    ///     doubly generic class, optionally using type inference.
    /// </summary>
    public static class EqualityComparer<TSource>
    {
        /// <summary>
        ///     Creates an instance of ProjectionEqualityComparer using the specified projection.
        /// </summary>
        /// <typeparam name="TKey">
        ///     Type parameter for the keys to be compared,
        ///     after being projected from the elements
        /// </typeparam>
        /// <param name="projection">Projection to use when determining the key of an element</param>
        /// <returns>
        ///     A comparer which will compare elements by projecting each element to its key,
        ///     and comparing keys
        /// </returns>
        public static EqualityComparer<TSource, TKey> Create<TKey>(Func<TSource, TKey> projection)
        {
            return new EqualityComparer<TSource, TKey>(projection);
        }
    }

    /// <summary>
    ///     Comparer which projects each element of the comparison to a key, and then compares
    ///     those keys using the specified (or default) _comparer for the key type.
    /// </summary>
    /// <typeparam name="TSource">
    ///     Type of elements which this _comparer
    ///     will be asked to compare
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     Type of the key projected
    ///     from the element
    /// </typeparam>
    public class EqualityComparer<TSource, TKey> : IEqualityComparer<TSource>
    {
        private readonly IEqualityComparer<TKey> _comparer;
        private readonly Func<TSource, TKey> _projection;

        /// <summary>
        ///     Creates a new instance using the specified _projection, which must not be null.
        ///     The default _comparer for the projected type is used.
        /// </summary>
        /// <param name="projection">Projection to use during comparisons</param>
        public EqualityComparer(Func<TSource, TKey> projection) : this(projection, null)
        {
        }

        /// <summary>
        ///     Creates a new instance using the specified _projection, which must not be null.
        /// </summary>
        /// <param name="projection">Projection to use during comparisons</param>
        /// <param name="comparer">
        ///     The _comparer to use on the keys. May be null, in
        ///     which case the default _comparer will be used.
        /// </param>
        public EqualityComparer(Func<TSource, TKey> projection, IEqualityComparer<TKey> comparer)
        {
            if (projection == null)
            {
                throw new ArgumentNullException(nameof(projection));
            }
            _comparer = comparer ?? System.Collections.Generic.EqualityComparer<TKey>.Default;
            _projection = projection;
        }

        /// <summary>
        ///     Compares the two specified values for equality by applying the _projection
        ///     to each value and then using the equality _comparer on the resulting keys. Null
        ///     references are never passed to the _projection.
        /// </summary>
        public bool Equals(TSource x, TSource y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return _comparer.Equals(_projection(x), _projection(y));
        }

        /// <summary>
        ///     Produces a hash code for the given value by projecting it and
        ///     then asking the equality _comparer to find the hash code of
        ///     the resulting key.
        /// </summary>
        public int GetHashCode(TSource obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            return _comparer.GetHashCode(_projection(obj));
        }
    }
}