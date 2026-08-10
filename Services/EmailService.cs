using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> SendVerificationCodeAsync(string email, string code, int expiresInMinutes)
        {
            var body = $"""
                Merhaba,

                Giriş yapmak için doğrulama kodunuz: {code}

                Bu kod {expiresInMinutes} dakika süreyle geçerlidir. Bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.

                Köyümüzden Sofranıza
                """;

            // Render gibi platformlar giden SMTP portlarını (25/465/587) ağ seviyesinde
            // engelliyor; bu yüzden production'da HTTPS tabanlı bir e-posta API'si tercih
            // ediliyor. SendGrid (Single Sender Verification, domain gerekmez, tüm alıcılara
            // gönderim yapabilir) öncelikli; Resend (yalnızca doğrulanmış domain'e sahip
            // hesabın kendi e-postasına gönderebiliyor, bkz. sandbox kısıtlaması) onun
            // yedeği; SMTP ise yalnızca local/dev için son çare.
            var sendGridApiKey = _configuration["EmailSettings:SendGrid:ApiKey"];
            if (!string.IsNullOrEmpty(sendGridApiKey))
            {
                return await SendViaSendGridAsync(sendGridApiKey, email, code, expiresInMinutes, body);
            }

            var resendApiKey = _configuration["EmailSettings:Resend:ApiKey"];
            if (!string.IsNullOrEmpty(resendApiKey))
            {
                return await SendViaResendAsync(resendApiKey, email, code, expiresInMinutes, body);
            }

            var smtpHost = _configuration["EmailSettings:Smtp:Host"];
            var smtpUsername = _configuration["EmailSettings:Smtp:Username"];
            var smtpPassword = _configuration["EmailSettings:Smtp:Password"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("E-posta ayarları (SendGrid/Resend/SMTP) bulunamadı. Development modunda kod console'a yazdırılıyor.");
                LogDevFallback(email, code, expiresInMinutes, null);
                return true; // Development için true döndür
            }

            return await SendViaSmtpAsync(smtpHost, smtpUsername, smtpPassword, email, code, expiresInMinutes, body);
        }

        private async Task<bool> SendViaResendAsync(string apiKey, string email, string code, int expiresInMinutes, string body)
        {
            var from = _configuration["EmailSettings:Resend:From"] ?? "onboarding@resend.dev";
            var fromName = _configuration["EmailSettings:Resend:FromName"] ?? "Köyümüzden Sofranıza";

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.resend.com/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new
                {
                    from = $"{fromName} <{from}>",
                    to = new[] { email },
                    subject = "Giriş doğrulama kodunuz",
                    text = body
                };

                using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("emails", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Doğrulama kodu e-postası Resend ile gönderildi: {email}");
                    return true;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Resend gönderim hatası ({(int)response.StatusCode}): {responseBody}");
                LogDevFallback(email, code, expiresInMinutes, $"Resend {(int)response.StatusCode}: {responseBody}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Resend gönderim hatası: {ex.Message}");
                LogDevFallback(email, code, expiresInMinutes, ex.Message);
                return false;
            }
        }

        private async Task<bool> SendViaSendGridAsync(string apiKey, string email, string code, int expiresInMinutes, string body)
        {
            var from = _configuration["EmailSettings:SendGrid:From"];
            var fromName = _configuration["EmailSettings:SendGrid:FromName"] ?? "Köyümüzden Sofranıza";

            if (string.IsNullOrEmpty(from))
            {
                _logger.LogError("EmailSettings:SendGrid:From tanımlı değil (Single Sender Verification ile doğrulanan adres olmalı).");
                LogDevFallback(email, code, expiresInMinutes, "EmailSettings:SendGrid:From eksik");
                return false;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.sendgrid.com/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new
                {
                    personalizations = new[]
                    {
                        new { to = new[] { new { email } } }
                    },
                    from = new { email = from, name = fromName },
                    subject = "Giriş doğrulama kodunuz",
                    content = new[]
                    {
                        new { type = "text/plain", value = body }
                    }
                };

                using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("v3/mail/send", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Doğrulama kodu e-postası SendGrid ile gönderildi: {email}");
                    return true;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError($"SendGrid gönderim hatası ({(int)response.StatusCode}): {responseBody}");
                LogDevFallback(email, code, expiresInMinutes, $"SendGrid {(int)response.StatusCode}: {responseBody}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SendGrid gönderim hatası: {ex.Message}");
                LogDevFallback(email, code, expiresInMinutes, ex.Message);
                return false;
            }
        }

        private async Task<bool> SendViaSmtpAsync(string smtpHost, string smtpUsername, string smtpPassword, string email, string code, int expiresInMinutes, string body)
        {
            var smtpPort = _configuration["EmailSettings:Smtp:Port"];
            var fromAddress = _configuration["EmailSettings:Smtp:From"] ?? smtpUsername;
            var fromName = _configuration["EmailSettings:Smtp:FromName"] ?? "Köyümüzden Sofranıza";
            var enableSsl = bool.TryParse(_configuration["EmailSettings:Smtp:EnableSsl"], out var ssl) ? ssl : true;

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromAddress));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Giriş doğrulama kodunuz";
                message.Body = new TextPart("plain") { Text = body };

                using var client = new SmtpClient();
                // MailKit, host için dönen tüm IP adreslerini (IPv4/IPv6) sırayla dener.
                await client.ConnectAsync(smtpHost, int.TryParse(smtpPort, out var port) ? port : 587,
                    enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Doğrulama kodu e-postası SMTP ile gönderildi: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SMTP gönderim hatası: {ex.Message}");
                LogDevFallback(email, code, expiresInMinutes, ex.Message);
                return false;
            }
        }

        private static void LogDevFallback(string email, string code, int expiresInMinutes, string? error)
        {
            Console.WriteLine($"\n=== DOĞRULAMA KODU ({(error == null ? "Development" : "Hata Durumunda")}) ===");
            Console.WriteLine($"E-posta: {email}");
            Console.WriteLine($"Kod: {code}");
            Console.WriteLine($"Geçerlilik: {expiresInMinutes} dakika");
            if (error != null) Console.WriteLine($"Hata: {error}");
            Console.WriteLine($"Zaman: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("=====================================\n");
        }
    }
}
