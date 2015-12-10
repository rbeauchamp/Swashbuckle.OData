using System.Data.Entity;

namespace SwashbuckleODataSample.Models
{
    public class TestRestierODataContext : DbContext
    {
        static TestRestierODataContext()
        {
            Database.SetInitializer<TestRestierODataContext>(null);
        }


        public TestRestierODataContext() : base("name=TestRestierODataContext")
        {
        }

        public DbSet<User> Users { get; set; }
    }
}