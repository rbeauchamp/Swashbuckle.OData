using System.Data.Entity;

namespace NorthwindAPI.Models
{
    public class NorthwindContext : DbContext
    {
        static NorthwindContext()
        {
            Database.SetInitializer<NorthwindContext>(null);
        }

        public NorthwindContext() : base("name=NorthwindContext")
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CustomerDemographic> CustomerDemographics { get; set; }
        public virtual DbSet<NorthwindCustomer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<NorthwindOrder> Orders { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Shipper> Shippers { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Territory> Territories { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerDemographic>().Property(e => e.CustomerTypeId).IsFixedLength();

            modelBuilder.Entity<CustomerDemographic>().HasMany(e => e.Customers).WithMany(e => e.CustomerDemographics).Map(m => m.ToTable("CustomerCustomerDemo").MapLeftKey("CustomerTypeID").MapRightKey("CustomerID"));

            modelBuilder.Entity<NorthwindCustomer>().Property(e => e.CustomerId).IsFixedLength();

            modelBuilder.Entity<Employee>().HasMany(e => e.Employees1).WithOptional(e => e.Employee1).HasForeignKey(e => e.ReportsTo);

            modelBuilder.Entity<Employee>().HasMany(e => e.Territories).WithMany(e => e.Employees).Map(m => m.ToTable("EmployeeTerritories").MapLeftKey("EmployeeID").MapRightKey("TerritoryID"));

            modelBuilder.Entity<OrderDetail>().Property(e => e.UnitPrice).HasPrecision(19, 4);

            modelBuilder.Entity<NorthwindOrder>().Property(e => e.CustomerId).IsFixedLength();

            modelBuilder.Entity<NorthwindOrder>().Property(e => e.Freight).HasPrecision(19, 4);

            modelBuilder.Entity<NorthwindOrder>().HasMany(e => e.OrderDetails).WithRequired(e => e.NorthwindOrder).WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>().Property(e => e.UnitPrice).HasPrecision(19, 4);

            modelBuilder.Entity<Product>().HasMany(e => e.OrderDetails).WithRequired(e => e.Product).WillCascadeOnDelete(false);

            modelBuilder.Entity<Region>().Property(e => e.RegionDescription).IsFixedLength();

            modelBuilder.Entity<Region>().HasMany(e => e.Territories).WithRequired(e => e.Region).WillCascadeOnDelete(false);

            modelBuilder.Entity<Shipper>().HasMany(e => e.Orders).WithOptional(e => e.Shipper).HasForeignKey(e => e.ShipVia);

            modelBuilder.Entity<Territory>().Property(e => e.TerritoryDescription).IsFixedLength();
        }
    }
}