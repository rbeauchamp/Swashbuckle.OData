// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.OData.Formatter;
using Flurl;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    /// <summary>
    ///     Utility methods used to convert the Swagger model.
    /// </summary>
    internal static class ODataSwaggerUtilities
    {
        /// <summary>
        ///     Create the Swagger path for the Edm entity set.
        /// </summary>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject" /> represents the related Edm entity set.</returns>
        public static PathItem CreateSwaggerPathForEntitySet(IEdmNavigationSource navigationSource)
        {
            var entitySet = navigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                return new PathItem();
            }

            return new PathItem
            {
                get = new Operation()
                .Summary("Get EntitySet " + entitySet.Name)
                .OperationId(entitySet.Name + "_Get")
                .Description("Returns the EntitySet " + entitySet.Name)
                .Tags(entitySet.Name)
                .Parameters(new List<Parameter>()
                .Parameter("$expand", "query", "Expands related entities inline.", "string", false)
                .Parameter("$filter", "query", "Filters the results, based on a Boolean condition.", "string", false)
                .Parameter("$select", "query", "Selects which properties to include in the response.", "string", false)
                .Parameter("$orderby", "query", "Sorts the results.", "string", false)
                .Parameter("$top", "query", "Returns only the first n results.", "integer", false, "int32")
                .Parameter("$skip", "query", "Skips the first n results.", "integer", false, "int32")
                .Parameter("$count", "query", "Includes a count of the matching results in the reponse.", "boolean", false))
                .Responses(new Dictionary<string, Response>().Response("200", "EntitySet " + entitySet.Name, entitySet.Type).DefaultErrorResponse()),
                post = new Operation()
                .Summary("Post a new entity to EntitySet " + entitySet.Name)
                .OperationId(entitySet.Name + "_Post")
                .Description("Post a new entity to EntitySet " + entitySet.Name)
                .Tags(entitySet.Name)
                .Parameters(new List<Parameter>()
                .Parameter(entitySet.GetEntityType().Name, "body", "The entity to post", entitySet.GetEntityType()))
                .Responses(new Dictionary<string, Response>().Response("200", "EntitySet " + entitySet.Name, entitySet.GetEntityType()).DefaultErrorResponse())
            };
        }

        /// <summary>
        ///     Create the Swagger path for the Edm entity.
        /// </summary>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject" /> represents the related Edm entity.</returns>
        public static PathItem CreateSwaggerPathForEntity(IEdmNavigationSource navigationSource)
        {
            var entitySet = navigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                return new PathItem();
            }

            var keyParameters = new List<Parameter>();
            foreach (var key in entitySet.GetEntityType().GetKey())
            {
                Contract.Assume(key != null);
                string format;
                var keyDefinition = key.GetPropertyType().GetDefinition() as IEdmPrimitiveType;
                Contract.Assume(keyDefinition != null);
                var type = GetPrimitiveTypeAndFormat(keyDefinition, out format);
                keyParameters.Parameter(key.Name, "path", "key: " + key.Name, type, true, format);
            }

            return new PathItem
            {
                get = new Operation()
                .Summary("Get entity from " + entitySet.Name + " by key.")
                .OperationId(entitySet.Name + "_GetById")
                .Description("Returns the entity with the key from " + entitySet.Name)
                .Tags(entitySet.Name)
                .Parameters(keyParameters.DeepClone()
                .Parameter("$expand", "query", "Expands related entities inline.", "string", false))
                .Parameters(keyParameters.DeepClone()
                .Parameter("$select", "query", "Selects which properties to include in the response.", "string", false))
                .Responses(new Dictionary<string, Response>().Response("200", "EntitySet " + entitySet.Name, entitySet.GetEntityType()).DefaultErrorResponse()),

                patch = new Operation()
                .Summary("Update entity in EntitySet " + entitySet.Name)
                .OperationId(entitySet.Name + "_PatchById")
                .Description("Update entity in EntitySet " + entitySet.Name)
                .Tags(entitySet.Name)
                .Parameters(keyParameters.DeepClone()
                .Parameter(entitySet.GetEntityType().Name, "body", "The entity to patch", entitySet.GetEntityType()))
                .Responses(new Dictionary<string, Response>()
                .Response("204", "Empty response").DefaultErrorResponse()),

                put = new Operation()
                .Summary("Replace entity in EntitySet " + entitySet.Name)
                .OperationId(entitySet.Name + "_PutById")
                .Description("Replace entity in EntitySet " + entitySet.Name)
                .Tags(entitySet.Name)
                .Parameters(keyParameters.DeepClone()
                .Parameter(entitySet.GetEntityType().Name, "body", "The entity to put", entitySet.GetEntityType()))
                .Responses(new Dictionary<string, Response>().Response("204", "Empty response").DefaultErrorResponse()),

                delete = new Operation().Summary("Delete entity in EntitySet " + entitySet.Name)
                .OperationId(entitySet.Name + "_DeleteById")
                .Description("Delete entity in EntitySet " + entitySet.Name)
                .Tags(entitySet.Name)
                .Parameters(keyParameters.DeepClone()
                .Parameter("If-Match", "header", "If-Match header", "string", false))
                .Responses(new Dictionary<string, Response>().Response("204", "Empty response").DefaultErrorResponse())
            };
        }

        /// <summary>
        ///     Create the Swagger path for the Edm operation import.
        /// </summary>
        /// <param name="operationImport">The Edm operation import</param>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject" /> represents the related Edm operation import.</returns>
        public static PathItem CreateSwaggerPathForOperationImport(IEdmOperationImport operationImport)
        {
            Contract.Requires(operationImport == null || operationImport.Operation != null);
            Contract.Requires(operationImport == null || operationImport.Operation.Parameters != null);

            if (operationImport == null)
            {
                return new PathItem();
            }

            var isFunctionImport = operationImport is IEdmFunctionImport;
            var swaggerParameters = new List<Parameter>();
            foreach (var parameter in operationImport.Operation.Parameters)
            {
                var edmType = parameter.GetOperationType().GetDefinition();
                swaggerParameters.Parameter(parameter.Name, isFunctionImport ? "path" : "body", "parameter: " + parameter.Name, edmType);
            }

            var swaggerResponses = new Dictionary<string, Response>();
            if (operationImport.Operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operationImport.Name, operationImport.Operation.ReturnType.GetDefinition());
            }

            var swaggerOperationImport = new Operation()
                .Summary("Call operation import  " + operationImport.Name)
                .OperationId(operationImport.Name + (isFunctionImport ? "_FunctionImportGet" : "_ActionImportPost"))
                .Description("Call operation import  " + operationImport.Name)
                .Tags(isFunctionImport ? "Function Import" : "Action Import");

            if (swaggerParameters.Count > 0)
            {
                swaggerOperationImport.Parameters(swaggerParameters);
            }
            swaggerOperationImport.Responses(swaggerResponses.DefaultErrorResponse());

            return isFunctionImport ? new PathItem
            {
                get = swaggerOperationImport
            } : new PathItem
            {
                post = swaggerOperationImport
            };
        }

        /// <summary>
        ///     Create the Swagger path for the Edm operation bound to the Edm entity set.
        /// </summary>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>
        ///     The <see cref="Newtonsoft.Json.Linq.JObject" /> represents the related Edm operation bound to the Edm entity
        ///     set.
        /// </returns>
        public static PathItem CreateSwaggerPathForOperationOfEntitySet(IEdmOperation operation, IEdmNavigationSource navigationSource)
        {
            Contract.Requires(operation == null || !(navigationSource is IEdmEntitySet) || operation.Parameters != null);

            var entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return new PathItem();
            }

            var isFunction = operation is IEdmFunction;
            var swaggerParameters = new List<Parameter>();
            foreach (var parameter in operation.Parameters.Skip(1))
            {
                var edmType = parameter.GetOperationType().GetDefinition();
                swaggerParameters.Parameter(parameter.Name, isFunction ? "path" : "body", "parameter: " + parameter.Name, edmType);
            }

            var swaggerResponses = new Dictionary<string, Response>();
            if (operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operation.Name, operation.ReturnType.GetDefinition());
            }

            var swaggerOperation = new Operation()
                .Summary("Call operation  " + operation.Name)
                .OperationId(operation.Name + (isFunction ? "_FunctionGet" : "_ActionPost"))
                .Description("Call operation  " + operation.Name)
                .OperationId(operation.Name + (isFunction ? "_FunctionGetById" : "_ActionPostById"))
                .Tags(entitySet.Name, isFunction ? "Function" : "Action");

            if (swaggerParameters.Count > 0)
            {
                swaggerOperation.Parameters(swaggerParameters);
            }
            swaggerOperation.Responses(swaggerResponses.DefaultErrorResponse());

            return isFunction ? new PathItem
            {
                get = swaggerOperation
            } : new PathItem
            {
                post = swaggerOperation
            };
        }

        /// <summary>
        ///     Create the Swagger path for the Edm operation bound to the Edm entity.
        /// </summary>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject" /> represents the related Edm operation bound to the Edm entity.</returns>
        public static PathItem CreateSwaggerPathForOperationOfEntity(IEdmOperation operation, IEdmNavigationSource navigationSource)
        {
            var entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return new PathItem();
            }

            var isFunction = operation is IEdmFunction;
            var swaggerParameters = new List<Parameter>();

            foreach (var key in entitySet.GetEntityType().GetKey())
            {
                Contract.Assume(key != null);
                string format;
                var edmPrimitiveType = key.GetPropertyType().GetDefinition() as IEdmPrimitiveType;
                Contract.Assume(edmPrimitiveType != null);
                var type = GetPrimitiveTypeAndFormat(edmPrimitiveType, out format);
                swaggerParameters.Parameter(key.Name, "path", "key: " + key.Name, type, true, format);
            }

            foreach (var parameter in operation.Parameters.Skip(1))
            {
                swaggerParameters.Parameter(parameter.Name, isFunction ? "path" : "body", "parameter: " + parameter.Name, parameter.GetOperationType().GetDefinition());
            }

            var swaggerResponses = new Dictionary<string, Response>();
            if (operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operation.Name, operation.ReturnType.GetDefinition());
            }

            var swaggerOperation = new Operation()
                .Summary("Call operation  " + operation.Name)
                .Description("Call operation  " + operation.Name)
                .Tags(entitySet.Name, isFunction ? "Function" : "Action");

            if (swaggerParameters.Count > 0)
            {
                swaggerOperation.Parameters(swaggerParameters);
            }
            swaggerOperation.Responses(swaggerResponses.DefaultErrorResponse());

            return isFunction ? new PathItem
            {
                get = swaggerOperation
            } : new PathItem
            {
                post = swaggerOperation
            };
        }

        /// <summary>
        /// Gets the path for entity set.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="entitySet">The entity set.</param>
        /// <returns></returns>
        public static Url GetPathForEntitySet(string routePrefix, IEdmEntitySet entitySet)
        {
            Contract.Requires(entitySet != null);

            return routePrefix.AppendPathSegment(entitySet.Name);
        }

        /// <summary>
        ///     Get the Uri Swagger path for the Edm entity set.
        /// </summary>
        /// <param name="routePrefix"></param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="System.String" /> path represents the related Edm entity set.</returns>
        public static string GetPathForEntity(string routePrefix, IEdmNavigationSource navigationSource)
        {
            var entitySet = navigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                return string.Empty;
            }

            var singleEntityPath = GetPathForEntitySet(routePrefix, entitySet) + "(";
            singleEntityPath = entitySet.GetEntityType().GetKey().Count() == 1 
                ? AppendSingleColumnKeyTemplate(entitySet, singleEntityPath) 
                : AppendMultiColumnKeyTemplate(entitySet, singleEntityPath);
            Contract.Assume(singleEntityPath.Length - 2 >= 0);
            singleEntityPath = singleEntityPath.Substring(0, singleEntityPath.Length - 2);
            singleEntityPath += ")";

            return singleEntityPath;
        }

        private static string AppendSingleColumnKeyTemplate(IEdmEntitySet entitySet, string singleEntityPath)
        {
            Contract.Requires(entitySet.GetEntityType().GetKey().Count() == 1);
            Contract.Ensures(Contract.Result<string>() != null);

            var key = entitySet.GetEntityType().GetKey().Single();
            singleEntityPath += "{" + key.Name + "}, ";
            return singleEntityPath;
        }

        private static string AppendMultiColumnKeyTemplate(IEdmEntitySet entitySet, string singleEntityPath)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            foreach (var key in entitySet.GetEntityType().GetKey())
            {
                Contract.Assume(key != null);
                singleEntityPath += key.Name + "={" + key.Name + "}, ";
            }
            Contract.Assume(singleEntityPath != null);
            return singleEntityPath;
        }

        /// <summary>
        /// Get the Uri Swagger path for Edm operation import.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="operationImport">The Edm operation import.</param>
        /// <returns>
        /// The <see cref="string" /> path represents the related Edm operation import.
        /// </returns>
        public static string GetPathForOperationImport(string routePrefix, IEdmOperationImport operationImport)
        {
            if (operationImport == null)
            {
                return string.Empty;
            }

            var swaggerOperationImportPath = routePrefix.AppendPathSegment(operationImport.Name) + "(";
            if (operationImport.IsFunctionImport())
            {
                swaggerOperationImportPath = operationImport.Operation.Parameters.Aggregate(swaggerOperationImportPath, (current, parameter) => current + parameter.Name + "=" + "{" + parameter.Name + "},");
            }
            if (swaggerOperationImportPath.EndsWith(",", StringComparison.Ordinal))
            {
                swaggerOperationImportPath = swaggerOperationImportPath.Substring(0, swaggerOperationImportPath.Length - 1);
            }
            swaggerOperationImportPath += ")";

            return swaggerOperationImportPath;
        }

        /// <summary>
        ///     Get the Uri Swagger path for Edm operation bound to entity set.
        /// </summary>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <param name="routePrefix"></param>
        /// <returns>The <see cref="System.String" /> path represents the related Edm operation.</returns>
        public static string GetPathForOperationOfEntitySet(IEdmOperation operation, IEdmNavigationSource navigationSource, string routePrefix)
        {
            var entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return string.Empty;
            }

            var swaggerOperationPath = GetPathForEntitySet(routePrefix, entitySet) +"/" + operation.FullName() + "(";
            if (operation.IsFunction())
            {
                var edmOperationParameters = operation.Parameters;
                Contract.Assume(edmOperationParameters != null);
                foreach (var parameter in edmOperationParameters.Skip(1))
                {
                    Contract.Assume(parameter != null);
                    swaggerOperationPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                }
            }
            if (swaggerOperationPath.EndsWith(",", StringComparison.Ordinal))
            {
                swaggerOperationPath = swaggerOperationPath.Substring(0, swaggerOperationPath.Length - 1);
            }
            swaggerOperationPath += ")";

            return swaggerOperationPath;
        }

        /// <summary>
        /// Get the Uri Swagger path for Edm operation bound to entity.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>
        /// The <see cref="System.String" /> path represents the related Edm operation.
        /// </returns>
        public static string GetPathForOperationOfEntity(string routePrefix, IEdmOperation operation, IEdmNavigationSource navigationSource)
        {
            var entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return string.Empty;
            }

            var swaggerOperationPath = GetPathForEntity(routePrefix, entitySet) + "/" + operation.FullName() + "(";
            if (operation.IsFunction())
            {
                var edmOperationParameters = operation.Parameters;
                Contract.Assume(edmOperationParameters != null);
                foreach (var parameter in edmOperationParameters.Skip(1))
                {
                    Contract.Assume(parameter != null);
                    swaggerOperationPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                }
            }
            if (swaggerOperationPath.EndsWith(",", StringComparison.Ordinal))
            {
                swaggerOperationPath = swaggerOperationPath.Substring(0, swaggerOperationPath.Length - 1);
            }
            swaggerOperationPath += ")";

            return swaggerOperationPath;
        }

        /// <summary>
        ///     Create the Swagger definition for the structure Edm type.
        /// </summary>
        /// <param name="edmType">The structure Edm type.</param>
        /// <returns>
        ///     The <see cref="Schema" /> represents the related structure Edm type.
        /// </returns>
        public static Schema CreateSwaggerDefinitionForStructureType(IEdmStructuredType edmType)
        {
            Contract.Requires(edmType.StructuralProperties() != null);

            if (edmType == null)
            {
                return new Schema();
            }

            var swaggerProperties = new Dictionary<string, Schema>();
            foreach (var property in edmType.StructuralProperties())
            {
                var swaggerProperty = new Schema().Description(property.Name);
                SetSwaggerType(swaggerProperty, property.GetPropertyType().GetDefinition());
                swaggerProperties.Add(property.Name, swaggerProperty);
            }

            return new Schema
            {
                properties = swaggerProperties
            };
        }

        private static void SetSwaggerType(Parameter obj, IEdmType edmType)
        {
            Contract.Requires(obj != null);
            Contract.Requires(edmType != null);
            Contract.Requires(edmType.TypeKind != EdmTypeKind.Collection || ((IEdmCollectionType)edmType).ElementType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    obj.@ref = "#/definitions/" + edmType.FullTypeName();
                    break;
                case EdmTypeKind.Primitive:
                    string format;
                    var type = GetPrimitiveTypeAndFormat((IEdmPrimitiveType) edmType, out format);
                    obj.type = type;
                    if (format != null)
                    {
                        obj.format = format;
                    }
                    break;
                case EdmTypeKind.Enum:
                    obj.type = "string";
                    break;
                case EdmTypeKind.Collection:
                    var itemEdmType = ((IEdmCollectionType) edmType).ElementType.GetDefinition();
                    var nestedItem = new Parameter();
                    SetSwaggerType(nestedItem, itemEdmType);
                    obj.type = "array";
                    obj.items = nestedItem;
                    break;
                case EdmTypeKind.None:
                    break;
                case EdmTypeKind.EntityReference:
                    break;
                case EdmTypeKind.TypeDefinition:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void SetSwaggerType(Schema obj, IEdmType edmType)
        {
            Contract.Requires(obj != null);
            Contract.Requires(edmType != null);
            Contract.Requires(edmType.TypeKind != EdmTypeKind.Collection || ((IEdmCollectionType)edmType).ElementType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    obj.@ref = "#/definitions/" + edmType.FullTypeName();
                    break;
                case EdmTypeKind.Primitive:
                    string format;
                    var type = GetPrimitiveTypeAndFormat((IEdmPrimitiveType) edmType, out format);
                    obj.type = type;
                    if (format != null)
                    {
                        obj.format = format;
                    }
                    break;
                case EdmTypeKind.Enum:
                    obj.type = "string";
                    break;
                case EdmTypeKind.Collection:
                    var itemEdmType = ((IEdmCollectionType) edmType).ElementType.GetDefinition();
                    var nestedItem = new Schema();
                    SetSwaggerType(nestedItem, itemEdmType);
                    obj.type = "array";
                    obj.items = nestedItem;
                    break;
                case EdmTypeKind.None:
                    break;
                case EdmTypeKind.EntityReference:
                    break;
                case EdmTypeKind.TypeDefinition:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetPrimitiveTypeAndFormat(IEdmPrimitiveType primitiveType, out string format)
        {
            Contract.Requires(primitiveType != null);

            format = null;
            switch (primitiveType.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.String:
                case EdmPrimitiveTypeKind.None:
                    return "string";
                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                    format = "int32";
                    return "integer";
                case EdmPrimitiveTypeKind.Int64:
                    format = "int64";
                    return "integer";
                case EdmPrimitiveTypeKind.Boolean:
                    return "boolean";
                case EdmPrimitiveTypeKind.Byte:
                    format = "byte";
                    return "string";
                case EdmPrimitiveTypeKind.Date:
                    format = "date";
                    return "string";
                case EdmPrimitiveTypeKind.DateTimeOffset:
                    format = "date-time";
                    return "string";
                case EdmPrimitiveTypeKind.Double:
                    format = "double";
                    return "number";
                case EdmPrimitiveTypeKind.Single:
                    format = "float";
                    return "number";
                case EdmPrimitiveTypeKind.Guid:
                    format = "guid";
                    return "string";
                case EdmPrimitiveTypeKind.Binary:
                    format = "binary";
                    return "string";
                default:
                    return "string";
            }
        }

        private static Operation Responses(this Operation obj, IDictionary<string, Response> responses)
        {
            Contract.Requires(obj != null);

            obj.responses = responses;
            return obj;
        }

        private static IDictionary<string, Response> ResponseRef(this IDictionary<string, Response> responses, string name, string description, string refType)
        {
            Contract.Requires(responses != null);
            Contract.Requires(name != null);

            responses.Add(name, new Response
            {
                description = description,
                schema = new Schema
                {
                    @ref = refType
                }
            });

            return responses;
        }

        private static IDictionary<string, Response> Response(this IDictionary<string, Response> responses, string name, string description, IEdmType type)
        {
            Contract.Requires(responses != null);
            Contract.Requires(name != null);

            var schema = new Schema();
            SetSwaggerType(schema, type);

            responses.Add(name, new Response
            {
                description = description,
                schema = schema
            });

            return responses;
        }

        private static IDictionary<string, Response> DefaultErrorResponse(this IDictionary<string, Response> responses)
        {
            Contract.Requires(responses != null);

            return responses.ResponseRef("default", "Unexpected error", "#/definitions/_Error");
        }

        private static IDictionary<string, Response> Response(this IDictionary<string, Response> responses, string name, string description)
        {
            Contract.Requires(responses != null);
            Contract.Requires(name != null);

            responses.Add(name, new Response
            {
                description = description
            });

            return responses;
        }

        private static Operation Parameters(this Operation obj, IList<Parameter> parameters)
        {
            Contract.Requires(obj != null);

            obj.parameters = parameters;
            return obj;
        }

        private static IList<Parameter> Parameter(this IList<Parameter> parameters, string name, string kind, string description, string type, bool required, string format = null)
        {
            Contract.Requires(parameters != null);

            parameters.Add(new Parameter
            {
                name = name,
                @in = kind,
                description = description,
                type = type,
                format = format,
                required = required
            });

            return parameters;
        }

        internal static IList<Parameter> Parameter(this IList<Parameter> parameters, string name, string kind, string description, IEdmType type)
        {
            Contract.Requires(parameters != null);
            Contract.Requires(type != null);

            var parameter = new Parameter
            {
                name = name,
                @in = kind,
                description = description,
                required = true
            };

            if (kind != "body")
            {
                SetSwaggerType(parameter, type);
            }
            else
            {
                var schema = new Schema();
                SetSwaggerType(schema, type);
                parameter.schema = schema;
            }

            parameters.Add(parameter);
            return parameters;
        }

        private static Operation Tags(this Operation obj, params string[] tags)
        {
            Contract.Requires(obj != null);

            obj.tags = tags;
            return obj;
        }

        private static Operation Summary(this Operation obj, string summary)
        {
            Contract.Requires(obj != null);

            obj.summary = summary;
            return obj;
        }

        private static Operation Description(this Operation obj, string description)
        {
            Contract.Requires(obj != null);

            obj.description = description;
            return obj;
        }

        private static Schema Description(this Schema obj, string description)
        {
            Contract.Requires(obj != null);

            obj.description = description;
            return obj;
        }

        private static Operation OperationId(this Operation obj, string operationId)
        {
            Contract.Requires(obj != null);

            obj.operationId = operationId;
            return obj;
        }

        /// <summary>
        ///     Perform a deep Copy of the object, using Json as a serialisation method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T DeepClone<T>(this T source)
            where T : class
        {
            Contract.Ensures(Contract.Result<T>() != null || source == null);

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            };
            // Don't serialize a null object, simply return the default for that object
            if (source == null)
            {
                return null;
            }
            var result = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializerSettings), serializerSettings);
            Contract.Assume(result != null);
            return result;
        }
    }
}