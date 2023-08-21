using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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

            var fieldInfo = instance.GetType().GetAllFields().SingleOrDefault(info => info.Name == fieldName);
            Contract.Assume(fieldInfo != null);
            var value = fieldInfo.GetValue(instance);
            Contract.Assume(value != null || !ensureNonNull);
            return value != null ? (T)value : default (T);
        }

        private static IEnumerable<FieldInfo> GetAllFields(this Type type)
        {
            if (type == null)
            {
                return Enumerable.Empty<FieldInfo>();
            }

            const BindingFlags flags = BindingFlags.Public 
                | BindingFlags.NonPublic 
                | BindingFlags.Static 
                | BindingFlags.Instance 
                | BindingFlags.DeclaredOnly;

            return type.GetFields(flags).Concat(GetAllFields(type.BaseType));   
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
        internal static T InvokeFunction<T>(this object instance, string methodName, params object[] methodParams)
        {
            Contract.Requires(instance != null);
            Contract.Requires(methodName != null);

            var methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
            {
                methodInfo = instance.GetType().BaseType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            }
            Contract.Assume(methodInfo != null);

            var result = methodInfo.Invoke(instance, methodParams);

            return result != null ? (T) result : default(T);
        }
    }
}