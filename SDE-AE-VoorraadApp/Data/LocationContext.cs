using Microsoft.EntityFrameworkCore;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Data
{
    public class LocationContext : DbContext
    {
        public LocationContext (DbContextOptions<LocationContext> options) : base(options)
        {

        }

        // All the objects used to create the LocationContext Database.
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderList> OrderLists { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Location> Locations { get; set; }

        /// <summary>
        /// Function used to create the LocationContext in accordance with the objects listed within.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>().ToTable("Location");
            modelBuilder.Entity<Machine>().ToTable("Machine");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<ProductStock>().ToTable("ProductStock");
            modelBuilder.Entity<OrderList>().ToTable("OrderList");
            modelBuilder.Entity<Order>().ToTable("Order");
        }
    }
}
