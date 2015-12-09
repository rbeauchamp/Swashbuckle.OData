// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData.Edm;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal class SwaggerPathGenerator
    {
        private readonly string _routePrefix;
        private readonly IEdmModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerPathGenerator" /> class.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="model">The Edm model.</param>
        public SwaggerPathGenerator(string routePrefix, IEdmModel model)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(routePrefix));
            _routePrefix = routePrefix;
            _model = model;
        }

        /// <summary>
        /// Converts the Edm model to Swagger model.
        /// </summary>
        public Dictionary<string, PathItem> GenerateSwaggerPaths()
        {
            Contract.Ensures(Contract.Result<Dictionary<string, PathItem>>() != null);

            var paths = GeneratePaths(_routePrefix, _model);

            GenerateOperations(_routePrefix, paths);

            return paths;
        }

        private static Dictionary<string, PathItem> GeneratePaths(string routePrefix, IEdmModel model)
        {
            var paths = new Dictionary<string, PathItem>();

            foreach (var entitySet in model.EntityContainer.EntitySets())
            {
                paths.Add(ODataSwaggerUtilities.GetPathForEntitySet(routePrefix, entitySet), ODataSwaggerUtilities.CreateSwaggerPathForEntitySet(entitySet));

                paths.Add(ODataSwaggerUtilities.GetPathForEntity(routePrefix, entitySet), ODataSwaggerUtilities.CreateSwaggerPathForEntity(entitySet));
            }

            foreach (var operationImport in model.EntityContainer.OperationImports())
            {
                paths.Add(ODataSwaggerUtilities.GetPathForOperationImport(operationImport), ODataSwaggerUtilities.CreateSwaggerPathForOperationImport(operationImport));
            }

            return paths;
        }

        /// <summary>
        ///     Initialize the operations to Swagger model.
        /// </summary>
        /// <param name="routePrefix"></param>
        /// <param name="paths"></param>
        private void GenerateOperations(string routePrefix, IDictionary<string, PathItem> paths)
        {
            Contract.Requires(paths != null);

            if (_model.EntityContainer == null)
            {
                return;
            }

            foreach (var operation in _model.SchemaElements.OfType<IEdmOperation>())
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
                        _model.EntityContainer.EntitySets().Where(es => es.EntityType().Equals(entityType)))
                    {
                        paths.Add(ODataSwaggerUtilities.GetPathForOperationOfEntity(routePrefix, operation, entitySet), ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntity(operation, entitySet));
                    }
                }
                else if (boundType.TypeKind == EdmTypeKind.Collection)
                {
                    var collectionType = boundType as IEdmCollectionType;

                    if (collectionType != null && collectionType.ElementType.Definition.TypeKind == EdmTypeKind.Entity)
                    {
                        var entityType = (IEdmEntityType) collectionType.ElementType.Definition;
                        foreach (var entitySet in
                            _model.EntityContainer.EntitySets().Where(es => es.EntityType().Equals(entityType)))
                        {
                            paths.Add(ODataSwaggerUtilities.GetPathForOperationOfEntitySet(operation, entitySet, _routePrefix), ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntitySet(operation, entitySet));
                        }
                    }
                }
            }
        }
    }
}