using Microsoft.EntityFrameworkCore;
using NaturalShop.API.Data;
using AutoMapper;
using NaturalShop.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NaturalShop.API.Services;
using Microsoft.Extensions.FileProviders;

// ... (using satırlarınız aynı kalıyor)

var builder = WebApplication.CreateBuilder(args);

// 1. JSON Ayarları
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.ReferenceHandler =
			System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
		// JavaScript/React uyumluluğu için camelCase kullan
		options.JsonSerializerOptions.PropertyNamingPolicy = 
			System.Text.Json.JsonNamingPolicy.CamelCase;
	});

// 2. Identity ve Auth Ayarları
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings["Issuer"],
		ValidAudience = jwtSettings["Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(key)
	};
});

// --- CORS YAPILANDIRMASI (DÜZELTİLDİ) ---
builder.Services.AddCors(options =>
{
	options.AddPolicy("FrontendCors", policy =>
	{
		policy.WithOrigins(
				"https://natural-shop-eta.vercel.app",
				"https://www.pinararsslan.com",
				"https://pinararsslan.com",
				"http://localhost:3000",
				"http://localhost:3001"
			)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials(); // Auth kullanıyorsanız bu önemlidir
	});
});

// Diğer servisler
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISmsService, SmsService>();

var app = builder.Build();

// --- 1. SWAGGER & STATIC FILES (Hemen hazır olmalı) ---
app.UseSwagger();
app.UseSwaggerUI();

// --- 2. VERİTABANI İŞLEMLERİNİ ASENKRON BAŞLAT (BLOKLAMADAN) ---
// Task.Run kullanarak bu ağır işlemi arka plana atıyoruz, böylece app.Run() hemen çalışabilir.
_ = Task.Run(async () =>
{
	using var scope = app.Services.CreateScope();
	var services = scope.ServiceProvider;
	try
	{
		var context = services.GetRequiredService<AppDbContext>();
		Console.WriteLine("🚀 Arka planda veritabanı işlemleri başladı...");

		await context.Database.MigrateAsync();
		await SeedData.InitializeAsync(context);

		Console.WriteLine("✅ Veritabanı arka planda hazırlandı!");
	}
	catch (Exception ex)
	{
		Console.WriteLine($"❌ Veritabanı hatası: {ex.Message}");
	}
});

// Görsel yönetimi
var imagesPath = Path.Combine(app.Environment.ContentRootPath, "images");
if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(imagesPath),
	RequestPath = "/images"
});


app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();

// ÖNEMLİ SIRALAMA: Cors -> Authentication -> Authorization
app.UseCors("FrontendCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();