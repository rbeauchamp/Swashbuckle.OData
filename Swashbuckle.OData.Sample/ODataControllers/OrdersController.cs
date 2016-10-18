using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.Repositories;
using SwashbuckleODataSample.Utils;

namespace SwashbuckleODataSample.ODataControllers
{
    public class OrdersController : ODataController
    {
        private readonly SwashbuckleODataContext _db = new SwashbuckleODataContext();

        /// <summary>
        /// Query orders
        /// </summary>
        [EnableQuery]
        public IQueryable<Order> GetOrders()
        {
            return _db.Orders;
        }

        /// <summary>
        /// An example of a custom route. Create a new order for the customer with the given id
        /// </summary>
        /// <param name="customerId">The customer id</param>
        /// <param name="order">Order details</param>
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> Post([FromODataUri] int customerId, Order order)
        {
            order.OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd);
            order.CustomerId = customerId;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return Created(order);
        }

        /// <summary>
        /// Query the order by id
        /// </summary>
        /// <param name="key">The order id</param>
        [EnableQuery]
        public SingleResult<Order> GetOrder([FromODataUri] Guid key)
        {
            return SingleResult.Create(_db.Orders.Where(order => order.OrderId == key));
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="order">Order details</param>
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> Post(Order order)
        {
            order.OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return Created(order);
        }

        /// <summary>
        /// Edit the order with the given id
        /// </summary>
        /// <param name="key">Order id</param>
        /// <param name="patch">Order details</param>
        [ResponseType(typeof(void))]
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] Guid key, Delta<Order> patch)
        {
            Validate(patch.GetInstance());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _db.Orders.FindAsync(key);
            if (order == null)
            {
                return NotFound();
            }

            patch.Patch(order);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(order);
        }

        /// <summary>
        /// Delete the order with the given id
        /// </summary>
        /// <param name="key">Order id</param>
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Delete([FromODataUri] Guid key)
        {
            var order = await _db.Orders.FindAsync(key);
            if (order == null)
            {
                return NotFound();
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [EnableQuery]
        public SingleResult<Customer> GetCustomer([FromODataUri] Guid key)
        {
            return SingleResult.Create(_db.Orders.Where(m => m.OrderId == key)
                .Select(m => m.Customer));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(Guid key)
        {
            return _db.Orders.Count(e => e.OrderId == key) > 0;
        }
    }
}