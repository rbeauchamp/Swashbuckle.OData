using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using SwashbuckleODataSample;

namespace Swashbuckle.OData.Tests
{
    public static class ValidationUtils
    {
        public static async Task ValidateSwaggerJson()
        {
            // Arrange
            var httpClient = HttpClientUtils.GetHttpClient(HttpClientUtils.BaseAddress, ODataConfig.ODataRoutePrefix);

            // Act
            var response = await httpClient.GetAsync("swagger/docs/v1");

            // Assert
            await response.ValidateSuccessAsync();
            var swaggerJson = await response.Content.ReadAsStringAsync();

            var resolver = new JSchemaPreloadedResolver();
            resolver.Add(new Uri("http://json-schema.org/draft-04/schema"), File.ReadAllText(@"schema-draft-v4.json"));

            var swaggerSchema = File.ReadAllText(@"swagger-2.0-schema.json");
            var schema = JSchema.Parse(swaggerSchema, resolver);

            var swaggerJObject = JObject.Parse(swaggerJson);
            IList<string> messages;
            var isValid = swaggerJObject.IsValid(schema, out messages);
            isValid.Should().BeTrue();
        }
    }
}