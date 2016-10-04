using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Routing;
using SwashbuckleODataSample.Models;

namespace Swashbuckle.OData.Tests
{
    public class ProductsV1Controller : ODataController
    {
        private static readonly ConcurrentDictionary<int, Product> Data;

        static ProductsV1Controller()
        {
            Data = new ConcurrentDictionary<int, Product>();
            var rand = new Random();

            var enumValues = Enum.GetValues(typeof(MyEnum));

            Enumerable.Range(0, 100).Select(i => new Product
            {
                Id = i,
                Name = "Product " + i,
                Price = rand.NextDouble() * 1000,
                EnumValue = (MyEnum)enumValues.GetValue(rand.Next(enumValues.Length))
            }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        [HttpGet]
        [ResponseType(typeof(List<Product>))]
        public IHttpActionResult MultipleParams([FromODataUri]int Id, [FromODataUri]int Year)
        {
            return Ok(Data.Values.Take(2));
        }

        [HttpGet]
        [ResponseType(typeof(List<Product>))]
        public IHttpActionResult EnumParam([FromODataUri]int Id, [FromODataUri]MyEnum EnumValue)
        {
            return Ok(Data.Values.Take(2));
        }

        [HttpGet]
        [ResponseType(typeof(bool))]
        public IHttpActionResult IsEnumValueMatch([FromODataUri] int key, [FromODataUri] MyEnum EnumValue)
        {
            return Ok(Data[key].EnumValue == EnumValue);
        }

        /// <summary>
        /// Get the most expensive product. This is a function bound to a collection.
        /// </summary>
        [HttpGet]
        [ResponseType(typeof(double))]
        public IHttpActionResult MostExpensive()
        {
            var retval = Data.Max(pair => pair.Value.Price);

            return Ok(retval);
        }

        /// <summary>
        /// Get the top 10 most expensive products. This is a function bound to a collection that returns a collection.
        /// </summary>
        [HttpGet]
        [ResponseType(typeof(List<Product>))]
        public IHttpActionResult Top10()
        {
            var retval = Data.Values.OrderByDescending(p => p.Price).Take(10).ToList();

            return Ok(retval);
        }

        /// <summary>
        /// Get the rank of the product price. This is a function bound to an entity.
        /// </summary>
        /// <param name="key">The product id</param>
        [HttpGet]
        [ResponseType(typeof(int))]
        public IHttpActionResult GetPriceRank(int key)
        {
            Product p;
            if (Data.TryGetValue(key, out p))
            {
                // NOTE: Use where clause to get the rank of the price may not
                // offer the good time complexity. The following code is intended
                // for demostration only.
                return Ok(Data.Values.Count(one => one.Price > p.Price));
            }
            return NotFound();
        }

        /// <summary>
        /// Get the sales tax for a product in a given state. This is a function which accepts a parameter.
        /// </summary>
        /// <param name="key">The product id</param>
        /// <param name="state">The state</param>
        [HttpGet]
        [ResponseType(typeof(double))]
        public IHttpActionResult CalculateGeneralSalesTax(int key, string state)
        {
            var taxRate = GetRate(state);

            Product product;
            if (Data.TryGetValue(key, out product))
            {
                var tax = product.Price * taxRate / 100;
                return Ok(tax);
            }
            return NotFound();
        }

        /// <summary>
        /// Get the sales tax rate for a state. This is an unbound function.
        /// </summary>
        /// <param name="state">The state</param>
        [HttpGet]
        [ResponseType(typeof(double))]
        [ODataRoute("GetSalesTaxRate(state={state})")]
        public IHttpActionResult GetSalesTaxRate([FromODataUri] string state)
        {
            return Ok(GetRate(state));
        }

        private static double GetRate(string state)
        {
            double taxRate;
            switch (state)
            {
                case "AZ":
                    taxRate = 5.6;
                    break;
                case "CA":
                    taxRate = 7.5;
                    break;
                case "CT":
                    taxRate = 6.35;
                    break;
                case "GA":
                    taxRate = 4;
                    break;
                case "IN":
                    taxRate = 7;
                    break;
                case "KS":
                    taxRate = 6.15;
                    break;
                case "KY":
                    taxRate = 6;
                    break;
                case "MA":
                    taxRate = 6.25;
                    break;
                case "NV":
                    taxRate = 6.85;
                    break;
                case "NJ":
                    taxRate = 7;
                    break;
                case "NY":
                    taxRate = 4;
                    break;
                case "NC":
                    taxRate = 4.75;
                    break;
                case "ND":
                    taxRate = 5;
                    break;
                case "PA":
                    taxRate = 6;
                    break;
                case "TN":
                    taxRate = 7;
                    break;
                case "TX":
                    taxRate = 6.25;
                    break;
                case "VA":
                    taxRate = 4.3;
                    break;
                case "WA":
                    taxRate = 6.5;
                    break;
                case "WV":
                    taxRate = 6;
                    break;
                case "WI":
                    taxRate = 5;
                    break;

                default:
                    taxRate = 0;
                    break;
            }

            return taxRate;
        }
    }
}