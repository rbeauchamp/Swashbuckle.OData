using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using FluentAssertions;
using SwashbuckleODataSample.Models;

namespace Swashbuckle.OData.Tests
{
    public class ProductWithStringKeysController : ODataController
    {
        private static readonly ConcurrentDictionary<string, ProductWithStringKey> Data;

        static ProductWithStringKeysController()
        {
            Data = new ConcurrentDictionary<string, ProductWithStringKey>();
            var rand = new Random();

            Enumerable.Range(0, 100).Select(i => new ProductWithStringKey
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Product " + i,
                Price = rand.NextDouble()*1000
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        public IHttpActionResult Put([FromODataUri] string key, [FromBody] ProductWithStringKey product)
        {
            key.Should().NotStartWith("'");
            key.Should().NotEndWith("'");

            return Updated(Data.Values.First());
        }
    }
}