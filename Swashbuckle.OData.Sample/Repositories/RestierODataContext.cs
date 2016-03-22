using System.Data.Entity;
using SwashbuckleODataSample.Models;

namespace SwashbuckleODataSample.Repositories
{
    public class RestierODataContext : DbContext
    {
        static RestierODataContext()
        {
            Database.SetInitializer<RestierODataContext>(null);
        }


        public RestierODataContext() : base("name=RestierODataContext")
        {
            Users = new TestDbSet<User>();
            Seed(this);
        }

        private static void Seed(RestierODataContext context)
        {
            context.Users.Add(new User { Id = 1, Name = "UserOne" });
            context.Users.Add(new User { Id = 2, Name = "UserTwo" });
        }

        public DbSet<User> Users { get; set; }
    }
}