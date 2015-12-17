// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataRouteGenerator : IODataRouteGenerator
    {
        public List<SwaggerRoute> Generate(string routePrefix, IEdmModel model)
        {
            var routes = new List<SwaggerRoute>();

            routes.AddRange(GenerateEntitySetRoutes(routePrefix, model));
            routes.AddRange(GenerateEntityRoutes(routePrefix, model));
            routes.AddRange(GenerateOperationImportRoutes(routePrefix, model));
            routes.AddRange(GenerateOperationRoutes(routePrefix, model));

            return routes;
        }

        private static IEnumerable<SwaggerRoute> GenerateEntitySetRoutes(string routePrefix, IEdmModel model)
        {
            return model.EntityContainer
                .EntitySets()
                .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForEntitySet(routePrefix, entitySet), ODataSwaggerUtilities.CreateSwaggerPathForEntitySet(entitySet)));
        }

        private static IEnumerable<SwaggerRoute> GenerateEntityRoutes(string routePrefix, IEdmModel model)
        {
            return model.EntityContainer
                .EntitySets()
                .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForEntity(routePrefix, entitySet), ODataSwaggerUtilities.CreateSwaggerPathForEntity(entitySet)));
        }

        private static IEnumerable<SwaggerRoute> GenerateOperationImportRoutes(string routePrefix, IEdmModel model)
        {
            return model.EntityContainer
                .OperationImports()
                .Select(operationImport => new SwaggerRoute(ODataSwaggerUtilities.GetPathForOperationImport(routePrefix, operationImport), ODataSwaggerUtilities.CreateSwaggerPathForOperationImport(operationImport)));
        }

        /// <summary>
        /// Initialize the operations to Swagger model.
        /// </summary>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        private static IEnumerable<SwaggerRoute> GenerateOperationRoutes(string routePrefix, IEdmModel model)
        {
            var routes = new List<SwaggerRoute>();

            foreach (var operation in model.SchemaElements.OfType<IEdmOperation>())
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
                    routes.AddRange(model.EntityContainer
                        .EntitySets()
                        .Where(es => es.EntityType().Equals(entityType))
                        .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForOperationOfEntity(routePrefix, operation, entitySet), ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntity(operation, entitySet))));
                }
                else if (boundType.TypeKind == EdmTypeKind.Collection)
                {
                    var collectionType = boundType as IEdmCollectionType;

                    if (collectionType != null && collectionType.ElementType.Definition.TypeKind == EdmTypeKind.Entity)
                    {
                        var entityType = (IEdmEntityType) collectionType.ElementType.Definition;
                        routes.AddRange(model.EntityContainer
                            .EntitySets()
                            .Where(es => es.EntityType().Equals(entityType))
                            .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForOperationOfEntitySet(operation, entitySet, routePrefix), ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntitySet(operation, entitySet))));
                    }
                }
            }

            return routes;
        }
    }
}