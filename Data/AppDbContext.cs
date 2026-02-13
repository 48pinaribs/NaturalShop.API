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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Supabase/PostgreSQL için en saðlýklý eþleþtirme:
			// Tablo adýný tam olarak senin söylediðin gibi "Products" yapýyoruz.
			// "public" þemasýný eklemek, baðlantý hýzýný ve doðruluðunu artýrýr.
			modelBuilder.Entity<Product>().ToTable("Products", "public");

			modelBuilder.Entity<Order>().ToTable("Orders", "public");
			modelBuilder.Entity<OrderItem>().ToTable("OrderItems", "public");
			modelBuilder.Entity<VerificationCode>().ToTable("VerificationCodes", "public");
		}
	}
}