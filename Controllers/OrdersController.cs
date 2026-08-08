using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaturalShop.API.Data;
using NaturalShop.API.DTOs;
using NaturalShop.API.Models;
using System.Security.Claims;

namespace NaturalShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Giriş yapmadan sipariş işlemleri yapılamaz
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrdersController(AppDbContext db)
        {
            _db = db;
        }

        // ✅ Kullanıcının kendi siparişlerini getir
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return Ok(orders);
        }

        // ✅ Belirli bir siparişi ID ile getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _db.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Include(o => o.Items)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new { message = "Sipariş bulunamadı" });
            }

            return Ok(order);
        }

        // ✅ Kapıda Ödeme siparişi oluşturur (online ödeme gerektirmez, sipariş anında onaylanır)
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
            }

            if (dto.Items == null || !dto.Items.Any())
            {
                return BadRequest(new { message = "Sepet boş olamaz" });
            }

            // Ürünleri veritabanından al ve fiyat/stok kontrolü yap (server-side, güvenlik için kritik!)
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var missingProducts = productIds.Except(products.Select(p => p.Id)).ToList();
            if (missingProducts.Any())
            {
                return BadRequest(new { message = $"Ürün bulunamadı: {string.Join(", ", missingProducts)}" });
            }

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                if (item.Quantity <= 0)
                {
                    return BadRequest(new { message = $"{product.Name} için geçersiz miktar: {item.Quantity}" });
                }

                if (product.Stock < item.Quantity)
                {
                    return BadRequest(new { message = $"{product.Name} için yeterli stok yok. Mevcut stok: {product.Stock}" });
                }

                totalAmount += product.Price * item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                });
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    Status = "CashOnDelivery", // Kapıda ödeme - online ödeme adımı yok, sipariş direkt onaylanır
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Items = orderItems,
                    RecipientName = dto.RecipientName,
                    RecipientPhone = dto.RecipientPhone,
                    ShippingAddress = dto.ShippingAddress,
                };

                _db.Orders.Add(order);

                // Stoktan düş (kapıda ödemede online ödeme callback'i olmadığı için stok burada düşürülür)
                foreach (var item in dto.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);
                    product.Stock -= item.Quantity;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Sipariş oluşturuldu ✅",
                    orderId = order.Id,
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Sipariş oluşturulamadı. Lütfen tekrar deneyin." });
            }
        }

    }
}
