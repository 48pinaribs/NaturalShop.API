using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NaturalShop.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendVerificationCodeAsync(string email, string code, int expiresInMinutes)
        {
            var smtpHost = _configuration["EmailSettings:Smtp:Host"];
            var smtpPort = _configuration["EmailSettings:Smtp:Port"];
            var smtpUsername = _configuration["EmailSettings:Smtp:Username"];
            var smtpPassword = _configuration["EmailSettings:Smtp:Password"];
            var fromAddress = _configuration["EmailSettings:Smtp:From"] ?? smtpUsername;
            var fromName = _configuration["EmailSettings:Smtp:FromName"] ?? "Köyümüzden Sofranıza";
            var enableSsl = bool.TryParse(_configuration["EmailSettings:Smtp:EnableSsl"], out var ssl) ? ssl : true;

            // SMTP ayarları tanımlı değilse (örn. henüz kurulmadıysa), development modunda
            // kodu console'a yazdır — SmsService'teki eski davranışla aynı mantık.
            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("E-posta (SMTP) ayarları bulunamadı. Development modunda kod console'a yazdırılıyor.");
                Console.WriteLine($"\n=== DOĞRULAMA KODU (Development) ===");
                Console.WriteLine($"E-posta: {email}");
                Console.WriteLine($"Kod: {code}");
                Console.WriteLine($"Geçerlilik: {expiresInMinutes} dakika");
                Console.WriteLine($"Zaman: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine("=====================================\n");
                return true; // Development için true döndür
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromAddress!));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Giriş doğrulama kodunuz";
                message.Body = new TextPart("plain")
                {
                    Text = $"""
                        Merhaba,

                        Giriş yapmak için doğrulama kodunuz: {code}

                        Bu kod {expiresInMinutes} dakika süreyle geçerlidir. Bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.

                        Köyümüzden Sofranıza
                        """
                };

                using var client = new SmtpClient();
                // MailKit, host için dönen tüm IP adreslerini (IPv4/IPv6) sırayla dener; bu sayede
                // Render gibi platformlarda IPv6 çıkışı çalışmasa bile IPv4'e otomatik düşer
                // (System.Net.Mail.SmtpClient'ın aksine — o tek adresi dener ve "Network unreachable" ile patlar).
                await client.ConnectAsync(smtpHost, int.TryParse(smtpPort, out var port) ? port : 587,
                    enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Doğrulama kodu e-postası gönderildi: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta gönderme hatası: {ex.Message}");

                // Hata durumunda development modunda console'a yazdır ki test akışı kesilmesin
                Console.WriteLine($"\n=== DOĞRULAMA KODU (Hata Durumunda) ===");
                Console.WriteLine($"E-posta: {email}");
                Console.WriteLine($"Kod: {code}");
                Console.WriteLine($"Hata: {ex.Message}");
                Console.WriteLine("========================================\n");

                return false;
            }
        }
    }
}
