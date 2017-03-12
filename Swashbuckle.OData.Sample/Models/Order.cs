using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace SwashbuckleODataSample.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        public string OrderName { get; set; }

        public double UnitPrice { get; set; }

       [ForeignKey("Customer")]
        public int CustomerId { get; set; }

       /* [ForeignKey("CustomerId")]
        [ActionOnDelete(EdmOnDeleteAction.Cascade)]*/
       public virtual Customer Customer { get; }
    }
}