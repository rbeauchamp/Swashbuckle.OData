using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal static class ParameterExtensions
    {
        public static ParameterSource MapToSwaggerSource(this Parameter parameter)
        {
            Contract.Requires(parameter != null);

            switch (parameter.@in)
            {
                case "query":
                    return ParameterSource.Query;
                case "header":
                    return ParameterSource.Header;
                case "path":
                    return ParameterSource.Path;
                case "body":
                    return ParameterSource.Body;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
            }
        }

        public static ApiParameterSource MapToApiParameterSource(this Parameter parameter)
        {
            Contract.Requires(parameter != null);

            switch (parameter.@in)
            {
                case "query":
                    return ApiParameterSource.FromUri;
                case "header":
                    return ApiParameterSource.Unknown;
                case "path":
                    return ApiParameterSource.FromUri;
                case "body":
                    return ApiParameterSource.FromBody;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
            }
        }

        public static Type GetClrType(this Parameter parameter)
        {
            Contract.Requires(parameter != null);

            var type = parameter.type;
            var format = parameter.format;

            switch (format)
            {
                case null:
                    switch (type)
                    {
                        case null:
                            Contract.Assume(parameter.@in.Equals(@"body"));
                            Contract.Assume(!string.IsNullOrWhiteSpace(parameter.schema?.@ref));
                            return GetEntityTypeForBodyParameter(parameter);
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

        public static string GenerateSamplePathParameterValue(this Parameter parameter)
        {
            Contract.Requires(parameter != null);
            Contract.Requires(parameter.@in == "path");

            var type = parameter.type;
            var format = parameter.format;

            switch (format)
            {
                case null:
                    switch (type)
                    {
                        case "string":
                            if (parameter.@enum != null && parameter.@enum.Any())
                            {
                                return parameter.@enum.First().ToString();
                            }
                            return "\'SampleString\'";
                        case "boolean":
                            return "true";
                        case "array":
                            return "[]";
                        default:
                            throw new Exception($"Could not generate sample value for query parameter type {type} and format {"null"}");
                    }
                case "int32":
                case "int64":
                    return "42";
                case "byte":
                    return "1";
                case "date":
                    return "2015-12-12T12:00";
                case "date-time":
                    return "2015-10-10T17:00:00Z";
                case "double":
                    return "2.34d";
                case "float":
                    return "2.0f";
                case "guid":
                    return Guid.NewGuid().ToString();
                case "binary":
                    return Convert.ToBase64String(new byte[] { 130, 200, 234, 23 });
                default:
                    throw new Exception($"Could not generate sample value for query parameter type {type} and format {format}");
            }
        }

        private static Type GetEntityTypeForBodyParameter(Parameter parameter)
        {
            Contract.Requires(parameter != null);
            Contract.Requires(parameter.schema != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(parameter.schema.@ref));
            Contract.Requires(parameter.@in == "body");

            var fullTypeName = parameter.schema.@ref.Replace("#/definitions/", string.Empty);

            return FindType(fullTypeName);
        }

        public static Type GetEntityType(this Schema schema)
        {
            Contract.Requires(schema != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(schema.@ref));

            var fullTypeName = schema.@ref.Replace("#/definitions/", string.Empty);

            return FindType(fullTypeName);
        }

        public static Type GetEntitySetType(this Schema schema)
        {
            Contract.Requires(schema != null);
            Contract.Requires(schema.type == "array");
            Contract.Requires(schema.items != null);
            Contract.Requires(schema.items.@ref != null);

            var queryableType = typeof(IQueryable<>);
            var fullTypeName = schema.items.@ref.Replace("#/definitions/", string.Empty);
            var entityType = FindType(fullTypeName);
            return queryableType.MakeGenericType(entityType);
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