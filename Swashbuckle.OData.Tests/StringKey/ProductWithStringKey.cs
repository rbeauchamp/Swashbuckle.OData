using System.ComponentModel.DataAnnotations;

namespace SwashbuckleODataSample.Models
{
    public class ProductWithStringKey
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }
}