using System.Data.Entity;

namespace SwashbuckleODataSample.Models
{
    public class RestierODataContext : DbContext
    {
        static RestierODataContext()
        {
            Database.SetInitializer(new RestierODataInitializer());
        }


        public RestierODataContext() : base("name=RestierODataContext")
        {
        }

        public DbSet<User> Users { get; set; }
    }

    public class RestierODataInitializer : DropCreateDatabaseAlways<RestierODataContext>
    {
        protected override void Seed(RestierODataContext context)
        {
            context.Users.Add(new User { Name = "UserOne" });
            context.Users.Add(new User { Name = "UserTwo" });

            base.Seed(context);
        }
    }
}