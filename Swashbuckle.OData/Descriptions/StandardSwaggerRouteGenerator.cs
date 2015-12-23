// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace Swashbuckle.OData.Descriptions
{
    internal class StandardSwaggerRouteGenerator : ISwaggerRouteGenerator
    {
        public IEnumerable<SwaggerRoute> Generate(HttpConfiguration httpConfig)
        {
            return httpConfig.GetODataRoutes().SelectMany(Generate);
        }

        private static List<SwaggerRoute> Generate(ODataRoute oDataRoute)
        {
            var routes = new List<SwaggerRoute>();

            routes.AddRange(GenerateEntitySetRoutes(oDataRoute));
            routes.AddRange(GenerateEntityRoutes(oDataRoute));
            routes.AddRange(GenerateOperationImportRoutes(oDataRoute));
            routes.AddRange(GenerateOperationRoutes(oDataRoute));

            return routes;
        }

        private static IEnumerable<SwaggerRoute> GenerateEntitySetRoutes(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);

            return oDataRoute.GetEdmModel().EntityContainer
                .EntitySets()
                .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForEntitySet(oDataRoute.RoutePrefix, entitySet), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForEntitySet(entitySet)));
        }

        private static IEnumerable<SwaggerRoute> GenerateEntityRoutes(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);

            return oDataRoute.GetEdmModel().EntityContainer
                .EntitySets()
                .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForEntity(oDataRoute.RoutePrefix, entitySet), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForEntity(entitySet)));
        }

        private static IEnumerable<SwaggerRoute> GenerateOperationImportRoutes(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);

            return oDataRoute.GetEdmModel().EntityContainer
                .OperationImports()
                .Select(operationImport => new SwaggerRoute(ODataSwaggerUtilities.GetPathForOperationImport(oDataRoute.RoutePrefix, operationImport), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForOperationImport(operationImport)));
        }

        /// <summary>
        /// Initialize the operations to Swagger model.
        /// </summary>
        /// <param name="oDataRoute">The o data route.</param>
        /// <returns></returns>
        private static IEnumerable<SwaggerRoute> GenerateOperationRoutes(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);

            var routes = new List<SwaggerRoute>();

            foreach (var operation in oDataRoute.GetEdmModel().SchemaElements.OfType<IEdmOperation>())
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
                    routes.AddRange(oDataRoute.GetEdmModel().EntityContainer
                        .EntitySets()
                        .Where(es => es.EntityType().Equals(entityType))
                        .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForOperationOfEntity(oDataRoute.RoutePrefix, operation, entitySet), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntity(operation, entitySet))));
                }
                else if (boundType.TypeKind == EdmTypeKind.Collection)
                {
                    var collectionType = boundType as IEdmCollectionType;

                    if (collectionType != null && collectionType.ElementType.Definition.TypeKind == EdmTypeKind.Entity)
                    {
                        var entityType = (IEdmEntityType) collectionType.ElementType.Definition;
                        routes.AddRange(oDataRoute.GetEdmModel().EntityContainer
                            .EntitySets()
                            .Where(es => es.EntityType().Equals(entityType))
                            .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForOperationOfEntitySet(operation, entitySet, oDataRoute.RoutePrefix), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntitySet(operation, entitySet))));
                    }
                }
            }

            return routes;
        }
    }
}