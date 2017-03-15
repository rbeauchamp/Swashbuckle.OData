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
            context.Orders.Add(new Order {
                OrderId = new System.Guid("ce37ae8d-4efe-2d5f-10a0-39ddd2436a52"),
                OrderName = "OrderOne",
                CustomerId = 1,
                UnitPrice = 4.0
            });
            context.Orders.Add(new Order {
                OrderId = new System.Guid("03b20510-a693-9504-b040-39ddd2436a52"),
                OrderName = "OrderTwo",
                CustomerId = 1,
                UnitPrice = 3.5
            });

            var customerTwo = new Customer { Id = 2, Name = "CustomerTwo" };
            context.Customers.Add(customerTwo);
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderOne",
                CustomerId = customerTwo.Id,
                UnitPrice = 4.0
            });
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderTwo",
                CustomerId = customerTwo.Id,
                UnitPrice = 3.5
            });           

            var customerThree = new Customer { Id = 3, Name = "CustomerThree" };
            context.Customers.Add(customerThree);
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderOne",
                CustomerId = customerThree.Id,
                UnitPrice = 4.0
            });
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderTwo",
                CustomerId = customerThree.Id,
                UnitPrice = 3.5
            });

            var customerFour = new Customer { Id = 4, Name = "CustomerFour" };
            context.Customers.Add(customerFour);
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderOne",
                CustomerId = customerFour.Id,
                UnitPrice = 4.0
            });
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderTwo",
                CustomerId = customerFour.Id,
                UnitPrice = 3.5
            });
            

            var customerFive = new Customer { Id = 5, Name = "CustomerFive" };
            context.Customers.Add(customerFive);
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderOne",
                CustomerId = customerFive.Id,
                UnitPrice = 4.0
            });
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderTwo",
                CustomerId = customerFive.Id,
                UnitPrice = 3.5
            });
            

            var customerSix = new Customer { Id = 6, Name = "CustomerSix" };
            context.Customers.Add(customerSix);
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderOne",
                CustomerId = customerSix.Id,
                UnitPrice = 4.0
            });
            context.Orders.Add(new Order {
                OrderId = SequentialGuidGenerator.Generate(SequentialGuidType.SequentialAtEnd),
                OrderName = "OrderTwo",
                CustomerId = customerSix.Id,
                UnitPrice = 3.5
            });
        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Project> Projects { get; set; }
    }
}