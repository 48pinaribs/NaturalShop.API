using Microsoft.EntityFrameworkCore;
using NaturalShop.API.Data;
using AutoMapper;
using NaturalShop.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using NaturalShop.API.Services;
using Microsoft.Extensions.FileProviders;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERİTABANI VE CONNECTION STRING ---
// Render üzerindeki DefaultConnection'ı alır.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(connectionString, npgsqlOptions => {
		npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null); // Bağlantı koparsa 5 kez dene
	}));

// --- 2. IDENTITY VE AUTHENTICATION ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"] ?? "SeniorSecretKey1234567890123456";
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

// --- 3. CORS AYARLARI ---
builder.Services.AddCors(options => {
	options.AddPolicy("AllowLocal", policy => {
		policy.WithOrigins(
				"http://localhost:3000",
				"https://natural-shop-eta.vercel.app",
				"https://www.pinararsslan.com",
				"https://pinararsslan.com"
			)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

// ✅ Doğru
builder.Services.AddControllers()
	.AddJsonOptions(options => {
		options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
		options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISmsService, SmsService>();

var app = builder.Build();

// --- 5. ARKA PLAN MIGRATION SİSTEMİ (BLOCKING OLMAYAN) ---
// Uygulama hemen ayağa kalkar, Render "Timed Out" vermez.
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var context = services.GetRequiredService<AppDbContext>();
		Console.WriteLine("📡 [DB] Bağlantı kontrol ediliyor...");

		// Timeout süresini ayarla
		context.Database.SetCommandTimeout(120);

		// ÖNEMLİ: Daha önce EnsureCreated kullandıysan MigrateAsync hata verebilir.
		// Şimdilik en garantisi şudur:
		await context.Database.EnsureCreatedAsync();

		Console.WriteLine("🚀 [DB] Tablolar kontrol edildi/oluşturuldu.");

		// DbInitializer'ı burada çağırıyoruz
		DbInitializer.Seed(app);

		Console.WriteLine("💎 [DB] DbInitializer işlemi tamamlandı.");
	}
	catch (Exception ex)
	{
		Console.WriteLine($"⚠️ [DB] Kritik Başlangıç Hatası: {ex.Message}");
		// İç hatayı da yazdıralım ki asıl sebebi görelim
		if (ex.InnerException != null)
			Console.WriteLine($"🔍 [DB] Detay: {ex.InnerException.Message}");
	}
}
// --- 6. MIDDLEWARE PIPELINE ---
// Geliştirme ortamında olmasak bile Swagger'ı Render'da görebilmek için if dışına aldık
app.UseSwagger();
app.UseSwaggerUI(c => {
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "NaturalShop API V1");
	c.RoutePrefix = "swagger";
});

// Resim dosyaları için fiziksel yol ayarı
var imagesPath = Path.Combine(app.Environment.ContentRootPath, "images");
if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(imagesPath),
	RequestPath = "/images"
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowLocal");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ana dizine geleni Swagger'a yönlendir
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();