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

var builder = WebApplication.CreateBuilder(args);

// 1. JSON Ayarları
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
		options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
	});

// 2. Identity ve Auth
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(connectionString));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"] ?? "VerySecretKey1234567890123456"; // Geçici fallback
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
// --- CORS DÜZELTME ---
builder.Services.AddCors(options => {
	options.AddPolicy("AllowLocal", policy => {
		policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // 3000 veya 5173 hangisini kullanıyorsan
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISmsService, SmsService>();

var app = builder.Build();

_ = Task.Run(async () =>
{
	using var scope = app.Services.CreateScope();
	var services = scope.ServiceProvider;
	try
	{
		var context = services.GetRequiredService<AppDbContext>();

		// 1. ADIM: Bağlantı hazır mı?
		Console.WriteLine("📡 Veritabanı fiziksel bağlantısı kontrol ediliyor...");
		if (await context.Database.CanConnectAsync())
		{
			Console.WriteLine("✅ Fiziksel bağlantı OK.");

			// 2. ADIM: Migration'ları zorla
			Console.WriteLine("🏗️ Migrationlar uygulanıyor (Tablolar oluşturuluyor)...");
			await context.Database.MigrateAsync();
			Console.WriteLine("🚀 Tablolar başarıyla oluşturuldu/güncellendi.");

			// 3. ADIM: Seed verilerini bas
			await SeedData.InitializeAsync(context);
			Console.WriteLine("💎 Seed verileri başarıyla yüklendi.");
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine($"❌ KRİTİK VERİTABANI HATASI: {ex.Message}");
		if (ex.InnerException != null)
			Console.WriteLine($"🔍 DETAY: {ex.InnerException.Message}");
	}
});

app.UseSwagger();
app.UseSwaggerUI();

var imagesPath = Path.Combine(app.Environment.ContentRootPath, "images");
if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(imagesPath),
	RequestPath = "/images"
});

app.UseDefaultFiles();
app.UseStaticFiles();

// Render/Docker ortamında bazen yönlendirme sorun çıkarabilir, şimdilik kapatabilirsin
// app.UseHttpsRedirection(); 

app.UseRouting();
app.UseCors("AllowLocal");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();