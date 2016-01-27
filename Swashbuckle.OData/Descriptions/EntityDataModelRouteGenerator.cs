// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace Swashbuckle.OData.Descriptions
{
    /// <summary>
    /// Generates a set of potential SwaggerRoutes based upon the <see cref="IEdmModel"/>
    /// associated with an ODataRoute.
    /// </summary>
    internal class EntityDataModelRouteGenerator : ISwaggerRouteGenerator
    {
        public IEnumerable<SwaggerRoute> Generate(HttpConfiguration httpConfig)
        {
            return httpConfig.GetODataRoutes().SelectMany(Generate);
        }

        private static List<SwaggerRoute> Generate(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Requires(oDataRoute.Constraints != null);

            var routes = new List<SwaggerRoute>();

            routes.AddRangeIfNotNull(GenerateEntitySetRoutes(oDataRoute));
            routes.AddRangeIfNotNull(GenerateEntityRoutes(oDataRoute));
            routes.AddRangeIfNotNull(GenerateOperationImportRoutes(oDataRoute));
            routes.AddRangeIfNotNull(GenerateOperationRoutes(oDataRoute));

            return routes;
        }

        private static IEnumerable<SwaggerRoute> GenerateEntitySetRoutes(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Requires(oDataRoute.Constraints != null);

            return oDataRoute.GetEdmModel()
                .EntityContainer
                .EntitySets()?
                .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForEntitySet(oDataRoute.GetRoutePrefix(), entitySet), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForEntitySet(entitySet)));
        }

        private static IEnumerable<SwaggerRoute> GenerateEntityRoutes(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Requires(oDataRoute.Constraints != null);

            return oDataRoute.GetEdmModel()
                .EntityContainer
                .EntitySets()?
                .Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForEntity(oDataRoute.GetRoutePrefix(), entitySet), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForEntity(entitySet)));
        }

        private static IEnumerable<SwaggerRoute> GenerateOperationImportRoutes(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Requires(oDataRoute.Constraints != null);

            return oDataRoute.GetEdmModel()
                .EntityContainer
                .OperationImports()?
                .Select(operationImport => new SwaggerRoute(ODataSwaggerUtilities.GetPathForOperationImport(oDataRoute.GetRoutePrefix(), operationImport), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForOperationImport(operationImport)));
        }

        /// <summary>
        /// Initialize the operations to Swagger model.
        /// </summary>
        /// <param name="oDataRoute">The o data route.</param>
        /// <returns></returns>
        private static IEnumerable<SwaggerRoute> GenerateOperationRoutes(ODataRoute oDataRoute)
        {
            Contract.Requires(oDataRoute != null);
            Contract.Requires(oDataRoute.Constraints != null);

            var routes = new List<SwaggerRoute>();

            var edmSchemaElements = oDataRoute.GetEdmModel().SchemaElements;
            if (edmSchemaElements != null)
            {
                foreach (var operation in edmSchemaElements.OfType<IEdmOperation>())
                {
                    // skip unbound operation
                    if (!operation.IsBound)
                    {
                        continue;
                    }

                    var edmOperationParameters = operation.Parameters;
                    if (edmOperationParameters != null && edmOperationParameters.Any())
                    {
                        var boundParameter = edmOperationParameters.First();
                        Contract.Assume(boundParameter != null);

                        var boundType = boundParameter.GetOperationType().GetDefinition();

                        // skip operation bound to non entity (or entity collection)
                        if (boundType.TypeKind == EdmTypeKind.Entity)
                        {
                            var entityType = (IEdmEntityType)boundType;
                            var edmEntitySets = oDataRoute.GetEdmModel().EntityContainer.EntitySets();
                            Contract.Assume(edmEntitySets != null);
                            routes.AddRange(edmEntitySets.Where(es => es.GetEntityType().Equals(entityType)).Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForOperationOfEntity(oDataRoute.GetRoutePrefix(), operation, entitySet), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntity(operation, entitySet))));
                        }
                        else if (boundType.TypeKind == EdmTypeKind.Collection)
                        {
                            var collectionType = boundType as IEdmCollectionType;

                            if (collectionType?.ElementType?.GetDefinition().TypeKind == EdmTypeKind.Entity)
                            {
                                var entityType = (IEdmEntityType)collectionType.ElementType?.GetDefinition();
                                var edmEntitySets = oDataRoute.GetEdmModel().EntityContainer.EntitySets();
                                Contract.Assume(edmEntitySets != null);
                                routes.AddRange(edmEntitySets.Where(es => es.GetEntityType().Equals(entityType)).Select(entitySet => new SwaggerRoute(ODataSwaggerUtilities.GetPathForOperationOfEntitySet(operation, entitySet, oDataRoute.GetRoutePrefix()), oDataRoute, ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntitySet(operation, entitySet))));
                            }
                        }
                    }
                }
            }

            return routes;
        }
    }
}