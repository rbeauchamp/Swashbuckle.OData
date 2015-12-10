using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal static class ParameterExtensions
    {
        public static Type GetClrType(this Parameter parameter)
        {
            var type = parameter.type;
            var format = parameter.format;

            switch (format)
            {
                case null:
                    switch (type)
                    {
                        case null:
                            return GetEntityType(parameter);
                        case "string":
                            return typeof(string);
                        case "boolean":
                            return typeof(bool);
                        default:
                            throw new Exception($"Could not determine .NET type for parameter type {type} and format 'null'");
                    }
                case "int32":
                    return typeof(int);
                case "int64":
                    return typeof(long);
                case "byte":
                    return typeof(byte);
                case "date":
                    return typeof(DateTime);
                case "date-time":
                    return typeof(DateTimeOffset);
                case "double":
                    return typeof(double);
                case "float":
                    return typeof(float);
                case "guid":
                    return typeof(Guid);
                case "binary":
                    return typeof(byte[]);
                default:
                    throw new Exception($"Could not determine .NET type for parameter type {type} and format {format}");
            }
        }

        private static Type GetEntityType(Parameter parameter)
        {
            Contract.Requires(parameter.@in == "body");

            var fullTypeName = parameter.schema.@ref.Replace("#/definitions/", string.Empty);

            return FindType(fullTypeName);
        }

        public static Type GetEntityType(this Schema schema)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(schema.@ref));

            var fullTypeName = schema.@ref.Replace("#/definitions/", string.Empty);

            return FindType(fullTypeName);
        }

        /// <summary>
        /// Looks in all loaded assemblies for the given type.
        /// </summary>
        /// <param name="fullName">
        /// The full name of the type.
        /// </param>
        /// <returns>
        /// The <see cref="Type"/> found; null if not found.
        /// </returns>
        private static Type FindType(string fullName)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .First(t => t.FullName.Equals(fullName));
        }
    }
}