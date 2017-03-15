using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using System.Xml.XPath;
using Swashbuckle.Swagger.XmlComments;

namespace Swashbuckle.OData
{
    internal class SwashbuckleOptions
    {
        public SwashbuckleOptions(SwaggerProviderOptions swaggerProviderOptions)
        {
            Contract.Requires(swaggerProviderOptions != null);

            VersionSupportResolver = swaggerProviderOptions.VersionSupportResolver;
            Schemes = swaggerProviderOptions.Schemes;
            SecurityDefinitions = swaggerProviderOptions.SecurityDefinitions;
            IgnoreObsoleteActions = swaggerProviderOptions.IgnoreObsoleteActions;
            GroupingKeySelector = swaggerProviderOptions.GroupingKeySelector ?? Descriptions.ApiDescriptionExtensions.DefaultGroupingKeySelector;
            GroupingKeyComparer = swaggerProviderOptions.GroupingKeyComparer ?? Comparer<string>.Default;
            CustomSchemaMappings = swaggerProviderOptions.CustomSchemaMappings ?? new Dictionary<Type, Func<Schema>>();
            SchemaFilters = swaggerProviderOptions.SchemaFilters ?? new List<ISchemaFilter>();
            ModelFilters = swaggerProviderOptions.ModelFilters ?? new List<IModelFilter>();
            IgnoreObsoleteProperties = swaggerProviderOptions.IgnoreObsoleteProperties;
            SchemaIdSelector = swaggerProviderOptions.SchemaIdSelector ?? DefaultSchemaIdSelector;
            DescribeAllEnumsAsStrings = swaggerProviderOptions.DescribeAllEnumsAsStrings;
            DescribeStringEnumsInCamelCase = swaggerProviderOptions.DescribeStringEnumsInCamelCase;
            OperationFilters = swaggerProviderOptions.OperationFilters ?? new List<IOperationFilter>();
            DocumentFilters = swaggerProviderOptions.DocumentFilters ?? new List<IDocumentFilter>();
            ConflictingActionsResolver = swaggerProviderOptions.ConflictingActionsResolver;
            ApplyFiltersToAllSchemas = swaggerProviderOptions.ApplyFiltersToAllSchemas;
            XmlDocFactories = swaggerProviderOptions.XmlDocFactories;
            
            //Add the xml comments files to the filters
            foreach (var xmlDocFactory in XmlDocFactories)
            {
                var xmlDoc = xmlDocFactory();
                this.ModelFilters.Insert(0, new ApplyXmlTypeComments(xmlDoc));
                this.OperationFilters.Insert(0, new ApplyXmlActionComments(xmlDoc));
            }

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

        private static string DefaultSchemaIdSelector(Type type)
        {
            return type.FriendlyId();
        }
    }
}