using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Tests
{
    public static class ValidationUtils
    {
        public static async Task ValidateSwaggerJson()
        {
            // Arrange
            var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress);

            // Act
            var response = await httpClient.GetAsync("swagger/docs/v1");

            // Assert
            await response.ValidateSuccessAsync();
            await IsValidAgainstJsonSchemaAsync(response);
            await HasUniqueOperationIdsAsync(response);
        }

        private static async Task HasUniqueOperationIdsAsync(HttpResponseMessage response)
        {
            var swaggerDoc = await response.Content.ReadAsAsync<SwaggerDocument>();

            var deletes = swaggerDoc.paths.Values.Select(pathItem => pathItem.delete).Where(operation => operation != null);
            var gets = swaggerDoc.paths.Values.Select(pathItem => pathItem.get).Where(operation => operation != null);
            var heads = swaggerDoc.paths.Values.Select(pathItem => pathItem.head).Where(operation => operation != null);
            var patches = swaggerDoc.paths.Values.Select(pathItem => pathItem.patch).Where(operation => operation != null);
            var puts = swaggerDoc.paths.Values.Select(pathItem => pathItem.put).Where(operation => operation != null);
            var posts = swaggerDoc.paths.Values.Select(pathItem => pathItem.post).Where(operation => operation != null);
            var options = swaggerDoc.paths.Values.Select(pathItem => pathItem.options).Where(operation => operation != null);

            deletes.ConcatEvenIfNull(gets)
                .ConcatEvenIfNull(heads)
                .ConcatEvenIfNull(patches)
                .ConcatEvenIfNull(puts)
                .ConcatEvenIfNull(posts)
                .ConcatEvenIfNull(options)
                .GroupBy(operation => operation.operationId)
                .All(grouping => grouping.Count() == 1)
                .Should()
                .BeTrue();
        }

        private static async Task IsValidAgainstJsonSchemaAsync(HttpResponseMessage response)
        {
            var outputDir = AppDomain.CurrentDomain.BaseDirectory;
            var resolver = new JSchemaPreloadedResolver();
            resolver.Add(new Uri("http://json-schema.org/draft-04/schema"), File.ReadAllText($@"{outputDir}\schema-draft-v4.json"));

            var swaggerSchema = File.ReadAllText($@"{outputDir}\swagger-2.0-schema.json");
            var schema = JSchema.Parse(swaggerSchema, resolver);

            var swaggerJson = await response.Content.ReadAsStringAsync();
            var swaggerJObject = JObject.Parse(swaggerJson);

            IList<string> messages;

            var isValid = swaggerJObject.IsValid(schema, out messages);
            isValid.Should().BeTrue();
        }
    }
}