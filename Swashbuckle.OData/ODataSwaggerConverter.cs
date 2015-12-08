// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData.Edm;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    /// <summary>
    ///     Represents an <see cref="ODataSwaggerConverter" /> used to converter an Edm model to Swagger model.
    /// </summary>
    internal class ODataSwaggerConverter
    {
        private const string DefaultHost = "default";
        private const string DefaultbasePath = "/odata";
        private static readonly Uri DefaultMetadataUri = new Uri("http://localhost");

        /// <summary>
        ///     Initializes a new instance of the <see cref="ODataSwaggerConverter" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public ODataSwaggerConverter(IEdmModel model)
        {
            Contract.Requires(model != null);

            EdmModel = model;
            MetadataUri = DefaultMetadataUri;
            Host = DefaultHost;
            BasePath = DefaultbasePath;
        }

        /// <summary>
        ///     Gets or sets the metadata Uri in the Swagger model.
        /// </summary>
        public Uri MetadataUri { get; set; }

        /// <summary>
        ///     Gets or sets the host in the Swagger model.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///     Gets or sets the base path in the Swagger model.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        ///     Gets or sets the Edm model.
        /// </summary>
        public IEdmModel EdmModel { get; }

        /// <summary>
        ///     Gets the document in the Swagger.
        /// </summary>
        private SwaggerDocument SwaggerDoc { get; set; }

        /// <summary>
        ///     Gets the paths in the Swagger.
        /// </summary>
        private IDictionary<string, PathItem> SwaggerPaths { get; set; }

        /// <summary>
        ///     Gets the definitions in the Swagger.
        /// </summary>
        private IDictionary<string, Schema> SwaggerDefinitions { get; set; }

        /// <summary>
        /// Converts the Edm model to Swagger model.
        /// </summary>
        public SwaggerDocument ConvertToSwaggerModel()
        {
            Contract.Ensures(Contract.Result<SwaggerDocument>() != null);

            if (SwaggerDoc != null)
            {
                return SwaggerDoc;
            }

            InitializeStart();
            InitializeDocument();
            InitializeContainer();
            InitializeTypeDefinitions();
            InitializeOperations();
            InitializeEnd();

            return SwaggerDoc;
        }

        /// <summary>
        ///     Start to initialize the Swagger model.
        /// </summary>
        private void InitializeStart()
        {
            SwaggerDoc = null;
            SwaggerPaths = null;
            SwaggerDefinitions = null;
        }

        /// <summary>
        ///     Initialize the document of Swagger model.
        /// </summary>
        private void InitializeDocument()
        {
            SwaggerDoc = new SwaggerDocument
            {
                info = new Info
                {
                    title = "OData Service",
                    description = "The OData Service at " + MetadataUri,
                    version = "0.1.0"
                },
                host = Host,
                schemes = new List<string>
                {
                    "http"
                },
                basePath = BasePath,
                consumes = new List<string>
                {
                    "application/json"
                },
                produces = new List<string>
                {
                    "application/json"
                }
            };
        }

        /// <summary>
        ///     Initialize the entity container to Swagger model.
        /// </summary>
        private void InitializeContainer()
        {
            Contract.Requires(SwaggerDoc != null);
            Contract.Requires(EdmModel != null);

            SwaggerPaths = new Dictionary<string, PathItem>();

            SwaggerDoc.paths = SwaggerPaths;

            if (EdmModel.EntityContainer == null)
            {
                return;
            }

            foreach (var entitySet in EdmModel.EntityContainer.EntitySets())
            {
                SwaggerPaths.Add("/" + entitySet.Name, ODataSwaggerUtilities.CreateSwaggerPathForEntitySet(entitySet));

                SwaggerPaths.Add(ODataSwaggerUtilities.GetPathForEntity(entitySet), ODataSwaggerUtilities.CreateSwaggerPathForEntity(entitySet));
            }

            foreach (var operationImport in EdmModel.EntityContainer.OperationImports())
            {
                SwaggerPaths.Add(ODataSwaggerUtilities.GetPathForOperationImport(operationImport), ODataSwaggerUtilities.CreateSwaggerPathForOperationImport(operationImport));
            }
        }

        /// <summary>
        ///     Initialize the type definitions to Swagger model.
        /// </summary>
        private void InitializeTypeDefinitions()
        {
            Contract.Requires(SwaggerDoc != null);
            Contract.Requires(EdmModel != null);

            SwaggerDefinitions = new Dictionary<string, Schema>();
            SwaggerDoc.definitions = SwaggerDefinitions;

            foreach (var type in EdmModel.SchemaElements.OfType<IEdmStructuredType>())
            {
                SwaggerDefinitions.Add(type.FullTypeName(), ODataSwaggerUtilities.CreateSwaggerDefinitionForStructureType(type));
            }
        }

        /// <summary>
        ///     Initialize the operations to Swagger model.
        /// </summary>
        private void InitializeOperations()
        {
            Contract.Requires(SwaggerDoc != null);
            Contract.Requires(EdmModel != null);
            Contract.Requires(SwaggerPaths != null);

            if (EdmModel.EntityContainer == null)
            {
                return;
            }

            foreach (var operation in EdmModel.SchemaElements.OfType<IEdmOperation>())
            {
                // skip unbound operation
                if (!operation.IsBound)
                {
                    continue;
                }

                var boundParameter = operation.Parameters.First();
                var boundType = boundParameter.Type.Definition;

                // skip operation bound to non entity (or entity collection)
                if (boundType.TypeKind == EdmTypeKind.Entity)
                {
                    var entityType = (IEdmEntityType) boundType;
                    foreach (var entitySet in
                        EdmModel.EntityContainer.EntitySets().Where(es => es.EntityType().Equals(entityType)))
                    {
                        SwaggerPaths.Add(ODataSwaggerUtilities.GetPathForOperationOfEntity(operation, entitySet), ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntity(operation, entitySet));
                    }
                }
                else if (boundType.TypeKind == EdmTypeKind.Collection)
                {
                    var collectionType = boundType as IEdmCollectionType;

                    if (collectionType != null && collectionType.ElementType.Definition.TypeKind == EdmTypeKind.Entity)
                    {
                        var entityType = (IEdmEntityType) collectionType.ElementType.Definition;
                        foreach (var entitySet in
                            EdmModel.EntityContainer.EntitySets().Where(es => es.EntityType().Equals(entityType)))
                        {
                            SwaggerPaths.Add(ODataSwaggerUtilities.GetPathForOperationOfEntitySet(operation, entitySet), ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntitySet(operation, entitySet));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     End to initialize the Swagger model.
        /// </summary>
        private void InitializeEnd()
        {
            Contract.Requires(SwaggerDefinitions != null);

            SwaggerDefinitions.Add("_Error", new Schema
            {
                properties = new Dictionary<string, Schema>
                {
                    {"error", new Schema
                    {
                        @ref = "#/definitions/_InError"
                    }}
                }
            });

            SwaggerDefinitions.Add("_InError", new Schema
            {
                properties = new Dictionary<string, Schema>
                {
                    {"code", new Schema
                    {
                        type = "string"
                    }},
                    {"message", new Schema
                    {
                        type = "string"
                    }}
                }
            });
        }
    }
}