using System.Linq;
using System.Web.Http;
using System.Web.OData;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.Repositories;

namespace SwashbuckleODataSample.ODataControllers
{
    public class CustomersV1Controller : ODataController
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