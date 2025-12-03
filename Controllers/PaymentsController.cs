using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaturalShop.API.Data;
using NaturalShop.API.DTOs;
using NaturalShop.API.Helpers;
using NaturalShop.API.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Security.Claims;
using System.Globalization;

namespace NaturalShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ödeme işlemleri için kullanıcı girişi zorunlu
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            AppDbContext db,
            IConfiguration configuration,
            ILogger<PaymentsController> logger)
        {
            _db = db;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Ödeme başlatma endpoint'i - Iyzico Checkout Form'unu başlatır
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartPayment([FromBody] CreateOrderDto dto)
        {
            try
            {
                // Model validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => new { Field = x.Key, Error = e.ErrorMessage }))
                        .ToList();
                    
                    _logger.LogWarning("StartPayment: Model validation hatası - {Errors}", 
                        string.Join(", ", errors.Select(e => $"{e.Field}: {e.Error}")));
                    
                    return BadRequest(new { 
                        message = "Geçersiz veri gönderildi",
                        errors = errors
                    });
                }

                // DTO null kontrolü
                if (dto == null)
                {
                    _logger.LogWarning("StartPayment: DTO null");
                    return BadRequest(new { message = "İstek verisi boş olamaz" });
                }

                _logger.LogInformation("StartPayment: DTO alındı - Items count: {Count}", dto.Items?.Count ?? 0);
                
                // Items null kontrolü
                if (dto.Items == null)
                {
                    _logger.LogWarning("StartPayment: Items null");
                    return BadRequest(new { message = "Items boş olamaz" });
                }

                // 1. Kullanıcı ID'sini JWT token'dan al
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("StartPayment: UserId from token: {UserId}", userId ?? "NULL");
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("StartPayment: Kullanıcı kimliği bulunamadı. Tüm claims: {Claims}", 
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
                }

                // 2. Kullanıcı bilgilerini veritabanından al
                var user = await _db.ApplicationUsers.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("StartPayment: Kullanıcı bulunamadı - UserId: {UserId}", userId);
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // 3. Sepet boş mu kontrol et
                if (dto.Items == null || !dto.Items.Any())
                {
                    _logger.LogWarning("StartPayment: Sepet boş - UserId: {UserId}", userId);
                    return BadRequest(new { message = "Sepet boş olamaz" });
                }

                // 4. Ürünleri veritabanından al ve fiyatları kontrol et
                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = await _db.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                _logger.LogInformation("StartPayment: Ürün sayısı: {Count}, Ürün IDs: {ProductIds}", 
                    products.Count, string.Join(", ", productIds));

                // 5. Eksik ürün kontrolü
                var missingProducts = productIds.Except(products.Select(p => p.Id)).ToList();
                if (missingProducts.Any())
                {
                    _logger.LogWarning("StartPayment: Ürün bulunamadı - ProductIds: {ProductIds}", 
                        string.Join(", ", missingProducts));
                    return BadRequest(new { message = $"Ürün bulunamadı: {string.Join(", ", missingProducts)}" });
                }

                // 6. Stok kontrolü ve toplam fiyat hesaplama (SERVER SIDE - güvenlik için kritik!)
                decimal totalAmount = 0;
                var orderItems = new List<Models.OrderItem>();

                foreach (var item in dto.Items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("StartPayment: Ürün bulunamadı - ProductId: {ProductId}", item.ProductId);
                        return BadRequest(new { message = $"Ürün bulunamadı: {item.ProductId}" });
                    }

                    // Miktar kontrolü
                    if (item.Quantity <= 0)
                    {
                        _logger.LogWarning("StartPayment: Geçersiz miktar - ProductId: {ProductId}, Quantity: {Quantity}", 
                            item.ProductId, item.Quantity);
                        return BadRequest(new { message = $"{product.Name} için geçersiz miktar: {item.Quantity}" });
                    }

                    // Stok kontrolü
                    if (product.Stock < item.Quantity)
                    {
                        _logger.LogWarning("StartPayment: Yetersiz stok - ProductId: {ProductId}, Name: {Name}, Stock: {Stock}, Requested: {Quantity}", 
                            product.Id, product.Name, product.Stock, item.Quantity);
                        return BadRequest(new { message = $"{product.Name} için yeterli stok yok. Mevcut stok: {product.Stock}" });
                    }

                    // Fiyat hesaplama - SERVER SIDE'dan alınan fiyat kullanılır (güvenlik!)
                    var itemTotal = product.Price * item.Quantity;
                    totalAmount += itemTotal;

                    // OrderItem oluştur
                    orderItems.Add(new Models.OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        Quantity = item.Quantity
                    });
                }

                // 7. Order entity'sini oluştur ve kaydet (Transaction içinde)
                Order order;
                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        order = new Order
                        {
                            UserId = userId,
                            Status = "Pending", // Ödeme bekleniyor durumu
                            CreatedAt = DateTime.UtcNow,
                            TotalAmount = totalAmount,
                            Items = orderItems
                        };

                        _db.Orders.Add(order);
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("StartPayment: Sipariş oluşturuldu - OrderId: {OrderId}, TotalAmount: {TotalAmount}", 
                            order.Id, totalAmount);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "StartPayment: Sipariş oluşturulurken hata - UserId: {UserId}", userId);
                        return StatusCode(500, new { message = "Sipariş oluşturulamadı. Lütfen tekrar deneyin." });
                    }
                }

                // 8. Iyzico Options'ı al
                var options = IyzipayConfig.GetOptions(_configuration);
                if (options == null || string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.SecretKey))
                {
                    _logger.LogError("StartPayment: Iyzico yapılandırması eksik");
                    order.Status = "Failed";
                    await _db.SaveChangesAsync();
                    return StatusCode(500, new { message = "Ödeme sistemi yapılandırma hatası" });
                }

                // 9. Iyzico Checkout Form Request oluştur
                // ÖNEMLİ: CallbackUrl localhost olamaz! Iyzico sandbox localhost'a erişemez.
                // Development için ngrok veya başka bir tunnel servisi kullanın, ya da production URL'i kullanın
                var callbackUrl = _configuration["Iyzipay:CallbackUrl"];
                if (string.IsNullOrEmpty(callbackUrl))
                {
                    _logger.LogError("StartPayment: CallbackUrl yapılandırılmamış");
                    order.Status = "Failed";
                    await _db.SaveChangesAsync();
                    return StatusCode(500, new { message = "Ödeme sistemi yapılandırma hatası: CallbackUrl eksik" });
                }
                
                var request = new CreateCheckoutFormInitializeRequest
                {
                    Locale = Locale.TR.ToString(),
                    ConversationId = order.Id.ToString(), // Sipariş ID'si conversation ID olarak kullanılır
                    Price = totalAmount.ToString("0.00", CultureInfo.InvariantCulture), // "123.45" formatında
                    PaidPrice = totalAmount.ToString("0.00", CultureInfo.InvariantCulture),
                    Currency = Currency.TRY.ToString(),
                    BasketId = order.Id.ToString(),
                    PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                    CallbackUrl = callbackUrl,
                    EnabledInstallments = new List<int> { 1, 2, 3, 6, 9, 12 } // Taksit seçenekleri
                };
                
                _logger.LogInformation("StartPayment: Iyzico Checkout Form Request oluşturuldu - OrderId: {OrderId}, CallbackUrl: {CallbackUrl}, Price: {Price}", 
                    order.Id, callbackUrl, totalAmount);

                // 10. Buyer bilgileri (kullanıcı bilgilerinden alınır)
                var buyerNameParts = (user.FullName ?? "Test User").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var buyerName = buyerNameParts.Length > 0 ? buyerNameParts[0] : "Test";
                var buyerSurname = buyerNameParts.Length > 1 ? string.Join(" ", buyerNameParts.Skip(1)) : "User";
                
                // Email formatını kontrol et ve düzelt
                string buyerEmail = user.Email ?? "test@example.com";
                
                // Iyzico geçerli email formatı bekliyor (örn: user@example.com)
                // Telefon numarası ile giriş yapan kullanıcılar için oluşturulan @naturalshop.local formatı geçersiz
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$");
                
                if (string.IsNullOrEmpty(buyerEmail) || 
                    buyerEmail.EndsWith("@naturalshop.local") ||
                    !emailRegex.IsMatch(buyerEmail))
                {
                    // Telefon numarasından geçerli bir email oluştur
                    var phoneNumber = user.PhoneNumber?.Replace("+", "").Replace(" ", "").Replace("-", "") ?? "905551234567";
                    buyerEmail = $"user{phoneNumber}@naturalshop.com";
                    _logger.LogInformation("StartPayment: Geçersiz email formatı düzeltildi. Eski: {OldEmail}, Yeni: {NewEmail}", 
                        user.Email, buyerEmail);
                }
                
                var buyer = new Buyer
                {
                    Id = userId,
                    Name = buyerName,
                    Surname = buyerSurname,
                    GsmNumber = user.PhoneNumber ?? "+905551234567", // Telefon yoksa test numarası
                    Email = buyerEmail,
                    IdentityNumber = "11111111111", // Test için - gerçek projede kullanıcıdan alınmalı
                    RegistrationAddress = user.Address ?? "Test Adres, Test Mahallesi, Test Sokak No:1",
                    City = "Istanbul",
                    Country = "Turkey",
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    ZipCode = "34000"
                };
                request.Buyer = buyer;
                
                _logger.LogInformation("StartPayment: Buyer bilgileri oluşturuldu - Name: {Name}, Surname: {Surname}, Email: {Email}", 
                    buyer.Name, buyer.Surname, buyer.Email);

                // 11. Shipping Address (Kargo adresi)
                var shippingAddress = new Address
                {
                    ContactName = user.FullName ?? "Test User",
                    City = "Istanbul",
                    Country = "Turkey",
                    Description = user.Address ?? "Test Adres, Test Mahallesi, Test Sokak No:1 Daire:1",
                    ZipCode = "34000"
                };
                request.ShippingAddress = shippingAddress;

                // 12. Billing Address (Fatura adresi - shipping ile aynı)
                var billingAddress = new Address
                {
                    ContactName = user.FullName ?? "Test User",
                    City = "Istanbul",
                    Country = "Turkey",
                    Description = user.Address ?? "Test Adres, Test Mahallesi, Test Sokak No:1 Daire:1",
                    ZipCode = "34000"
                };
                request.BillingAddress = billingAddress;

                // 13. Basket Items (Sepet ürünleri) - OrderItem'lardan oluşturulur
                var basketItems = new List<BasketItem>();
                foreach (var item in orderItems)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null) continue;

                    basketItems.Add(new BasketItem
                    {
                        Id = item.ProductId.ToString(),
                        Name = item.ProductName,
                        Category1 = product.Category ?? "Genel", // Ürün kategorisi
                        ItemType = BasketItemType.PHYSICAL.ToString(), // Fiziksel ürün
                        Price = (item.UnitPrice * item.Quantity).ToString("0.00", CultureInfo.InvariantCulture)
                    });
                }
                request.BasketItems = basketItems;

                // 14. Iyzico'ya istek gönder
                CheckoutFormInitialize checkoutFormInitialize;
                try
                {
                    checkoutFormInitialize = await Task.Run(() => CheckoutFormInitialize.Create(request, options));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "StartPayment: Iyzico API hatası - OrderId: {OrderId}", order.Id);
                    order.Status = "Failed";
                    await _db.SaveChangesAsync();
                    return StatusCode(500, new { message = "Ödeme sistemi hatası. Lütfen tekrar deneyin." });
                }

                // 15. Başarısız olursa sipariş durumunu güncelle
                if (checkoutFormInitialize == null || checkoutFormInitialize.Status != "success")
                {
                    order.Status = "Failed";
                    await _db.SaveChangesAsync();

                    var errorMessage = checkoutFormInitialize?.ErrorMessage ?? "Bilinmeyen hata";
                    _logger.LogError("StartPayment: Iyzico ödeme başlatma hatası - OrderId: {OrderId}, Error: {ErrorMessage}", 
                        order.Id, errorMessage);
                    return BadRequest(new
                    {
                        message = "Ödeme başlatılamadı",
                        error = errorMessage
                    });
                }

                // 16. Başarılı olursa token'ı siparişe kaydet
                if (string.IsNullOrEmpty(checkoutFormInitialize.Token) || string.IsNullOrEmpty(checkoutFormInitialize.PaymentPageUrl))
                {
                    _logger.LogError("StartPayment: Iyzico token veya PaymentPageUrl boş - OrderId: {OrderId}", order.Id);
                    order.Status = "Failed";
                    await _db.SaveChangesAsync();
                    return StatusCode(500, new { message = "Ödeme sistemi yanıt hatası" });
                }

                order.IyzipayToken = checkoutFormInitialize.Token;
                await _db.SaveChangesAsync();

                _logger.LogInformation("StartPayment: Ödeme başlatıldı - OrderId: {OrderId}, Token: {Token}", 
                    order.Id, checkoutFormInitialize.Token);

                // 17. Frontend'e ödeme sayfası URL'ini döndür
                return Ok(new
                {
                    orderId = order.Id,
                    paymentPageUrl = checkoutFormInitialize.PaymentPageUrl,
                    token = checkoutFormInitialize.Token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StartPayment: Beklenmeyen hata");
                return StatusCode(500, new { message = "Bir hata oluştu. Lütfen tekrar deneyin." });
            }
        }

        /// <summary>
        /// Iyzico callback endpoint'i - Ödeme sonucunu alır ve sipariş durumunu günceller
        /// GET veya POST ile çağrılabilir (Iyzico farklı şekillerde çağırabilir)
        /// </summary>
        [HttpPost("callback")]
        [HttpGet("callback")]
        [AllowAnonymous] // Iyzico'dan gelecek callback için authentication gerekmez
        public async Task<IActionResult> Callback([FromForm] string? token, [FromQuery] string? tokenQuery)
        {
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            
            try
            {
                // Token hem form'dan hem de query string'den gelebilir
                var paymentToken = token ?? tokenQuery;
                Console.WriteLine("paymentToken: " + paymentToken);
                Console.WriteLine("token: " + token);
                Console.WriteLine("tokenQuery: " + tokenQuery);
                
                if (string.IsNullOrEmpty(paymentToken))
                {
                    _logger.LogWarning("Callback: Token bulunamadı");
                    return Redirect($"{frontendUrl}/payment-result?status=error");
                }

                var options = IyzipayConfig.GetOptions(_configuration);
                if (options == null || string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.SecretKey))
                {
                    _logger.LogError("Callback: Iyzico yapılandırması eksik");
                    return Redirect($"{frontendUrl}/payment-result?status=error");
                }

                // 1) Iyzico ödeme sonucunu doğrula
                CheckoutForm checkoutForm;
                try
                {
                    var request = new RetrieveCheckoutFormRequest
                    {
                        Locale = Locale.TR.ToString(),
                        Token = paymentToken
                    };

                    checkoutForm = await Task.Run(() => CheckoutForm.Retrieve(request, options));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Callback: Iyzico API hatası - Token: {Token}", paymentToken);
                    return Redirect($"{frontendUrl}/payment-result?status=error");
                }

                if (checkoutForm == null)
                {
                    _logger.LogWarning("Callback: Iyzico yanıtı boş - Token: {Token}", paymentToken);
                    return Redirect($"{frontendUrl}/payment-result?status=error");
                }

                // 2) Token üzerinden siparişi bul (Include ile OrderItem'ları da yükle)
                var order = await _db.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.IyzipayToken == paymentToken);
                    
                if (order == null)
                {
                    _logger.LogWarning("Callback: Sipariş bulunamadı - Token: {Token}", paymentToken);
                    return Redirect($"{frontendUrl}/payment-result?status=error");
                }

                // 3) Ödeme durumunu güncelle ve stok düşür (Transaction içinde)
                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Ödeme zaten işlenmişse tekrar işleme
                        if (order.Status == "Paid")
                        {
                            _logger.LogInformation("Callback: Sipariş zaten ödendi - OrderId: {OrderId}", order.Id);
                            await transaction.CommitAsync();
                            return Redirect($"{frontendUrl}/payment-result?orderId={order.Id}");
                        }

                        if (checkoutForm.Status == "success" && checkoutForm.PaymentStatus == "SUCCESS")
                        {
                            order.Status = "Paid";
                            _logger.LogInformation("Callback: Ödeme başarılı - OrderId: {OrderId}", order.Id);

                            // Stok düşürme işlemi
                            foreach (var orderItem in order.Items)
                            {
                                var product = await _db.Products.FindAsync(orderItem.ProductId);
                                if (product != null)
                                {
                                    if (product.Stock >= orderItem.Quantity)
                                    {
                                        product.Stock -= orderItem.Quantity;
                                        _logger.LogInformation("Callback: Stok düşürüldü - ProductId: {ProductId}, Quantity: {Quantity}, RemainingStock: {Stock}", 
                                            product.Id, orderItem.Quantity, product.Stock);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Callback: Yetersiz stok - ProductId: {ProductId}, Stock: {Stock}, Requested: {Quantity}", 
                                            product.Id, product.Stock, orderItem.Quantity);
                                    }
                                }
                            }
                        }
                        else
                        {
                            order.Status = "Failed";
                            _logger.LogWarning("Callback: Ödeme başarısız - OrderId: {OrderId}, Status: {Status}, PaymentStatus: {PaymentStatus}", 
                                order.Id, checkoutForm.Status, checkoutForm.PaymentStatus);
                        }

                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Callback: Transaction hatası - OrderId: {OrderId}", order.Id);
                        return Redirect($"{frontendUrl}/payment-result?status=error");
                    }
                }

                // 4) Kullanıcıyı React sonuç sayfasına yönlendir
                return Redirect($"{frontendUrl}/payment-result?orderId={order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback: Beklenmeyen hata");
                return Redirect($"{frontendUrl}/payment-result?status=error");
            }
        }

        /// <summary>
        /// Iyzico webhook endpoint'i - Asenkron bildirimler için (şimdilik skeleton)
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous] // Webhook için authentication gerekmez
        public async Task<IActionResult> Webhook()
        {
            // TODO: Iyzico webhook signature doğrulaması yapılmalı
            // Iyzico webhook'ları için X-IYZ-SIGNATURE-V3 header'ını kontrol et

            // Raw body'yi oku
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Signature header'ını al
            var signature = Request.Headers["X-IYZ-SIGNATURE-V3"].FirstOrDefault();

            _logger.LogInformation("Webhook alındı - Signature: {Signature}, Body: {Body}",
                signature, body);

            // TODO: Signature doğrulaması yapılmalı
            // TODO: Webhook payload'ını parse et ve sipariş durumunu güncelle
            // TODO: Ödeme durumu değişikliklerini işle (ör: refund, cancel, vb.)

            return Ok(new { message = "Webhook alındı" });
        }
    }
}

