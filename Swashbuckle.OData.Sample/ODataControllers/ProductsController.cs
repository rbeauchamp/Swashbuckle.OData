using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using SwashbuckleODataSample.Models;

namespace SwashbuckleODataSample.ODataControllers
{
    public class ProductsController : ODataController
    {
        private static readonly ConcurrentDictionary<int, Product> Data;

        static ProductsController()
        {
            Data = new ConcurrentDictionary<int, Product>();
            var rand = new Random();

            var enumValues = Enum.GetValues(typeof(MyEnum));

            Enumerable.Range(0, 100).Select(i => new Product
            {
                Id = i,
                Name = "Product " + i,
                Price = rand.NextDouble()*1000,
                EnumValue = (MyEnum)enumValues.GetValue(rand.Next(enumValues.Length))
        }).ToList().ForEach(p => Data.TryAdd(p.Id, p));
        }

        /// <summary>
        /// Query products
        /// </summary>
        [EnableQuery]
        public IQueryable<Product> GetProducts()
        {
            return Data.Values.AsQueryable();
        }

        /// <summary>
        /// Demonstrates a function that accepts an enum parameter from the OData URI.
        /// </summary>
        [HttpGet]
        [ResponseType(typeof(List<Product>))]
        public IHttpActionResult GetByEnumValue([FromODataUri]MyEnum EnumValue)
        {
            return Ok(Data.Values.Where(product => product.EnumValue == EnumValue));
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
        [EnableQuery]
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
        /// Get the sales tax for a product in a given state. This function accepts a parameter.
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
                var tax = product.Price*taxRate/100;
                return Ok(tax);
            }
            return NotFound();
        }

        /// <summary>
        /// Get products with the given Ids. This function accepts a parameter of type 'array'.
        /// </summary>
        /// <param name="Ids">The ids.</param>
        [HttpGet]
        [EnableQuery]
        public IQueryable<Product> ProductsWithIds([FromODataUri]int[] Ids)
        {
            return Data.Values.Where(p => Ids.Contains(p.Id)).AsQueryable();
        }

        /// <summary>
        /// Creates a product. This action accepts parameters via an ODataActionParameters object.
        /// </summary>
        /// <param name="parameters">The OData action parameters.</param>
        [HttpPost]
        [ResponseType(typeof(Product))]
        public IHttpActionResult Create(ODataActionParameters parameters)
        {
            var product = new Product
            {
                Id = Data.Values.Max(existingProduct => existingProduct.Id) + 1,
                Name = (string)parameters["name"],
                Price = (double)parameters["price"],
                EnumValue = (MyEnum)parameters["enumValue"]
            };
            Data.TryAdd(product.Id, product);
            return Created(product);
        }

        /// <summary>
        /// Rates a product. This action targets a specific entity by id.
        /// </summary>
        /// <param name="key">The product id.</param>
        /// <param name="parameters">The OData action parameters.</param>
        [HttpPost]
        [ResponseType(typeof(void))]
        public IHttpActionResult Rate([FromODataUri] int key, ODataActionParameters parameters)
        {
            return StatusCode(HttpStatusCode.NoContent);
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