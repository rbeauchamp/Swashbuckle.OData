using System.Data.Entity;
using SwashbuckleODataSample.ApiControllers;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.Utils;

namespace SwashbuckleODataSample.Repositories
{
    public class SwashbuckleODataContext : DbContext
    {
        static SwashbuckleODataContext()
        {
            Database.SetInitializer<SwashbuckleODataContext>(null);
        }


        public SwashbuckleODataContext() : base("name=SwashbuckleODataContext")
        {
            Customers = new TestDbSet<Customer>();
            Orders = new TestDbSet<Order>();
            Clients = new TestDbSet<Client>();
            Projects = new TestDbSet<Project>();

            Seed(this);
        }

        private static void Seed(SwashbuckleODataContext context)
        {
            var clientOne = new Client { Id = 1, Name = "ClientOne" };
            context.Clients.Add(clientOne);
            context.Clients.Add(new Client { Id = 2, Name = "ClientTwo" });

            context.Projects.Add(new Project { ProjectId = 1, ProjectName = "ProjectOne", Client = clientOne });
            context.Projects.Add(new Project { ProjectId = 2, ProjectName = "ProjectTwo", Client = clientOne });

            var customerOne = new Customer { Id = 1, Name = "CustomerOne" };
            context.Customers.Add(customerOne);
            context.Customers.Add(new Customer { Id = 2, Name = "CustomerTwo" });

            context.Orders.Add(new Order { OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd), OrderName = "OrderOne", Customer = customerOne, UnitPrice = 4.0 });
            context.Orders.Add(new Order { OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd), OrderName = "OrderTwo", Customer = customerOne, UnitPrice = 3.5 });
        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Project> Projects { get; set; }
    }
}