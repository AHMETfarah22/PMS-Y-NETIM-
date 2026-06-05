using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace PmsApi.Helpers
{
    public static class EmailHelper
    {
        private static string SmtpUser;
        private static string SmtpPass;
        private static string SmtpHost;
        private static int SmtpPort;
        
        public static void Initialize(IConfiguration configuration)
        {
            SmtpUser = configuration["SmtpConfig:User"] ?? "";
            SmtpPass = configuration["SmtpConfig:Pass"] ?? "";
            SmtpHost = configuration["SmtpConfig:Host"] ?? "smtp.gmail.com";
            SmtpPort = int.TryParse(configuration["SmtpConfig:Port"], out int port) ? port : 587;
        }

        public class EmailResult
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }

        public static async Task SendReservationReceivedEmailAsync(string to, string name, string room, int bed, decimal price, string resCode)
        {
            if (string.IsNullOrEmpty(to)) return;

            string subject = "Rezervasyon Talebiniz Alındı - SOM-PMS";
            string body = $@"
                <div style='font-family: sans-serif; max-width: 500px; border: 1px solid #eee; padding: 20px; border-radius: 10px;'>
                    <h2 style='color: #d4af37;'>SOM-PMS | Rezervasyon Talebi</h2>
                    <p>Sayın <b>{name}</b>,</p>
                    <p>Rezervasyon talebiniz başarıyla alınmıştır. Ekibimiz en kısa sürede onaylayacaktır.</p>
                    <hr/>
                    <p><b>Rezervasyon Kodunuz:</b> <span style='color: #d4af37; font-weight: bold;'>{resCode}</span></p>
                    <p><b>Oda No:</b> {room}</p>
                    <p><b>Yatak No:</b> {bed}</p>
                    <p><b>Gecelik Fiyat:</b> {price:N2} ₺</p>
                    <hr/>
                    <p style='font-size: 12px; color: #777;'>Bu bir otomatik bilgilendirme e-postasıdır. Lütfen yanıtlamayınız.</p>
                </div>";

            try
            {
                using var smtpClient = new SmtpClient(SmtpHost)
                {
                    Port = SmtpPort,
                    Credentials = new NetworkCredential(SmtpUser, SmtpPass),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 20000 // 20 saniye zaman aşımı
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(SmtpUser, "SOM-PMS Online Rezervasyon"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // API loguna yaz
                System.Console.WriteLine($"Email Error: {ex.Message}");
            }
        }

        public static async Task<EmailResult> SendLogoutEmailAsync(string to, string name)
        {
            if (string.IsNullOrEmpty(to))
                return new EmailResult { Success = false, Message = "Email adresi boş" };

            string subject = "Oturumunuz Kapatıldı - SOM-PMS";
            string body = $@"
                <div style='font-family: sans-serif; max-width: 500px; border: 1px solid #eee; padding: 20px; border-radius: 10px;'>
                    <h2 style='color: #d4af37;'>SOM-PMS | Oturum Kapatma Bildirimi</h2>
                    <p>Sayın <b>{name}</b>,</p>
                    <p>Sistemde güvenli bir şekilde oturumunuz kapatılmıştır.</p>
                    <p><b>Çıkış Saati:</b> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
                    <hr/>
                    <p>Eğer bu işlemi siz yapmadıysanız, lütfen hemen yöneticiye başvurunuz.</p>
                    <p style='font-size: 12px; color: #777;'>Bu bir otomatik bilgilendirme e-postasıdır. Lütfen yanıtlamayınız.</p>
                </div>";

            try
            {
                Console.WriteLine($"[{DateTime.Now}] Email gönderiliyor: {to}");
                
                using var smtpClient = new SmtpClient(SmtpHost)
                {
                    Port = SmtpPort,
                    Credentials = new NetworkCredential(SmtpUser, SmtpPass),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 20000
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(SmtpUser, "SOM-PMS Sistem"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
                
                Console.WriteLine($"[{DateTime.Now}] Email başarıyla gönderildi: {to}");
                return new EmailResult { Success = true, Message = "Email başarıyla gönderildi" };
            }
            catch (Exception ex)
            {
                string errorMsg = $"Email Hata ({to}): {ex.GetType().Name} - {ex.Message}";
                Console.WriteLine($"[{DateTime.Now}] {errorMsg}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[{DateTime.Now}] İç Hata: {ex.InnerException.Message}");
                
                return new EmailResult { Success = false, Message = errorMsg };
            }
        }
    }
}
