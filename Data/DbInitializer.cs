using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using NaturalShop.API.Data;
using NaturalShop.API.Models;
using System;

namespace NaturalShop.API.Data
{
    public static class DbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

                // NOT: Tablolar EF Core migration'ları ile oluşturulur (bkz. Program.cs -> Database.MigrateAsync()).
                // EnsureCreated() migration geçmişiyle çakışacağı için burada kullanılmıyor.

                // Eğer tabloda ürün varsa işlemi durdur (mükerrer kayıt önleme)
                var productCount = context.Products.Count();
                Console.WriteLine($"📊 Mevcut ürün sayısı: {productCount}");
                if (productCount > 0)
                {
                    Console.WriteLine("ℹ️ Ürünler zaten mevcut, seed işlemi atlanıyor.");
                    return;
                }

                Console.WriteLine("🌱 Yeni ürünler ekleniyor...");

                context.Products.AddRange(
                    new Product
                    {
                        Name = "Ege Sızma Zeytinyağı - Erken Hasat (1L)",
                        Description = "0.8 asit oranına sahip, soğuk sıkım tekniğiyle üretilmiş, taze zeytin kokulu şifa kaynağı.",
                        Price = 350,
                        ImageUrl = "/images/zeytinyagi1.jpg",
                        Category = "Zeytinyağı Grubu",
                        Stock = 100,
                        Code = "NAT-ZEY-01",
                        StoryText = "Asırlık zeytin ağaçlarımızdan sabahın ilk ışıklarıyla toplanan zeytinlerin 4 saatte yağa dönüşme hikayesi.",
                        StoryImages = new string[] { "/images/zeytinyagi2.jpg", "/images/zeytinyagi3.jpg", "/images/zeytinyagi4.jpg", "/images/zeytinyagi5.jpg", "/images/zeytinyagi6.jpg", "/images/zeytinyagi7.jpg" }
                    },
                    new Product
                    {
                        Name = "Geleneksel Odun Ateşi Üzüm Pekmezi",
                        Description = "Hiçbir katkı maddesi ve şeker ilavesi içermez. Tamamen doğal yöntemlerle yoğunlaştırılmıştır.",
                        Price = 220,
                        ImageUrl = "/images/pekmez1.jpg",
                        Category = "Pekmez & Bal",
                        Stock = 60,
                        Code = "NAT-PKM-01",
                        StoryText = "Bağ bozumu heyecanıyla toplanan üzümlerin dev bakır kazanlarda ağır ağır kaynama süreci.",
                        StoryImages = new string[] { "/images/pekmez2.jpg", "/images/pekmez3.jpg", "/images/pekmez4.jpg", "/images/pekmez5.jpg", "/images/pekmez6.jpg", "/images/pekmez7.jpg" }
                    },
                    new Product
                    {
                        Name = "Pul Biber",
                        Description = "Güneşte kurutulmuş taze biberlerden, ipek çekim yöntemiyle elde edilen premium lezzet.",
                        Price = 110,
                        ImageUrl = "/images/tozbiber1.jpg",
                        Category = "Baharatlar",
                        Stock = 150,
                        Code = "NAT-BBR-01",
                        StoryText = "Biberlerin tarladan toplanıp sergi alanlarında doğal yollarla kurutulma aşamaları.",
                        StoryImages = new string[] { "/images/tozbiber2.jpg", "/images/tozbiber3.jpg", "/images/tozbiber4.jpg", "/images/tozbiber5.jpg", "/images/tozbiber6.jpg", "/images/tozbiber7.jpg" }
                    },
                    new Product
                    {
                        Name = "Dalaman Dağ İnciri",
                        Description = "Dışı incecik, içi ballı, dalında kurumuş ve hiçbir kimyasal işlem görmemiş en üst kalite kuru incir.",
                        Price = 350,
                        ImageUrl = "/images/incir1.jpg",
                        Category = "Kuru Meyveler",
                        Stock = 40,
                        Code = "NAT-INC-01",
                        StoryText = "Ege'nin yüksek rakımlı köylerinde, rüzgar ve güneşin yardımıyla ballanan incirlerin hikayesi.",
                        StoryImages = new string[] { "/images/incir2.jpg", "/images/incir3.jpg", "/images/incir4.jpg", "/images/incir5.jpg", "/images/incir6.jpg", "/images/incir7.jpg" }
                    }
                );

                context.SaveChanges();
                var savedCount = context.Products.Count();
                Console.WriteLine($"✅ {savedCount} ürün başarıyla veritabanına eklendi.");
            }
        }
    }
}