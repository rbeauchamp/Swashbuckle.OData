using System.Data.Entity;

namespace SwashbuckleODataSample.Models
{
    public class SwashbuckleODataContext : DbContext
    {
        static SwashbuckleODataContext()
        {
            Database.SetInitializer(new SwashbuckleODataInitializer());
        }


        public SwashbuckleODataContext() : base("name=SwashbuckleODataContext")
        {
        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Project> Projects { get; set; }
    }

    public class SwashbuckleODataInitializer : DropCreateDatabaseAlways<SwashbuckleODataContext>
    {
        protected override void Seed(SwashbuckleODataContext context)
        {
            var clientOne = new Client { Name = "ClientOne" };
            context.Clients.Add(clientOne);
            context.Clients.Add(new Client { Name = "ClientTwo" });

            context.Projects.Add(new Project { ProjectName = "ProjectOne", Client = clientOne});
            context.Projects.Add(new Project { ProjectName = "ProjectTwo", Client = clientOne});

            var customerOne = new Customer { Name = "CustomerOne" };
            context.Customers.Add(customerOne);
            context.Customers.Add(new Customer { Name = "CustomerTwo" });

            context.Orders.Add(new Order { OrderName = "OrderOne", Customer = customerOne });
            context.Orders.Add(new Order { OrderName = "OrderTwo", Customer = customerOne });

            base.Seed(context);
        }
    }
}