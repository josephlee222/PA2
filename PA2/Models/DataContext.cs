using System.Data.Entity;

namespace PA2.Models
{
    public class DataContext : DbContext
    {
        public DataContext(): base("conn") { }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }
}