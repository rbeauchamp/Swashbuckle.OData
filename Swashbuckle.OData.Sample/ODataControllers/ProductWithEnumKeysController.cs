using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using SwashbuckleODataSample.Models;

namespace SwashbuckleODataSample.ODataControllers
{
    public class ProductWithEnumKeysController : ODataController
    {
        private static readonly Dictionary<MyEnum, 
                                    ProductWithEnumKey> DataEnumAsKey;

        static ProductWithEnumKeysController()
        {
            DataEnumAsKey = new Dictionary<MyEnum, ProductWithEnumKey>()
            {
                {
                    MyEnum.ValueOne,
                    new ProductWithEnumKey {
                        EnumValue = MyEnum.ValueOne,
                        Name = "ValueOneName",
                        Price = 101
                    }
                },
                {
                    MyEnum.ValueTwo,
                    new ProductWithEnumKey
                    {
                        EnumValue = MyEnum.ValueTwo,
                        Name = "ValueTwoName",
                        Price = 102
                    }
                }
            };
        }

        /// <summary>
        /// Query products
        /// </summary>
        [HttpGet]
        [EnableQuery]
        public IQueryable<ProductWithEnumKey> Get()
        {
            return DataEnumAsKey.Values.AsQueryable();
        }

        /// <summary>
        /// Query product by enum key
        /// </summary>
        /// <param name="Key">key enum value</param>
        /// <returns>project enum model</returns>
        [HttpGet]
        public IHttpActionResult Get([FromODataUri]MyEnum Key)
        {
            return Ok(DataEnumAsKey[Key]);
        }
    }
}