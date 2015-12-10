using System;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.Repositories;

namespace SwashbuckleODataSample.ODataControllers
{
    public class OrdersV1Controller : ODataController
    {
        private readonly SwashbuckleODataContext _db = new SwashbuckleODataContext();

        [EnableQuery]
        public IQueryable<Order> GetOrders()
        {
            return _db.Orders;
        }

        [EnableQuery]
        public SingleResult<Order> GetOrder([FromODataUri] Guid key)
        {
            return SingleResult.Create(_db.Orders.Where(order => order.OrderId == key));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}