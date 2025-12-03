using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NaturalShop.API.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
        {
            try
            {
                // Netgsm SMS servisi için örnek implementasyon
                // Gerçek SMS servisi entegrasyonu için Netgsm, IletiMerkezi vb. kullanılabilir
                
                var netgsmUsername = _configuration["SmsSettings:Netgsm:Username"];
                var netgsmPassword = _configuration["SmsSettings:Netgsm:Password"];
                var netgsmApiUrl = _configuration["SmsSettings:Netgsm:ApiUrl"] ?? "https://api.netgsm.com.tr/sms/send/get";

                // Eğer Netgsm ayarları yoksa, development modunda console'a yazdır
                if (string.IsNullOrEmpty(netgsmUsername) || string.IsNullOrEmpty(netgsmPassword))
                {
                    _logger.LogWarning("SMS ayarları bulunamadı. Development modunda kod console'a yazdırılıyor.");
                    Console.WriteLine($"\n=== SMS KODU (Development) ===");
                    Console.WriteLine($"Telefon: {phoneNumber}");
                    Console.WriteLine($"Kod: {code}");
                    Console.WriteLine($"Süre: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine("=============================\n");
                    return true; // Development için true döndür
                }

                // Netgsm API çağrısı
                var message = $"Köyümüzden Sofranıza - Doğrulama kodunuz: {code}";
                var requestUrl = $"{netgsmApiUrl}?usercode={netgsmUsername}&password={netgsmPassword}&gsmno={phoneNumber}&message={Uri.EscapeDataString(message)}&msgheader=KÖYÜMÜZDEN SOFRANIZA";

                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(requestUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode && responseContent.StartsWith("00"))
                {
                    _logger.LogInformation($"SMS başarıyla gönderildi: {phoneNumber}");
                    return true;
                }
                else
                {
                    _logger.LogError($"SMS gönderilemedi: {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SMS gönderme hatası: {ex.Message}");
                
                // Hata durumunda development modunda console'a yazdır
                Console.WriteLine($"\n=== SMS KODU (Hata Durumunda) ===");
                Console.WriteLine($"Telefon: {phoneNumber}");
                Console.WriteLine($"Kod: {code}");
                Console.WriteLine($"Hata: {ex.Message}");
                Console.WriteLine("=============================\n");
                
                return false;
            }
        }
    }
}

