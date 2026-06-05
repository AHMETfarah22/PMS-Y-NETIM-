using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PmsApi.Database;

namespace PmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(ILogger<PaymentController> logger)
        {
            _logger = logger;
        }

        public class PaymentInitRequest
        {
            public string ReservationCode { get; set; }
            public string CardHolderName { get; set; }
            public string CardNumber { get; set; }
            public string ExpireMonth { get; set; }
            public string ExpireYear { get; set; }
            public string Cvc { get; set; }
        }

        [HttpPost("initiate")]
        public async Task<ActionResult> InitiatePayment([FromBody] PaymentInitRequest request)
        {
            // Bu metod Iyzico veya PayTR gibi bir sanal POS sağlayıcısına istek atmak için bir mock'tur.
            if (string.IsNullOrEmpty(request.ReservationCode) || string.IsNullOrEmpty(request.CardNumber))
            {
                return BadRequest("Eksik ödeme bilgisi.");
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    // 1. Rezervasyonu bul ve tutarını al
                    decimal amountToPay = 0;
                    int reservationId = 0;
                    
                    var query = "SELECT ReservationID, TotalAmount, PaidAmount FROM RESERVATIONS WHERE Notes LIKE @code AND Status = 'Pending'";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", $"%({request.ReservationCode})%");
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                reservationId = Convert.ToInt32(reader["ReservationID"]);
                                amountToPay = Convert.ToDecimal(reader["TotalAmount"]) - Convert.ToDecimal(reader["PaidAmount"]);
                            }
                            else
                            {
                                return NotFound("Ödenecek rezervasyon bulunamadı veya zaten ödenmiş.");
                            }
                        }
                    }

                    if (amountToPay <= 0) return BadRequest("Bu rezervasyon için ödenecek tutar bulunmuyor.");

                    // 2. Sanal POS İsteği (Mock)
                    // Normalde burada Iyzico Options oluşturulur ve Create() çağrılır.
                    bool paymentSuccess = SimulateVirtualPos(request, amountToPay);

                    if (paymentSuccess)
                    {
                        // 3. Ödeme başarılıysa veritabanını güncelle
                        var updateQuery = @"
                            UPDATE RESERVATIONS 
                            SET PaidAmount = PaidAmount + @amount, 
                                Status = 'Reserved' 
                            WHERE ReservationID = @resId;

                            INSERT INTO PAYMENTS (ReservationID, TotalAmount, RoomAmount, PaymentMethod, PaymentDate)
                            VALUES (@resId, @amount, @amount, 'Online Kredi Kartı', NOW());
                        ";

                        using (var cmdUpdate = new MySqlCommand(updateQuery, conn))
                        {
                            cmdUpdate.Parameters.AddWithValue("@amount", amountToPay);
                            cmdUpdate.Parameters.AddWithValue("@resId", reservationId);
                            await cmdUpdate.ExecuteNonQueryAsync();
                        }

                        return Ok(new { Success = true, Message = "Ödeme başarıyla alındı. Rezervasyonunuz onaylandı." });
                    }
                    else
                    {
                        return BadRequest(new { Success = false, Message = "Ödeme reddedildi. Lütfen kart bilgilerinizi kontrol edin." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment error");
                return StatusCode(500, "Ödeme işlemi sırasında bir hata oluştu.");
            }
        }

        private bool SimulateVirtualPos(PaymentInitRequest request, decimal amount)
        {
            // Basit Mock: Kart numarası '4' ile başlıyorsa (Visa) veya '5' ile başlıyorsa (Mastercard) başarılı sayalım.
            if (request.CardNumber.StartsWith("4") || request.CardNumber.StartsWith("5"))
            {
                return true;
            }
            return false;
        }

        [HttpPost("webhook/paytr")]
        public async Task<ActionResult> PayTRWebhook([FromForm] string merchant_oid, [FromForm] string status, [FromForm] string total_amount, [FromForm] string hash)
        {
            // PayTR veya benzeri sistemler asenkron olarak webhook (callback) gönderir.
            // merchant_oid genelde reservationCode'a eşittir.
            // Hash doğrulaması yapılmalıdır.
            
            _logger.LogInformation($"Webhook received from PayTR for OID: {merchant_oid}, Status: {status}");

            if (status == "success")
            {
                // Veritabanını güncelleme işlemleri burada yapılır (Initiate payment'in 3D Secure sonrası adımı gibi)
                return Ok("OK"); // PayTR 'OK' yanıtı bekler
            }

            return Ok("FAILED");
        }
    }
}
