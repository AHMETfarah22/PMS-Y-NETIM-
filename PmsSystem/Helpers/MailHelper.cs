using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PmsSystem.Helpers
{
    public static class MailHelper
    {
        private static IConfiguration _configuration;
        private static string SmtpUser;
        private static string SmtpPass;
        private static string SmtpHost;
        private static int SmtpPort;

        static MailHelper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();

            SmtpUser = _configuration["SmtpConfig:User"] ?? "";
            SmtpPass = _configuration["SmtpConfig:Pass"] ?? "";
            SmtpHost = _configuration["SmtpConfig:Host"] ?? "smtp.gmail.com";
            SmtpPort = int.TryParse(_configuration["SmtpConfig:Port"], out int port) ? port : 587;
        }

        public static async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(to) || !to.Contains("@")) return;

            try
            {
                using var smtpClient = new SmtpClient(SmtpHost)
                {
                    Port = SmtpPort,
                    Credentials = new NetworkCredential(SmtpUser, SmtpPass),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 30000
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(SmtpUser, "SOM-PMS | Rezervasyon Sistemi"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Hata mesajını kullanıcıya göster (Hata ayıklama için)
                System.Windows.Forms.MessageBox.Show($"E-posta Gönderim Hatası: {ex.Message}\nLütfen internet bağlantınızı kontrol edin.", "E-posta Hatası", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                System.Diagnostics.Debug.WriteLine("Email sending failed: " + ex.Message);
            }
        }

        public static string GetConfirmationTemplate(string name, string room, int bed, int floor, string checkIn, string checkOut, decimal price)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; padding: 20px; border-radius: 10px;'>
                    <h2 style='color: #2c3e50; text-align: center;'>Rezervasyonunuz Onaylandı! 🎉</h2>
                    <p>Sayın <strong>{name}</strong>,</p>
                    <p>Online rezervasyon talebiniz ekibimiz tarafından incelenmiş ve onaylanmıştır. Konaklama detaylarınız aşağıdadır:</p>
                    <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; line-height: 1.6;'>
                        <p><strong>🏨 Oda No:</strong> {room}</p>
                        <p><strong>🛏️ Yatak No:</strong> {bed}</p>
                        <p><strong>🏢 Kat:</strong> {floor}</p>
                        <p><strong>📅 Giriş Tarihi:</strong> {checkIn}</p>
                        <p><strong>📅 Çıkış Tarihi:</strong> {checkOut}</p>
                        <p><strong>💰 Gecelik Fiyat:</strong> {price:N2} ₺</p>
                    </div>
                    <p style='text-align: center; margin-top: 20px;'>Sizi ağırlamak için sabırsızlanıyoruz!</p>
                    <hr style='border: none; border-top: 1px solid #eee;'>
                    <p style='font-size: 12px; color: #7f8c8d; text-align: center;'>SOM-PMS Pansiyon Yönetim Sistemi</p>
                </div>";
        }

        public static string GetWelcomeTemplate(string name, string room, int bed, int floor, string checkIn, string checkOut)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; padding: 20px; border-radius: 10px;'>
                    <h2 style='color: #27ae60; text-align: center;'>Hoş Geldiniz! ✨</h2>
                    <p>Sayın <strong>{name}</strong>,</p>
                    <p><strong>SOM-PMS</strong>'ye hoş geldiniz! Giriş işleminiz başarıyla tamamlanmıştır. Keyifli bir konaklama dileriz.</p>
                    <div style='background-color: #e8f5e9; padding: 15px; border-radius: 5px; line-height: 1.6;'>
                        <p><strong>🏨 Oda:</strong> {room} ({bed}. Yatak)</p>
                        <p><strong>🏢 Kat:</strong> {floor}</p>
                        <p><strong>📅 Konaklama:</strong> {checkIn} - {checkOut}</p>
                        <p><strong>📶 Wi-Fi Adı:</strong> sompansiyon</p>
                        <p><strong>🔑 Wi-Fi Şifre:</strong> Sompms1122</p>
                        <p><strong>🍴 Lokanta:</strong> 1. Katta hizmetinizdedir.</p>
                    </div>
                    <p style='text-align: center; margin-top: 20px;'>Herhangi bir ihtiyacınızda resepsiyona başvurabilirsiniz.</p>
                    <hr style='border: none; border-top: 1px solid #eee;'>
                    <p style='font-size: 12px; color: #7f8c8d; text-align: center;'>SOM-PMS Pansiyon Yönetim Sistemi</p>
                </div>";
        }
    }
}
