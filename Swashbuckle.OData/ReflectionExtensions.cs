using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.OData
{
    internal static class ReflectionExtensions
    {
        /// <summary>
        ///     Uses reflection to get the field value from an object.
        /// </summary>
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        /// <returns>The field value from the object.</returns>
        internal static object GetInstanceField(this Type type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        //public static T MergeFields<T>(this T mergeTo, T mergeFrom)
        //{
        //    var fields = mergeTo.GetType().GetFields();

        //    foreach (var fieldInfo in fields)
        //    {
        //        var fromValue = fieldInfo.GetValue(mergeFrom);

        //        if (fromValue != null)
        //        {
        //            Merge(mergeTo, fieldInfo, fromValue);
        //        }
        //    }

        //    return mergeTo;
        //}

        //private static void Merge<T>(T mergeTo, FieldInfo fieldInfo, object fromValue)
        //{
        //    var fromListValue = fromValue as IList;
        //    if (fromListValue != null)
        //    {
        //        var toListValue = fieldInfo.GetValue(mergeTo) as IList;

        //        foreach (var fromItem in fromListValue)
        //        {
        //            toListValue.Add(fromItem);
        //        }
        //    }
        //    else
        //    {
        //        var fromDictionaryValue = fromValue as IDictionary;
        //        if (fromDictionaryValue != null)
        //        {
        //            var toDictionaryValue = fieldInfo.GetValue(mergeTo) as IDictionary;

        //            if (toDictionaryValue != null)
        //            {
        //                var mergedDictionary = new Dictionary<object, object>();
        //                foreach (DictionaryEntry keyValuePair in toDictionaryValue)
        //                {
                            
        //                }
        //            }
        //        }
        //    }
        //}
    }
}