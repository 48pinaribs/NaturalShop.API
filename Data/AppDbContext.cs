using Microsoft.EntityFrameworkCore;
using NaturalShop.API.Models;

namespace NaturalShop.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}


