using System.Diagnostics.Contracts;
using System.Reflection;

namespace Swashbuckle.OData
{
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        /// <param name="ensureNonNull">if set to <c>true</c> [ensure non null].</param>
        /// <returns>
        /// The field value from the object.
        /// </returns>
        internal static T GetInstanceField<T>(this object instance, string fieldName, bool ensureNonNull = false)
        {
            Contract.Requires(instance != null);
            Contract.Ensures(Contract.Result<T>() != null || !ensureNonNull);

            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var fieldInfo = instance.GetType().GetField(fieldName, bindFlags);
            Contract.Assume(fieldInfo != null);
            var value = fieldInfo.GetValue(instance);
            Contract.Assume(value != null || !ensureNonNull);
            return value != null ? (T)value : default (T);
        }

        /// <summary>
        /// Uses reflection to set the property value in an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance object.</param>
        /// <param name="propertyName">The name of the property to be set.</param>
        /// <param name="newValue">The new value.</param>
        internal static void SetInstanceProperty<T>(this object instance, string propertyName, T newValue)
        {
            Contract.Requires(instance != null);
            Contract.Requires(propertyName != null);

            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var propertyInfo = instance.GetType().GetProperty(propertyName, bindFlags);
            Contract.Assume(propertyInfo != null);
            propertyInfo.SetValue(instance, newValue);
        }

        /// <summary>
        /// Invokes the function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        internal static T InvokeFunction<T>(this object instance, string methodName)
        {
            Contract.Requires(instance != null);
            Contract.Requires(methodName != null);

            var methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Contract.Assume(methodInfo != null);

            var result = methodInfo.Invoke(instance, null);

            return result != null ? (T) result : default(T);
        }
    }
}