using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwashbuckleODataSample.Models
{
    /// <summary>
    /// Customer comment
    /// </summary>
    public class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
        }
        /// <summary>
        /// Customer Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// customer Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Customer Orders
        /// </summary>
        public virtual ICollection<Order> Orders { get; }
    }
}