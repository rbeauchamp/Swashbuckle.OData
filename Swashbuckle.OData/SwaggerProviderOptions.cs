using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using System.Xml.XPath;

namespace Swashbuckle.OData
{
    internal class SwaggerProviderOptions
    {
        public SwaggerProviderOptions(
            Func<ApiDescription, string, bool> versionSupportResolver,
            IEnumerable<string> schemes,
            IDictionary<string, SecurityScheme> securityDefinitions,
            bool ignoreObsoleteActions,
            Func<ApiDescription, string> groupingKeySelector,
            IComparer<string> groupingKeyComparer,
            IDictionary<Type, Func<Schema>> customSchemaMappings,
            IEnumerable<ISchemaFilter> schemaFilters,
            IList<IModelFilter> modelFilters,
            bool ignoreObsoleteProperties,
            Func<Type, string> schemaIdSelector,
            bool describeAllEnumsAsStrings,
            bool describeStringEnumsInCamelCase,
            IList<IOperationFilter> operationFilters,
            IEnumerable<IDocumentFilter> documentFilters,
            Func<IEnumerable<ApiDescription>, ApiDescription> conflictingActionsResolver,
            bool applyFiltersToAllSchemas,
            IList<Func<XPathDocument>> xmlDocFactories
            )
        {
            VersionSupportResolver = versionSupportResolver;
            Schemes = schemes;
            SecurityDefinitions = securityDefinitions;
            IgnoreObsoleteActions = ignoreObsoleteActions;
            GroupingKeySelector = groupingKeySelector;
            GroupingKeyComparer = groupingKeyComparer;
            CustomSchemaMappings = customSchemaMappings;
            SchemaFilters = schemaFilters;
            ModelFilters = modelFilters;
            IgnoreObsoleteProperties = ignoreObsoleteProperties;
            SchemaIdSelector = schemaIdSelector;
            DescribeAllEnumsAsStrings = describeAllEnumsAsStrings;
            DescribeStringEnumsInCamelCase = describeStringEnumsInCamelCase;
            OperationFilters = operationFilters;
            DocumentFilters = documentFilters;
            ConflictingActionsResolver = conflictingActionsResolver;
            ApplyFiltersToAllSchemas = applyFiltersToAllSchemas;
            XmlDocFactories = xmlDocFactories;
        }

        public Func<ApiDescription, string, bool> VersionSupportResolver { get; private set; }

        public IEnumerable<string> Schemes { get; private set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

        public bool IgnoreObsoleteActions { get; private set; }

        public Func<ApiDescription, string> GroupingKeySelector { get; private set; }

        public IComparer<string> GroupingKeyComparer { get; private set; }

        public IDictionary<Type, Func<Schema>> CustomSchemaMappings { get; private set; }

        public IEnumerable<ISchemaFilter> SchemaFilters { get; private set; }

        public IList<IModelFilter> ModelFilters { get; private set; }

        public bool IgnoreObsoleteProperties { get; private set; }

        public Func<Type, string> SchemaIdSelector { get; private set; }

        public bool DescribeAllEnumsAsStrings { get; private set; }

        public bool DescribeStringEnumsInCamelCase { get; private set; }

        public IList<IOperationFilter> OperationFilters { get; private set; }

        public IEnumerable<IDocumentFilter> DocumentFilters { get; private set; }

        public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; private set; }

        public bool ApplyFiltersToAllSchemas { get; private set; }

        public IList<Func<XPathDocument>> XmlDocFactories { get; private set; }
    }
}