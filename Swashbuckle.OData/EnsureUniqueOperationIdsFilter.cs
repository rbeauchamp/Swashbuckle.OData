using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public class EnsureUniqueOperationIdsFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            Contract.Assume(swaggerDoc != null);
            Contract.Assume(schemaRegistry != null);
            Contract.Assume(apiExplorer != null);

            var pathItems = swaggerDoc.paths.Values;

            var deletes = pathItems.Select(pathItem => pathItem.delete).Where(operation => operation != null);
            var gets = pathItems.Select(pathItem => pathItem.get).Where(operation => operation != null);
            var heads = pathItems.Select(pathItem => pathItem.head).Where(operation => operation != null);
            var patches = pathItems.Select(pathItem => pathItem.patch).Where(operation => operation != null);
            var puts = pathItems.Select(pathItem => pathItem.put).Where(operation => operation != null);
            var posts = pathItems.Select(pathItem => pathItem.post).Where(operation => operation != null);
            var options = pathItems.Select(pathItem => pathItem.options).Where(operation => operation != null);

            var allOperations = deletes.ConcatEvenIfNull(gets)
                                       .ConcatEvenIfNull(heads)
                                       .ConcatEvenIfNull(patches)
                                       .ConcatEvenIfNull(puts)
                                       .ConcatEvenIfNull(posts)
                                       .ConcatEvenIfNull(options)
                                       .ToList();

            AppendParameterNamesToOperationId(allOperations);

            UniquifyOperationId(allOperations);
        }

        private static void AppendParameterNamesToOperationId(List<Operation> allOperations)
        {
            Contract.Requires(allOperations != null);

            foreach (var operation in allOperations.Where(operation => !operation.operationId.Contains("By") 
                                                                        && operation.parameters != null
                                                                        && operation.parameters.Any(p => p.@in == "path")))
            {
                // Select the capitalized parameter names
                var parameters = operation.parameters
                    .Where(p => p.@in == "path")
                    .Select(p => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(p.name));

                // Set the operation id to match the format "OperationByParam1AndParam2"
                operation.operationId = $"{operation.operationId}By{string.Join("And", parameters)}";
            }
        }

        private static void UniquifyOperationId(List<Operation> allOperations)
        {
            Contract.Requires(allOperations != null);

            foreach (var operationsWithDupIds in allOperations.GroupBy(operation => operation.operationId).Where(grouping => grouping.Count() > 1))
            {
                var sequence = 1;
                foreach (var operationsWithDupId in operationsWithDupIds)
                {
                    operationsWithDupId.operationId = $"{operationsWithDupId.operationId}_{sequence}";
                    sequence++;
                }
            }
        }

    }
}