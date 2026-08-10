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
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    // Doğrulama kodunun geçerlilik süresi (dakika). Kod bu süre sonunda otomatik geçersiz olur.
    private const int CodeExpiryMinutes = 10;
    // Aynı e-postaya art arda kod isteği için bekleme süresi (dakika).
    private const int CodeResendCooldownMinutes = 5;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        AppDbContext context,
        IEmailService emailService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _config = config;
        _context = context;
        _emailService = emailService;
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
            var email = dto.Email.Trim().ToLowerInvariant();

            // Son birkaç dakika içinde gönderilmiş kod var mı kontrol et
            var recentCode = await _context.VerificationCodes
                .Where(v => v.Email == email &&
                           !v.IsUsed &&
                           v.ExpiresAt > DateTime.UtcNow &&
                           v.CreatedAt > DateTime.UtcNow.AddMinutes(-CodeResendCooldownMinutes))
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (recentCode != null)
            {
                var remainingSeconds = (int)(recentCode.CreatedAt.AddMinutes(CodeResendCooldownMinutes) - DateTime.UtcNow).TotalSeconds;
                return BadRequest(new { message = $"Lütfen {remainingSeconds} saniye sonra tekrar deneyin." });
            }

            // Eski kullanılmamış kodları işaretle
            var oldCodes = await _context.VerificationCodes
                .Where(v => v.Email == email && !v.IsUsed && v.ExpiresAt < DateTime.UtcNow)
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
                Email = email,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(CodeExpiryMinutes),
                IsUsed = false
            };

            _context.VerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();

            // E-posta gönder
            var emailSent = await _emailService.SendVerificationCodeAsync(email, code, CodeExpiryMinutes);

            if (!emailSent)
            {
                _logger.LogWarning($"E-posta gönderilemedi: {email}");

                // Gönderim başarısız oldu; bu kodu geçersiz say ki kullanıcı
                // bekleme süresine takılmadan hemen tekrar deneyebilsin.
                verificationCode.IsUsed = true;
                await _context.SaveChangesAsync();

                return StatusCode(502, new { message = "Doğrulama kodu e-postanıza gönderilemedi. Lütfen tekrar deneyin." });
            }

            return Ok(new { message = "Doğrulama kodu e-postanıza gönderildi.", expiresInMinutes = CodeExpiryMinutes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Kod gönderme hatası: {ex.Message}");
            return StatusCode(500, new { message = "Kod gönderilirken bir hata oluştu. Lütfen tekrar deneyin." });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "Kullanıcı bulunamadı" });
        }

        return Ok(new { user.Id, user.FullName, user.Email, user.Address });
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "Kullanıcı bulunamadı" });
        }

        user.FullName = dto.FullName.Trim();
        user.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Profil güncellenemedi.", errors = result.Errors });
        }

        return Ok(new { user.Id, user.FullName, user.Email, user.Address });
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
            var email = dto.Email.Trim().ToLowerInvariant();

            // Geçerli kod bul
            var verificationCode = await _context.VerificationCodes
                .Where(v => v.Email == email &&
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
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Yeni kullanıcı oluştur
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Kullanıcı", // Varsayılan isim, sonra güncellenebilir
                    EmailConfirmed = true // Kod ile doğrulandı
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { message = "Kullanıcı oluşturulamadı.", errors = createResult.Errors });
                }
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            // JWT token oluştur
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Kod doğrulama hatası: {ex.Message}");
            return StatusCode(500, new { message = "Kod doğrulanırken bir hata oluştu. Lütfen tekrar deneyin." });
        }
    }
}
