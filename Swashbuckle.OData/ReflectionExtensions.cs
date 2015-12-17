using System.Reflection;

namespace Swashbuckle.OData
{
    internal static class ReflectionExtensions
    {
        /// <summary>
        ///     Uses reflection to get the field value from an object.
        /// </summary>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        /// <returns>The field value from the object.</returns>
        internal static T GetInstanceField<T>(this object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = instance.GetType().GetField(fieldName, bindFlags);
            return (T)field.GetValue(instance);
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
            var methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)methodInfo.Invoke(instance, null);
        }
    }
}