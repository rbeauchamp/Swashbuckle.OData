using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.Repositories;

namespace SwashbuckleODataSample.ODataControllers
{
    public class CustomersController : ODataController
    {
        private readonly SwashbuckleODataContext _db = new SwashbuckleODataContext();

        /// <summary>
        /// Query customers
        /// </summary>
        [EnableQuery]
        public IQueryable<Customer> GetCustomers()
        {
            return _db.Customers;
        }

        /// <summary>
        /// Query the customer by id
        /// </summary>
        /// <param name="key">The customer id</param>
        [EnableQuery]
        public SingleResult<Customer> GetCustomer([FromODataUri] int key)
        {
            return SingleResult.Create(_db.Customers.Where(customer => customer.Id == key));
        }

        /// <summary>
        /// Replace all data for the customer with the given id
        /// </summary>
        /// <param name="key">Customer id</param>
        /// <param name="patch">Customer details</param>
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Delta<Customer> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _db.Customers.FindAsync(key);
            if (customer == null)
            {
                return NotFound();
            }

            patch.Put(customer);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(customer);
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        /// <param name="customer">Customer details</param>
        [ResponseType(typeof(Customer))]
        public async Task<IHttpActionResult> Post(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return Created(customer);
        }

        /// <summary>
        /// Edit the customer with the given id
        /// </summary>
        /// <param name="key">Customer id</param>
        /// <param name="patch">Customer details</param>
        [ResponseType(typeof(void))]
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Customer> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _db.Customers.FindAsync(key);
            if (customer == null)
            {
                return NotFound();
            }

            patch.Patch(customer);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(customer);
        }

        /// <summary>
        /// Delete the customer with the given id
        /// </summary>
        /// <param name="key">Customer id</param>
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            var customer = await _db.Customers.FindAsync(key);
            if (customer == null)
            {
                return NotFound();
            }

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [EnableQuery]
        public IQueryable<Order> GetOrders([FromODataUri] int key)
        {
            return _db.Customers.Where(m => m.Id == key)
                .SelectMany(m => m.Orders);
        }

        /// <summary>
        /// An OData function example. Gets the total count of customers.
        /// </summary>
        [HttpGet]
        [ResponseType(typeof(int))]
        public IHttpActionResult TotalCount()
        {
            var customerCount = _db.Customers.Count();
            return Ok(customerCount);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CustomerExists(int key)
        {
            return _db.Customers.Count(e => e.Id == key) > 0;
        }
    }
}