using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using SwashbuckleODataSample.Models;

namespace SwashbuckleODataSample.Controllers
{
    public class CustomersController : ODataController
    {
        private readonly SwashbuckleODataContext _db = new SwashbuckleODataContext();

        [EnableQuery]
        public IQueryable<Customer> GetCustomers()
        {
            return _db.Customers;
        }

        [EnableQuery]
        public SingleResult<Customer> GetCustomer([FromODataUri] int key)
        {
            return SingleResult.Create(_db.Customers.Where(customer => customer.Id == key));
        }

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