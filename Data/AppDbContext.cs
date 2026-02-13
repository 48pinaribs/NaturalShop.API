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

		// --- BU KISMI EKLEDÝM: TABLO ÝSÝMLERÝNÝ SABÝTLÝYORUZ ---
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Identity (Giriþ/Kayýt) tablolarý için bu satýrý asla silme
			base.OnModelCreating(modelBuilder);

			// Eðer Supabase'de tablo adýn "Product" (tekil) ise bu satýr 32 saniyelik hatayý çözer
			modelBuilder.Entity<Product>().ToTable("Product");

			// Diðer tablolarýný da garantiye alalým
			modelBuilder.Entity<Order>().ToTable("Order");
			modelBuilder.Entity<OrderItem>().ToTable("OrderItem");
			modelBuilder.Entity<VerificationCode>().ToTable("VerificationCode");
		}
	}
}