using Microsoft.EntityFrameworkCore;
using Product_Management_API.Models;

namespace Product_Management_API.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor that accepts DbContextOptions
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }


        // Define DbSets for your entities
        public DbSet<Product> Products { get; set; }
    }
}
