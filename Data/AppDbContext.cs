using Microsoft.EntityFrameworkCore;
using NaturalShop.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace NaturalShop.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
    
}
}


