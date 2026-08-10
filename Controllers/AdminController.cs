using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NaturalShop.API.Data;
using NaturalShop.API.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NaturalShop.API.Controllers
{
    /// <summary>
    /// Mağaza sahibinin siparişleri görüp kargo durumunu güncellemesi için basit, şifre korumalı panel.
    /// Müşteri hesap sistemiyle (ApplicationUser/Identity) ilişkisi yoktur - tek bir paylaşılan
    /// admin şifresi (AdminSettings:Password, user-secrets/env var ile ayarlanır) doğrulanır ve
    /// "scope=admin" claim'i taşıyan kısa ömürlü ayrı bir JWT üretilir.
    /// </summary>
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private static readonly string[] AllowedShippingStatuses =
        {
            "Hazırlanıyor", "Kargoya Verildi", "Teslim Edildi"
        };

        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext db, IConfiguration config, ILogger<AdminController> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] AdminLoginDto dto)
        {
            var adminPassword = _config["AdminSettings:Password"];
            if (string.IsNullOrEmpty(adminPassword))
            {
                _logger.LogError("AdminSettings:Password yapılandırılmamış.");
                return StatusCode(500, new { message = "Admin paneli yapılandırılmamış." });
            }

            if (dto.Password != adminPassword)
            {
                return Unauthorized(new { message = "Şifre hatalı ❌" });
            }

            var jwtKey = _config["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key not found");
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("scope", "admin")
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        [HttpGet("orders")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.User)
                .OrderByDescending(o => o.Id)
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.Status,
                    o.ShippingStatus,
                    o.TrackingNumber,
                    o.TotalAmount,
                    o.RecipientName,
                    o.RecipientPhone,
                    o.ShippingAddress,
                    CustomerEmail = o.User.Email,
                    Items = o.Items.Select(i => new { i.ProductName, i.Quantity, i.UnitPrice })
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpPut("orders/{id}/shipping-status")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateShippingStatus(int id, [FromBody] UpdateShippingStatusDto dto)
        {
            if (!AllowedShippingStatuses.Contains(dto.ShippingStatus))
            {
                return BadRequest(new { message = $"Geçersiz kargo durumu. İzin verilenler: {string.Join(", ", AllowedShippingStatuses)}" });
            }

            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Sipariş bulunamadı" });
            }

            order.ShippingStatus = dto.ShippingStatus;
            if (dto.TrackingNumber != null)
            {
                order.TrackingNumber = string.IsNullOrWhiteSpace(dto.TrackingNumber) ? null : dto.TrackingNumber.Trim();
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Kargo durumu güncellendi ✅", order.Id, order.ShippingStatus, order.TrackingNumber });
        }
    }
}
