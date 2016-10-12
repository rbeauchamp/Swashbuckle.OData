using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using SwashbuckleODataSample.Models;
using System;

namespace SwashbuckleODataSample.ODataControllers
{
    public class ProductWithCompositeEnumIntKeysController : ODataController
    {
        private static readonly Dictionary<Tuple<MyEnum, int>, 
                                    ProductWithCompositeEnumIntKey> DataCompositeKey;

        static ProductWithCompositeEnumIntKeysController()
        {
            DataCompositeKey = new Dictionary<Tuple<MyEnum, int>, 
                                    ProductWithCompositeEnumIntKey>()
            {
                {
                    Tuple.Create(MyEnum.ValueOne, 1),
                    new ProductWithCompositeEnumIntKey
                    {
                        EnumValue = MyEnum.ValueOne,
                        Id = 1,
                        Name = "ValueOneName",
                        Price = 101
                    }
                },
                {
                    Tuple.Create(MyEnum.ValueTwo, 2),
                    new ProductWithCompositeEnumIntKey
                    {
                        EnumValue = MyEnum.ValueTwo,
                        Id = 2,
                        Name = "ValueTwoName",
                        Price = 102
                    }
                }
            };
        }

        /// <summary>
        /// Query products
        /// </summary>
        [EnableQuery]
        public IQueryable<ProductWithCompositeEnumIntKey> Get()
        {
            return DataCompositeKey.Values.AsQueryable();
        }

        /// <summary>
        /// Query products by keys
        /// </summary>
        /// <param name="keyenumValue">key enum value</param>
        /// <param name="keyid">key id</param>
        /// <returns>composite enum-int key model</returns>
        [EnableQuery]
        public IHttpActionResult Get([FromODataUri]MyEnum keyenumValue, [FromODataUri]int keyid)
        {
            return Ok(DataCompositeKey[Tuple.Create(keyenumValue, keyid)]);
        }
    }
}