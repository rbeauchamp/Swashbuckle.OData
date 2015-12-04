// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    /// <summary>
    ///     Utility methods used to convert the Swagger model.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "o", Justification = "Utils is spelled correctly.")]
    public static class ODataSwaggerUtilities
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
                .Parameters(new List<Parameter>().Parameter("$expand", "query", "Expands related entities inline.", "string")
                .Parameter("$filter", "query", "Filters the results, based on a Boolean condition.", "string")
                .Parameter("$select", "query", "Selects which properties to include in the response.", "string")
                .Parameter("$orderby", "query", "Sorts the results.", "string")
                .Parameter("$top", "query", "Returns only the first n results.", "integer", "int32")
                .Parameter("$skip", "query", "Skips the first n results.", "integer", "int32")
                .Parameter("$count", "query", "Includes a count of the matching results in the reponse.", "boolean"))
                .Responses(new Dictionary<string, Response>().Response("200", "EntitySet " + entitySet.Name, entitySet.EntityType()).DefaultErrorResponse()),
                post = new Operation()
                .Summary("Post a new entity to EntitySet " + entitySet.Name)
                .OperationId(entitySet.Name + "_Post")
                .Description("Post a new entity to EntitySet " + entitySet.Name)
                .Tags(entitySet.Name).Parameters(new List<Parameter>()
                .Parameter(entitySet.EntityType().Name, "body", "The entity to post", entitySet.EntityType()))
                .Responses(new Dictionary<string, Response>().Response("200", "EntitySet " + entitySet.Name, entitySet.EntityType()).DefaultErrorResponse())
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
            foreach (var key in entitySet.EntityType().Key())
            {
                string format;
                var type = GetPrimitiveTypeAndFormat(key.Type.Definition as IEdmPrimitiveType, out format);
                keyParameters.Parameter(key.Name, "path", "key: " + key.Name, type, format);
            }

            return new PathItem
            {
                get = new Operation()
                .Summary("Get entity from " + entitySet.Name + " by key.")
                .OperationId(entitySet.Name + "_GetById")
                .Description("Returns the entity with the key from " + entitySet.Name)
                .Tags(entitySet.Name).Parameters(keyParameters.DeepClone()
                .Parameter("$expand", "query", "Expands related entities inline.", "string"))
                .Parameters(keyParameters.DeepClone().Parameter("$select", "query", "Selects which properties to include in the response.", "string"))
                .Responses(new Dictionary<string, Response>().Response("200", "EntitySet " + entitySet.Name, entitySet.EntityType()).DefaultErrorResponse()),

                patch = new Operation()
                .Summary("Update entity in EntitySet " + entitySet.Name)
                .OperationId(entitySet.Name + "_PatchById")
                .Description("Update entity in EntitySet " + entitySet.Name)
                .Tags(entitySet.Name)
                .Parameters(keyParameters.DeepClone().Parameter(entitySet.EntityType().Name, "body", "The entity to patch", entitySet.EntityType()))
                .Responses(new Dictionary<string, Response>().Response("204", "Empty response").DefaultErrorResponse()),

                delete = new Operation().Summary("Delete entity in EntitySet " + entitySet.Name)
                .OperationId(entitySet.Name + "_DeleteById")
                .Description("Delete entity in EntitySet " + entitySet.Name)
                .Tags(entitySet.Name)
                .Parameters(keyParameters.DeepClone().Parameter("If-Match", "header", "If-Match header", "string"))
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
            if (operationImport == null)
            {
                return new PathItem();
            }

            var isFunctionImport = operationImport is IEdmFunctionImport;
            var swaggerParameters = new List<Parameter>();
            foreach (var parameter in operationImport.Operation.Parameters)
            {
                swaggerParameters.Parameter(parameter.Name, isFunctionImport ? "path" : "body", "parameter: " + parameter.Name, parameter.Type.Definition);
            }

            var swaggerResponses = new Dictionary<string, Response>();
            if (operationImport.Operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operationImport.Name, operationImport.Operation.ReturnType.Definition);
            }

            var swaggerOperationImport = new Operation().Summary("Call operation import  " + operationImport.Name).OperationId(operationImport.Name + (isFunctionImport ? "_FunctionImportGet" : "_ActionImportPost")).Description("Call operation import  " + operationImport.Name).Tags(isFunctionImport ? "Function Import" : "Action Import");

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
            var entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return new PathItem();
            }

            var isFunction = operation is IEdmFunction;
            var swaggerParameters = new List<Parameter>();
            foreach (var parameter in operation.Parameters.Skip(1))
            {
                swaggerParameters.Parameter(parameter.Name, isFunction ? "path" : "body", "parameter: " + parameter.Name, parameter.Type.Definition);
            }

            var swaggerResponses = new Dictionary<string, Response>();
            if (operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operation.Name, operation.ReturnType.Definition);
            }

            var swaggerOperation = new Operation().Summary("Call operation  " + operation.Name).OperationId(operation.Name + (isFunction ? "_FunctionGet" : "_ActionPost")).Description("Call operation  " + operation.Name).OperationId(operation.Name + (isFunction ? "_FunctionGetById" : "_ActionPostById")).Tags(entitySet.Name, isFunction ? "Function" : "Action");

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

            foreach (var key in entitySet.EntityType().Key())
            {
                string format;
                var type = GetPrimitiveTypeAndFormat(key.Type.Definition as IEdmPrimitiveType, out format);
                swaggerParameters.Parameter(key.Name, "path", "key: " + key.Name, type, format);
            }

            foreach (var parameter in operation.Parameters.Skip(1))
            {
                swaggerParameters.Parameter(parameter.Name, isFunction ? "path" : "body", "parameter: " + parameter.Name, parameter.Type.Definition);
            }

            var swaggerResponses = new Dictionary<string, Response>();
            if (operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operation.Name, operation.ReturnType.Definition);
            }

            var swaggerOperation = new Operation().Summary("Call operation  " + operation.Name).Description("Call operation  " + operation.Name).Tags(entitySet.Name, isFunction ? "Function" : "Action");

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
        ///     Get the Uri Swagger path for the Edm entity set.
        /// </summary>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="System.String" /> path represents the related Edm entity set.</returns>
        public static string GetPathForEntity(IEdmNavigationSource navigationSource)
        {
            var entitySet = navigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                return string.Empty;
            }

            var singleEntityPath = "/" + entitySet.Name + "(";
            foreach (var key in entitySet.EntityType().Key())
            {
                if (key.Type.Definition.TypeKind == EdmTypeKind.Primitive && ((IEdmPrimitiveType) key.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.String)
                {
                    singleEntityPath += "'{" + key.Name + "}', ";
                }
                else
                {
                    singleEntityPath += "{" + key.Name + "}, ";
                }
            }
            singleEntityPath = singleEntityPath.Substring(0, singleEntityPath.Length - 2);
            singleEntityPath += ")";

            return singleEntityPath;
        }

        /// <summary>
        ///     Get the Uri Swagger path for Edm operation import.
        /// </summary>
        /// <param name="operationImport">The Edm operation import.</param>
        /// <returns>The <see cref="System.String" /> path represents the related Edm operation import.</returns>
        public static string GetPathForOperationImport(IEdmOperationImport operationImport)
        {
            if (operationImport == null)
            {
                return string.Empty;
            }

            var swaggerOperationImportPath = "/" + operationImport.Name + "(";
            if (operationImport.IsFunctionImport())
            {
                foreach (var parameter in operationImport.Operation.Parameters)
                {
                    swaggerOperationImportPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                }
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
        /// <returns>The <see cref="System.String" /> path represents the related Edm operation.</returns>
        public static string GetPathForOperationOfEntitySet(IEdmOperation operation, IEdmNavigationSource navigationSource)
        {
            var entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return string.Empty;
            }

            var swaggerOperationPath = "/" + entitySet.Name + "/" + operation.FullName() + "(";
            if (operation.IsFunction())
            {
                foreach (var parameter in operation.Parameters.Skip(1))
                {
                    if (parameter.Type.Definition.TypeKind == EdmTypeKind.Primitive && ((IEdmPrimitiveType) parameter.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.String)
                    {
                        swaggerOperationPath += parameter.Name + "=" + "'{" + parameter.Name + "}',";
                    }
                    else
                    {
                        swaggerOperationPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                    }
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
        ///     Get the Uri Swagger path for Edm operation bound to entity.
        /// </summary>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="System.String" /> path represents the related Edm operation.</returns>
        public static string GetPathForOperationOfEntity(IEdmOperation operation, IEdmNavigationSource navigationSource)
        {
            var entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return string.Empty;
            }

            var swaggerOperationPath = GetPathForEntity(entitySet) + "/" + operation.FullName() + "(";
            if (operation.IsFunction())
            {
                foreach (var parameter in operation.Parameters.Skip(1))
                {
                    if (parameter.Type.Definition.TypeKind == EdmTypeKind.Primitive && ((IEdmPrimitiveType) parameter.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.String)
                    {
                        swaggerOperationPath += parameter.Name + "=" + "'{" + parameter.Name + "}',";
                    }
                    else
                    {
                        swaggerOperationPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                    }
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
            if (edmType == null)
            {
                return new Schema();
            }

            var swaggerProperties = new Dictionary<string, Schema>();
            foreach (var property in edmType.StructuralProperties())
            {
                var swaggerProperty = new Schema().Description(property.Name);
                SetSwaggerType(swaggerProperty, property.Type.Definition);
                swaggerProperties.Add(property.Name, swaggerProperty);
            }

            return new Schema
            {
                properties = swaggerProperties
            };
        }

        private static void SetSwaggerType(Parameter obj, IEdmType edmType)
        {
            Contract.Assert(obj != null);
            Contract.Assert(edmType != null);

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
                    var itemEdmType = ((IEdmCollectionType) edmType).ElementType.Definition;
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
            Contract.Assert(obj != null);
            Contract.Assert(edmType != null);

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
                    var itemEdmType = ((IEdmCollectionType) edmType).ElementType.Definition;
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
            Contract.Assert(primitiveType != null);

            format = null;
            switch (primitiveType.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.String:
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
                default:
                    return "string";
            }
        }

        private static Operation Responses(this Operation obj, IDictionary<string, Response> responses)
        {
            obj.responses = responses;
            return obj;
        }

        private static IDictionary<string, Response> ResponseRef(this IDictionary<string, Response> responses, string name, string description, string refType)
        {
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
            return responses.ResponseRef("default", "Unexpected error", "#/definitions/_Error");
        }

        private static IDictionary<string, Response> Response(this IDictionary<string, Response> responses, string name, string description)
        {
            responses.Add(name, new Response
            {
                description = description
            });

            return responses;
        }

        private static Operation Parameters(this Operation obj, IList<Parameter> parameters)
        {
            obj.parameters = parameters;
            return obj;
        }

        private static IList<Parameter> Parameter(this IList<Parameter> parameters, string name, string kind, string description, string type, string format = null)
        {
            parameters.Add(new Parameter
            {
                name = name,
                @in = kind,
                description = description,
                type = type,
                format = format
            });

            //if (!string.IsNullOrEmpty(format))
            //{
            //    parameters.First().format = format;
            //}

            return parameters;
        }

        private static IList<Parameter> Parameter(this IList<Parameter> parameters, string name, string kind, string description, IEdmType type)
        {
            var parameter = new Parameter
            {
                name = name,
                @in = kind,
                description = description
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
            obj.tags = tags;
            return obj;
        }

        private static Operation Summary(this Operation obj, string summary)
        {
            obj.summary = summary;
            return obj;
        }

        private static Operation Description(this Operation obj, string description)
        {
            obj.description = description;
            return obj;
        }

        private static Schema Description(this Schema obj, string description)
        {
            obj.description = description;
            return obj;
        }

        private static Operation OperationId(this Operation obj, string operationId)
        {
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
        {
            // Don't serialize a null object, simply return the default for that object
            return ReferenceEquals(source, null) ? default(T) : JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
        }
    }
}