using Microsoft.EntityFrameworkCore;
using NaturalShop.API.Models;

namespace NaturalShop.API.Data
{
    public static class SeedData
    {
        private static string GetImageUrl(string imagePath)
        {
            // Backend URL'i - Development için localhost:5072
            var baseUrl = "http://localhost:5072";
            return $"{baseUrl}{imagePath}";
        }

        public static async Task InitializeAsync(AppDbContext context, CancellationToken cancellationToken = default)
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);

            if (await context.Products.AnyAsync(cancellationToken))
            {
                return;
            }

            var sampleProducts = new List<Product>
            {
                new Product
                {
                    Name = "Soğuk Sıkım Sızma Zeytinyağı",
                    Code = "NS-001",
                    Description = "Ege bölgesinde organik tarım ile yetiştirilen zeytinlerden soğuk sıkım yöntemi ile elde edilir. Aroması güçlü ve tamamen katkısızdır. 500ml",
                    Category = "Yağlar",
                    Price = 320m,
                    ImageUrl = GetImageUrl("/images/zeytinyağı.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/zeytinyağı.png"),
                        GetImageUrl("/images/organic-olive-oil.jpg")
                    },
                    Stock = 45,
                    StoryText = "Ege'nin verimli topraklarında yetişen zeytin ağaçlarından, geleneksel yöntemlerle toplanan meyveler soğuk sıkım tekniğiyle işlenir. Bu yöntem zeytinin tüm besin değerlerini koruyarak, en saf halini sunar. Organik tarım sertifikalı bu zeytinyağı, sofralarınızın vazgeçilmez şifa kaynağıdır.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/zeytinyağı.png"),
                        GetImageUrl("/images/organic-olive-oil.jpg")
                    }
                },
                new Product
                {
                    Name = "Çam Balı",
                    Code = "NS-002",
                    Description = "Muğla yöresindeki çam ormanlarından toplanan doğal çam balı. Tamamen katkısız ve organik üretim ile elde edilir. 1kg",
                    Category = "Bal & Pekmez",
                    Price = 450m,
                    ImageUrl = GetImageUrl("/images/çam_balı.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/çam_balı.png")
                    },
                    Stock = 32,
                    StoryText = "Muğla'nın zengin çam ormanlarında, arılarımız doğanın en saf nektarını toplar. Geleneksel kovanlardan elde edilen bu bal, hiçbir işleme tabi tutulmadan sofralarınıza gelir. Çam balı, şifa kaynağı özellikleriyle binlerce yıldır Anadolu'da tüketilmektedir.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/çam_balı.png")
                    }
                },
                new Product
                {
                    Name = "Siyah İncir",
                    Code = "NS-003",
                    Description = "Dalaman yöresinden toplanan taze siyah incir. Tamamen doğal ve organik tarım ile yetiştirilir. Hiçbir katkı maddesi içermez. 1kg",
                    Category = "Taze Meyveler",
                    Price = 120m,
                    ImageUrl = GetImageUrl("/images/siyah_incir.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/siyah_incir.png")
                    },
                    Stock = 75,
                    StoryText = "Dalaman'ın bereketli topraklarında yetişen siyah incirler, tam olgunlaştıktan sonra özenle toplanır. Hiçbir kimyasal işlem görmeden, doğal haliyle sofralarınıza gelir. Bu taze siyah incirler, vitamin ve mineral açısından zengin bir şifa kaynağıdır. Her tanesi, doğanın en saf halidir.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/siyah_incir.png")
                    }
                },
                new Product
                {
                    Name = "Köy Tulum Peyniri",
                    Code = "NS-006",
                    Description = "Köydeki tecrübeli ustaların elinde, geleneksel yöntemlerle hazırlanan tulum peyniri. Tamamen katkısız ve doğal üretim. 500gr",
                    Category = "Süt Ürünleri",
                    Price = 280m,
                    ImageUrl = GetImageUrl("/images/peynir.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/peynir.png")
                    },
                    Stock = 42,
                    StoryText = "Köyün deneyimli peynir ustaları, dedelerinden kalan geleneksel yöntemlerle bu peyniri hazırlar. Doğal koyun sütünden, hiçbir katkı maddesi kullanılmadan üretilen tulum peyniri, özenle olgunlaştırılır. Her lokması, doğanın ve el emeğinin birleşimidir.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/peynir.png")
                    }
                },
                new Product
                {
                    Name = "Üzüm Pekmezi",
                    Code = "NS-008",
                    Description = "Ege bağlarından toplanan organik üzümlerden, geleneksel yöntemlerle kaynatılarak hazırlanır. Tamamen katkısızdır. 500ml",
                    Category = "Bal & Pekmez",
                    Price = 195m,
                    ImageUrl = GetImageUrl("/images/pekmez.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/pekmez.png")
                    },
                    Stock = 65,
                    StoryText = "Ege'nin güneşli bağlarından toplanan üzümler, köydeki kadınların elinde geleneksel yöntemlerle pekmeze dönüşür. Hiçbir şeker veya katkı maddesi eklenmeden, sadece üzümün doğal şekeriyle kaynatılarak hazırlanır. Demir açısından zengin bu pekmez, doğal bir şifa kaynağıdır.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/pekmez.png")
                    }
                },
                new Product
                {
                    Name = "Kuru Domates",
                    Code = "NS-009",
                    Description = "Güneşte doğal yöntemlerle kurutulmuş domates. Hiçbir koruyucu madde kullanılmadan hazırlanır. 250gr",
                    Category = "Kurutulmuş Sebzeler",
                    Price = 145m,
                    ImageUrl = GetImageUrl("/images/kuru_domates.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/kuru_domates.png")
                    },
                    Stock = 85,
                    StoryText = "Akdeniz ikliminin güneşli günlerinde, özenle seçilmiş domatesler güneş altında doğal yöntemlerle kurutulur. Hiçbir kimyasal işlem görmeden, sadece tuz ve güneş ile hazırlanan bu kuru domatesler, Ege mutfağının vazgeçilmez lezzetidir. Tamamen doğal üretim ile elde edilir.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/kuru_domates.png")
                    }
                },
                new Product
                {
                    Name = "Adaçayı",
                    Code = "NS-011",
                    Description = "Toroslar'ın yüksek yaylalarından toplanan adaçayı yaprakları, doğal yöntemlerle kurutulur. Şifa kaynağı özellikleriyle bilinir. 40gr",
                    Category = "Şifalı Bitkiler",
                    Price = 110m,
                    ImageUrl = GetImageUrl("/images/adaçayı.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/adaçayı.png")
                    },
                    Stock = 105,
                    StoryText = "Toroslar'ın temiz havasında yetişen adaçayı, geleneksel yöntemlerle toplanıp doğal güneş altında kurutulur. Binlerce yıldır şifa kaynağı olarak kullanılan bu bitki, organik tarım yöntemleriyle yetiştirilir. Hiçbir kimyasal işlem görmeden sofralarınıza gelir.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/adaçayı.png")
                    }
                },
                new Product
                {
                    Name = "Köy Yumurtası",
                    Code = "NS-012",
                    Description = "Doğal ortamda yetiştirilen tavuklardan elde edilen köy yumurtası. Organik tarım sertifikalı ve tamamen doğal beslenme ile üretilir. 30 adet",
                    Category = "Süt Ürünleri",
                    Price = 180m,
                    ImageUrl = GetImageUrl("/images/yumurta.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/yumurta.png")
                    },
                    Stock = 38,
                    StoryText = "Köydeki küçük çiftliklerde, doğal ortamda yetiştirilen tavuklar, organik yemlerle beslenir. Bu tavuklardan elde edilen yumurtalar, fabrika üretiminden tamamen farklıdır. Her yumurta, doğanın ve özenli bakımın bir meyvesidir. Organik tarım sertifikalı bu yumurtalar, sağlıklı beslenmenin en doğal hali.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/yumurta.png")
                    }
                },
                new Product
                {
                    Name = "Siyah Zeytin",
                    Code = "NS-013",
                    Description = "Ege bölgesinden toplanan taze yeşil zeytinler, geleneksel yöntemlerle salamura edilir. Tamamen doğal ve katkısız. 1kg",
                    Category = "Konserveler",
                    Price = 150m,
                    ImageUrl = GetImageUrl("/images/zeytin.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/zeytin.png")
                    },
                    Stock = 60,
                    StoryText = "Ege'nin zeytin bahçelerinden özenle toplanan yeşil zeytinler, geleneksel salamura yöntemiyle hazırlanır. Hiçbir kimyasal koruyucu kullanılmadan, sadece tuz ve su ile işlenen bu zeytinler, doğal lezzetiyle sofralarınızı zenginleştirir. Her tanesi, binlerce yıllık geleneğin bir yansımasıdır.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/zeytin.png")
                    }
                },
                new Product
                {
                    Name = "Kuru Biber",
                    Code = "NS-014",
                    Description = "Güneşte doğal yöntemlerle kurutulmuş kırmızı biber. Hiçbir koruyucu madde kullanılmadan hazırlanır. 250gr",
                    Category = "Kurutulmuş Sebzeler",
                    Price = 120m,
                    ImageUrl = GetImageUrl("/images/kuru_biber.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/kuru_biber.png")
                    },
                    Stock = 50,
                    StoryText = "Akdeniz'in güneşli günlerinde, özenle seçilmiş kırmızı biberler güneş altında doğal yöntemlerle kurutulur. Hiçbir kimyasal işlem görmeden, sadece güneşin doğal enerjisiyle hazırlanan bu kuru biberler, Ege mutfağının vazgeçilmez lezzetidir. Tamamen doğal üretim ile elde edilir.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/kuru_biber.png")
                    }
                },
                new Product
                {
                    Name = "Domates Salçası",
                    Code = "NS-015",
                    Description = "Güneşte olgunlaşmış domateslerden geleneksel yöntemlerle hazırlanan doğal domates salçası. Hiçbir katkı maddesi içermez. 500gr",
                    Category = "Konserveler",
                    Price = 95m,
                    ImageUrl = GetImageUrl("/images/salça.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/salça.png")
                    },
                    Stock = 70,
                    StoryText = "Ege'nin güneşli bahçelerinde yetişen domatesler, tam olgunlaştıktan sonra geleneksel yöntemlerle salçaya dönüşür. Hiçbir koruyucu veya katkı maddesi kullanılmadan, sadece domatesin doğal lezzetiyle hazırlanan bu salça, mutfağınızın vazgeçilmez lezzetidir. Her kaşığında doğanın bereketi vardır.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/salça.png")
                    }
                },
                new Product
                {
                    Name = "Toz Biber",
                    Code = "NS-016",
                    Description = "Geleneksel taş değirmende öğütülmüş kırmızı biber tozu. Hiçbir katkı maddesi içermez. 200gr",
                    Category = "Baharatlar",
                    Price = 85m,
                    ImageUrl = GetImageUrl("/images/toz_biber.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/toz_biber.png")
                    },
                    Stock = 65,
                    StoryText = "Güneşte kurutulmuş kırmızı biberler, geleneksel taş değirmenlerde özenle öğütülür. Hiçbir kimyasal işlem görmeden, sadece doğal yöntemlerle hazırlanan bu toz biber, Ege mutfağının karakteristik lezzetini verir. Her tutamında, binlerce yıllık geleneğin tadı vardır.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/toz_biber.png")
                    }
                },
                new Product
                {
                    Name = "Kuru İncir",
                    Code = "NS-017",
                    Description = "Dalaman yöresinden, güneşte doğal yöntemlerle kurutulmuş premium kuru incir. Hiçbir katkı maddesi içermez. 500gr",
                    Category = "Kurutulmuş Meyveler",
                    Price = 175m,
                    ImageUrl = GetImageUrl("/images/kuru_incir.png"),
                    Images = new string[] 
                    {
                        GetImageUrl("/images/kuru_incir.png")
                    },
                    Stock = 80,
                    StoryText = "Dalaman'ın bereketli topraklarında yetişen incirler, yüzyıllardır süren geleneksel yöntemlerle güneş altında kurutulur. Hiçbir kimyasal işlem görmeden, sadece doğanın gücüyle hazırlanan bu kuru incirler, vitamin ve mineral açısından zengin bir şifa kaynağıdır. Her tanesi, doğanın ve geleneğin birleşimidir.",
                    StoryImages = new string[]
                    {
                        GetImageUrl("/images/kuru_incir.png")
                    }
                }
            };

            await context.Products.AddRangeAsync(sampleProducts, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}


