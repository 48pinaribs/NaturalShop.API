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

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = new Order
            {
                UserId = userId ?? throw new InvalidOperationException("User ID not found"),
                Status = "Hazırlanıyor",
                CreatedAt = DateTime.Now,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Sipariş oluşturuldu ✅" });
        }

    }
}
