using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NaturalShop.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NaturalShop.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using NaturalShop.API.Data;
using NaturalShop.API.Services;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;
    private readonly ISmsService _smsService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager, 
        IConfiguration config,
        AppDbContext context,
        ISmsService smsService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _config = config;
        _context = context;
        _smsService = smsService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Kayıt başarılı ✅");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return Unauthorized("Kullanıcı bulunamadı ❌");

        var check = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!check) return Unauthorized("Şifre hatalı ❌");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var key = Encoding.UTF8.GetBytes(_config["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key not found"));
        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            user = new { user.Id, user.FullName, user.Email }
        });
    }

    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromBody] SendCodeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Son 5 dakika içinde gönderilmiş kod var mı kontrol et
            var recentCode = await _context.VerificationCodes
                .Where(v => v.PhoneNumber == dto.PhoneNumber && 
                           !v.IsUsed && 
                           v.ExpiresAt > DateTime.UtcNow &&
                           v.CreatedAt > DateTime.UtcNow.AddMinutes(-5))
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (recentCode != null)
            {
                var remainingSeconds = (int)(recentCode.CreatedAt.AddMinutes(5) - DateTime.UtcNow).TotalSeconds;
                return BadRequest(new { message = $"Lütfen {remainingSeconds} saniye sonra tekrar deneyin." });
            }

            // Eski kullanılmamış kodları işaretle
            var oldCodes = await _context.VerificationCodes
                .Where(v => v.PhoneNumber == dto.PhoneNumber && !v.IsUsed && v.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
            
            foreach (var oldCode in oldCodes)
            {
                oldCode.IsUsed = true;
            }
            await _context.SaveChangesAsync();

            // Yeni kod oluştur (6 haneli)
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            var verificationCode = new VerificationCode
            {
                PhoneNumber = dto.PhoneNumber,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), // 10 dakika geçerli
                IsUsed = false
            };

            _context.VerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();

            // SMS gönder
            var smsSent = await _smsService.SendVerificationCodeAsync(dto.PhoneNumber, code);

            if (!smsSent)
            {
                _logger.LogWarning($"SMS gönderilemedi ama kod oluşturuldu: {dto.PhoneNumber}");
                // Development modunda bile kod oluşturulduğu için başarılı dönebiliriz
            }

            return Ok(new { message = "Doğrulama kodu gönderildi." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Kod gönderme hatası: {ex.Message}");
            return StatusCode(500, new { message = "Kod gönderilirken bir hata oluştu. Lütfen tekrar deneyin." });
        }
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Geçerli kod bul
            var verificationCode = await _context.VerificationCodes
                .Where(v => v.PhoneNumber == dto.PhoneNumber &&
                           v.Code == dto.Code &&
                           !v.IsUsed &&
                           v.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (verificationCode == null)
            {
                return BadRequest(new { message = "Geçersiz veya süresi dolmuş kod." });
            }

            // Kodu kullanıldı olarak işaretle
            verificationCode.IsUsed = true;
            verificationCode.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Kullanıcıyı bul veya oluştur
            var user = await _userManager.FindByNameAsync(dto.PhoneNumber);
            
            if (user == null)
            {
                // Yeni kullanıcı oluştur
                user = new ApplicationUser
                {
                    UserName = dto.PhoneNumber,
                    PhoneNumber = dto.PhoneNumber,
                    FullName = "Kullanıcı", // Varsayılan isim, sonra güncellenebilir
                    Email = $"{dto.PhoneNumber}@naturalshop.local", // Geçici email
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { message = "Kullanıcı oluşturulamadı.", errors = createResult.Errors });
                }
            }
            else
            {
                // Mevcut kullanıcının telefon numarasını doğrula
                if (!user.PhoneNumberConfirmed)
                {
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }
            }

            // JWT token oluştur
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.MobilePhone, dto.PhoneNumber)
            };

            var key = Encoding.UTF8.GetBytes(_config["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key not found"));
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = new { user.Id, user.FullName, user.PhoneNumber }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Kod doğrulama hatası: {ex.Message}");
            return StatusCode(500, new { message = "Kod doğrulanırken bir hata oluştu. Lütfen tekrar deneyin." });
        }
    }
}
