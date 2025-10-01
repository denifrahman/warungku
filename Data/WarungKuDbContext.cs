using Microsoft.EntityFrameworkCore;
using WarungKu.Models;

namespace WarungKu.Data
{
    public class WarungKuDbContext : DbContext
    {
        public WarungKuDbContext(DbContextOptions<WarungKuDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
