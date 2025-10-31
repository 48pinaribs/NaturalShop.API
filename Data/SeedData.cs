using Microsoft.EntityFrameworkCore;
using NaturalShop.API.Models;

namespace NaturalShop.API.Data
{
    public static class SeedData
    {
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
                    ImageUrl = "https://images.unsplash.com/photo-1601004890684-d8cbf643f5f2?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1601004890684-d8cbf643f5f2?w=800&q=80",
                        "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=800&q=80", 
                        "https://images.unsplash.com/photo-1508624217470-5ef0f947d8be?w=800&q=80"
                    },
                    Stock = 45,
                    StoryText = "Ege'nin verimli topraklarında yetişen zeytin ağaçlarından, geleneksel yöntemlerle toplanan meyveler soğuk sıkım tekniğiyle işlenir. Bu yöntem zeytinin tüm besin değerlerini koruyarak, en saf halini sunar. Organik tarım sertifikalı bu zeytinyağı, sofralarınızın vazgeçilmez şifa kaynağıdır.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1601004890684-d8cbf643f5f2?w=800&q=80",
                        "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=800&q=80",
                        "https://images.unsplash.com/photo-1508624217470-5ef0f947d8be?w=800&q=80",
                        "https://images.unsplash.com/photo-1599599810769-bcde5a160d32?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Çam Balı",
                    Code = "NS-002",
                    Description = "Muğla yöresindeki çam ormanlarından toplanan doğal çam balı. Tamamen katkısız ve organik üretim ile elde edilir. 1kg",
                    Category = "Bal & Pekmez",
                    Price = 450m,
                    ImageUrl = "https://images.unsplash.com/photo-1558642452-9d2a7deb7f62?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1558642452-9d2a7deb7f62?w=800&q=80",
                        "https://images.unsplash.com/photo-1587049352846-4a222e784d38?w=800&q=80",
                        "https://images.unsplash.com/photo-1599599810769-bcde5a160d32?w=800&q=80"
                    },
                    Stock = 32,
                    StoryText = "Muğla'nın zengin çam ormanlarında, arılarımız doğanın en saf nektarını toplar. Geleneksel kovanlardan elde edilen bu bal, hiçbir işleme tabi tutulmadan sofralarınıza gelir. Çam balı, şifa kaynağı özellikleriyle binlerce yıldır Anadolu'da tüketilmektedir.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1558642452-9d2a7deb7f62?w=800&q=80",
                        "https://images.unsplash.com/photo-1587049352846-4a222e784d38?w=800&q=80",
                        "https://images.unsplash.com/photo-1599599810769-bcde5a160d32?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Kuru İncir",
                    Code = "NS-003",
                    Description = "Aydın yöresinden, güneşte doğal yöntemlerle kurutulmuş premium incir. Hiçbir katkı maddesi içermez. 500gr",
                    Category = "Kurutulmuş Meyveler",
                    Price = 180m,
                    ImageUrl = "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=800&q=80",
                        "https://images.unsplash.com/photo-1596662951482-0c4ba4ad40fd?w=800&q=80",
                        "https://images.unsplash.com/photo-1604335393987-0a89e5b5e0b9?w=800&q=80"
                    },
                    Stock = 75,
                    StoryText = "Aydın'ın bereketli topraklarında yetişen incirler, yüzyıllardır süren geleneksel yöntemlerle güneş altında kurutulur. Hiçbir kimyasal işlem görmeden, sadece doğanın gücüyle hazırlanan bu kuru incirler, vitamin ve mineral açısından zengin bir şifa kaynağıdır.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=800&q=80",
                        "https://images.unsplash.com/photo-1596662951482-0c4ba4ad40fd?w=800&q=80",
                        "https://images.unsplash.com/photo-1604335393987-0a89e5b5e0b9?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "El Yapımı Zeytinyağı Sabunu",
                    Code = "NS-004",
                    Description = "Soğuk sıkım zeytinyağından el yapımı doğal sabun. Doğal lavanta ve kekik ile zenginleştirilmiştir. 100gr",
                    Category = "Kişisel Bakım",
                    Price = 95m,
                    ImageUrl = "https://images.unsplash.com/photo-1608248543803-ba4f8c70ae0b?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1608248543803-ba4f8c70ae0b?w=800&q=80",
                        "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=800&q=80",
                        "https://images.unsplash.com/photo-1571875257727-256c39da42af?w=800&q=80"
                    },
                    Stock = 120,
                    StoryText = "Bu sabunun hikayesi, Ege'nin en kaliteli zeytinlerinden başlar. Soğuk sıkım zeytinyağı, doğal lavanta ve kekik ile buluşarak el emeği ile şekillenir. Hiçbir kimyasal katkı maddesi kullanılmadan, geleneksel yöntemlerle hazırlanan bu sabun, cildiniz için doğal bir şifa kaynağıdır.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1608248543803-ba4f8c70ae0b?w=800&q=80",
                        "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=800&q=80",
                        "https://images.unsplash.com/photo-1571875257727-256c39da42af?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Lavanta Esansiyel Yağı",
                    Code = "NS-005",
                    Description = "Ege bölgesindeki lavanta tarlalarından toplanan çiçeklerin damıtılmasıyla elde edilen saf esansiyel yağ. 10ml",
                    Category = "Şifalı Bitkiler",
                    Price = 165m,
                    ImageUrl = "https://images.unsplash.com/photo-1607345366928-199ea26cfe3e?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1607345366928-199ea26cfe3e?w=800&q=80",
                        "https://images.unsplash.com/photo-1609743522653-52354461eb27?w=800&q=80",
                        "https://images.unsplash.com/photo-1509937528035-ad76254b0356?w=800&q=80"
                    },
                    Stock = 58,
                    StoryText = "Ege'nin mor lavanta tarlalarından, en kaliteli çiçekler seçilerek geleneksel damıtma yöntemiyle elde edilir. Bu saf esansiyel yağ, rahatlatıcı ve sakinleştirici özellikleriyle binlerce yıldır kullanılan bir şifa kaynağıdır. Tamamen doğal üretim ile elde edilir.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1607345366928-199ea26cfe3e?w=800&q=80",
                        "https://images.unsplash.com/photo-1609743522653-52354461eb27?w=800&q=80",
                        "https://images.unsplash.com/photo-1509937528035-ad76254b0356?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Köy Tulum Peyniri",
                    Code = "NS-006",
                    Description = "Köydeki tecrübeli ustaların elinde, geleneksel yöntemlerle hazırlanan tulum peyniri. Tamamen katkısız ve doğal üretim. 500gr",
                    Category = "Süt Ürünleri",
                    Price = 280m,
                    ImageUrl = "https://images.unsplash.com/photo-1486297678162-eb2a19b0a32d?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1486297678162-eb2a19b0a32d?w=800&q=80",
                        "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=800&q=80",
                        "https://images.unsplash.com/photo-1587330979470-3585ac7d99e8?w=800&q=80"
                    },
                    Stock = 42,
                    StoryText = "Köyün deneyimli peynir ustaları, dedelerinden kalan geleneksel yöntemlerle bu peyniri hazırlar. Doğal koyun sütünden, hiçbir katkı maddesi kullanılmadan üretilen tulum peyniri, özenle olgunlaştırılır. Her lokması, doğanın ve el emeğinin birleşimidir.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1486297678162-eb2a19b0a32d?w=800&q=80",
                        "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=800&q=80",
                        "https://images.unsplash.com/photo-1587330979470-3585ac7d99e8?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Kekik Çayı",
                    Code = "NS-007",
                    Description = "Dağ kekiğinden toplanan yapraklar, geleneksel yöntemlerle kurutularak hazırlanır. Şifa kaynağı özellikleriyle bilinir. 50gr",
                    Category = "Doğal İçecekler",
                    Price = 125m,
                    ImageUrl = "https://images.unsplash.com/photo-1597481499755-32356fd68e7d?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1597481499755-32356fd68e7d?w=800&q=80",
                        "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?w=800&q=80",
                        "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=800&q=80"
                    },
                    Stock = 90,
                    StoryText = "Toroslar'ın yüksek yaylalarından toplanan kekik yaprakları, doğal güneş altında kurutulur. Geleneksel yöntemlerle hazırlanan bu çay, solunum yolu rahatsızlıklarına karşı şifa kaynağı olarak yüzyıllardır kullanılmaktadır. Tamamen doğal ve katkısızdır.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1597481499755-32356fd68e7d?w=800&q=80",
                        "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?w=800&q=80",
                        "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Üzüm Pekmezi",
                    Code = "NS-008",
                    Description = "Ege bağlarından toplanan organik üzümlerden, geleneksel yöntemlerle kaynatılarak hazırlanır. Tamamen katkısızdır. 500ml",
                    Category = "Bal & Pekmez",
                    Price = 195m,
                    ImageUrl = "https://images.unsplash.com/photo-1615485925511-7250453881a3?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1615485925511-7250453881a3?w=800&q=80",
                        "https://images.unsplash.com/photo-1506377247307-41a3deda1c40?w=800&q=80",
                        "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=800&q=80"
                    },
                    Stock = 65,
                    StoryText = "Ege'nin güneşli bağlarından toplanan üzümler, köydeki kadınların elinde geleneksel yöntemlerle pekmeze dönüşür. Hiçbir şeker veya katkı maddesi eklenmeden, sadece üzümün doğal şekeriyle kaynatılarak hazırlanır. Demir açısından zengin bu pekmez, doğal bir şifa kaynağıdır.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1615485925511-7250453881a3?w=800&q=80",
                        "https://images.unsplash.com/photo-1506377247307-41a3deda1c40?w=800&q=80",
                        "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Kuru Domates",
                    Code = "NS-009",
                    Description = "Güneşte doğal yöntemlerle kurutulmuş domates. Hiçbir koruyucu madde kullanılmadan hazırlanır. 250gr",
                    Category = "Kurutulmuş Sebzeler",
                    Price = 145m,
                    ImageUrl = "https://images.unsplash.com/photo-1576045057995-568f588f82fb?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1576045057995-568f588f82fb?w=800&q=80",
                        "https://images.unsplash.com/photo-1594223274512-ad4803739b7c?w=800&q=80",
                        "https://images.unsplash.com/photo-1530836369250-ef72a3f5cda8?w=800&q=80"
                    },
                    Stock = 85,
                    StoryText = "Akdeniz ikliminin güneşli günlerinde, özenle seçilmiş domatesler güneş altında doğal yöntemlerle kurutulur. Hiçbir kimyasal işlem görmeden, sadece tuz ve güneş ile hazırlanan bu kuru domatesler, Ege mutfağının vazgeçilmez lezzetidir. Tamamen doğal üretim ile elde edilir.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1576045057995-568f588f82fb?w=800&q=80",
                        "https://images.unsplash.com/photo-1594223274512-ad4803739b7c?w=800&q=80",
                        "https://images.unsplash.com/photo-1530836369250-ef72a3f5cda8?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Tahin",
                    Code = "NS-010",
                    Description = "Ege susamlarından geleneksel taş değirmende öğütülerek hazırlanan doğal tahin. Soğuk sıkım yöntemiyle elde edilir. 400gr",
                    Category = "Yağlar",
                    Price = 235m,
                    ImageUrl = "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=800&q=80",
                        "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=800&q=80",
                        "https://images.unsplash.com/photo-1587330979470-3585ac7d99e8?w=800&q=80"
                    },
                    Stock = 55,
                    StoryText = "Ege'nin verimli topraklarında yetişen susamlar, geleneksel taş değirmenlerde özenle öğütülür. Soğuk sıkım yöntemiyle işlenen susamlar, tüm besin değerlerini koruyarak en saf haline dönüşür. Hiçbir katkı maddesi veya koruyucu içermeyen bu tahin, geleneksel lezzetin ta kendisidir.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=800&q=80",
                        "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=800&q=80",
                        "https://images.unsplash.com/photo-1587330979470-3585ac7d99e8?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Adaçayı",
                    Code = "NS-011",
                    Description = "Toroslar'ın yüksek yaylalarından toplanan adaçayı yaprakları, doğal yöntemlerle kurutulur. Şifa kaynağı özellikleriyle bilinir. 40gr",
                    Category = "Şifalı Bitkiler",
                    Price = 110m,
                    ImageUrl = "https://images.unsplash.com/photo-1597481499755-32356fd68e7d?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1597481499755-32356fd68e7d?w=800&q=80",
                        "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?w=800&q=80",
                        "https://images.unsplash.com/photo-1615485505536-81b1e6e9e843?w=800&q=80"
                    },
                    Stock = 105,
                    StoryText = "Toroslar'ın temiz havasında yetişen adaçayı, geleneksel yöntemlerle toplanıp doğal güneş altında kurutulur. Binlerce yıldır şifa kaynağı olarak kullanılan bu bitki, organik tarım yöntemleriyle yetiştirilir. Hiçbir kimyasal işlem görmeden sofralarınıza gelir.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1597481499755-32356fd68e7d?w=800&q=80",
                        "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?w=800&q=80",
                        "https://images.unsplash.com/photo-1615485505536-81b1e6e9e843?w=800&q=80"
                    }
                },
                new Product
                {
                    Name = "Köy Yumurtası",
                    Code = "NS-012",
                    Description = "Doğal ortamda yetiştirilen tavuklardan elde edilen köy yumurtası. Organik tarım sertifikalı ve tamamen doğal beslenme ile üretilir. 30 adet",
                    Category = "Süt Ürünleri",
                    Price = 180m,
                    ImageUrl = "https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?w=800&q=80",
                    Images = new string[] 
                    {
                        "https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?w=800&q=80",
                        "https://images.unsplash.com/photo-1518568814500-bf0f8d125f46?w=800&q=80",
                        "https://images.unsplash.com/photo-1586972339394-88c24febe0b6?w=800&q=80"
                    },
                    Stock = 38,
                    StoryText = "Köydeki küçük çiftliklerde, doğal ortamda yetiştirilen tavuklar, organik yemlerle beslenir. Bu tavuklardan elde edilen yumurtalar, fabrika üretiminden tamamen farklıdır. Her yumurta, doğanın ve özenli bakımın bir meyvesidir. Organik tarım sertifikalı bu yumurtalar, sağlıklı beslenmenin en doğal hali.",
                    StoryImages = new string[]
                    {
                        "https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?w=800&q=80",
                        "https://images.unsplash.com/photo-1518568814500-bf0f8d125f46?w=800&q=80",
                        "https://images.unsplash.com/photo-1586972339394-88c24febe0b6?w=800&q=80"
                    }
                }
            };

            await context.Products.AddRangeAsync(sampleProducts, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}


