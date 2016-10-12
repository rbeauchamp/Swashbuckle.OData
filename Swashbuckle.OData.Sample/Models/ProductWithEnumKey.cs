using System.ComponentModel.DataAnnotations;

namespace SwashbuckleODataSample.Models
{
    public class ProductWithEnumKey
    {
        [Key]
        public MyEnum EnumValue { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class ProductWithCompositeEnumIntKey
    {
        [Key]
        public MyEnum EnumValue { get; set; }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }
}