using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PmsApi.Database;

namespace PmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChannelManagerController : ControllerBase
    {
        private readonly ILogger<ChannelManagerController> _logger;

        public ChannelManagerController(ILogger<ChannelManagerController> logger)
        {
            _logger = logger;
        }

        public class OtaReservationPayload
        {
            public string OtaReservationId { get; set; }
            public string ChannelName { get; set; } // e.g., Booking.com, Airbnb
            public string GuestFirstName { get; set; }
            public string GuestLastName { get; set; }
            public string RoomType { get; set; }
            public DateTime CheckIn { get; set; }
            public DateTime CheckOut { get; set; }
            public decimal TotalPrice { get; set; }
            public decimal Commission { get; set; }
        }

        [HttpPost("webhook/reservation")]
        public async Task<ActionResult> ReceiveReservation([FromBody] OtaReservationPayload payload)
        {
            // Bu endpoint Channel Manager (HotelRunner, Octorate vb.) veya OTA'lerden doğrudan
            // (Booking.com XML API) gelen rezervasyon push bildirimlerini yakalar.
            
            _logger.LogInformation($"Received OTA Reservation from {payload.ChannelName} - ID: {payload.OtaReservationId}");

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    using (var tr = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            // 1. Müşteri bul veya yarat (Eksik detaylarla geçici bir kayıt atılır)
                            int customerId = 0;
                            using (var cmdCust = new MySqlCommand("INSERT INTO CUSTOMERS (FirstName, LastName, Phone) VALUES (@f, @l, '0000000000'); SELECT LAST_INSERT_ID();", conn, tr))
                            {
                                cmdCust.Parameters.AddWithValue("@f", payload.GuestFirstName);
                                cmdCust.Parameters.AddWithValue("@l", payload.GuestLastName);
                                customerId = Convert.ToInt32(await cmdCust.ExecuteScalarAsync());
                            }

                            // 2. Müsait oda bul (Belirtilen tipe göre)
                            int roomId = 0;
                            using (var cmdRoom = new MySqlCommand(@"
                                SELECT r.RoomID 
                                FROM ROOMS r 
                                LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID = rt.RoomTypeID
                                WHERE rt.TypeName LIKE @tname AND r.Status = 'Available' 
                                LIMIT 1", conn, tr))
                            {
                                cmdRoom.Parameters.AddWithValue("@tname", $"%{payload.RoomType}%");
                                var rId = await cmdRoom.ExecuteScalarAsync();
                                if (rId != null) roomId = Convert.ToInt32(rId);
                            }

                            if (roomId == 0)
                            {
                                // Overbooking durumu! Logla veya admin uyarısı gönder.
                                _logger.LogWarning("Overbooking detected from channel manager!");
                                // Gerçek bir senaryoda bu rezervasyon "Unassigned" (Odasız) olarak havuza atılır.
                            }

                            // 3. Rezervasyonu oluştur
                            using (var cmdRes = new MySqlCommand(@"
                                INSERT INTO RESERVATIONS 
                                (CustomerID, RoomID, CheckInDate, CheckOutDate, Status, TotalAmount, ChannelName, CommissionAmount, IsOnline, Notes) 
                                VALUES (@cid, @rid, @ci, @co, 'Reserved', @amt, @chan, @comm, 1, @notes)", conn, tr))
                            {
                                cmdRes.Parameters.AddWithValue("@cid", customerId);
                                cmdRes.Parameters.AddWithValue("@rid", roomId > 0 ? (object)roomId : DBNull.Value);
                                cmdRes.Parameters.AddWithValue("@ci", payload.CheckIn.Date);
                                cmdRes.Parameters.AddWithValue("@co", payload.CheckOut.Date);
                                cmdRes.Parameters.AddWithValue("@amt", payload.TotalPrice);
                                cmdRes.Parameters.AddWithValue("@chan", payload.ChannelName);
                                cmdRes.Parameters.AddWithValue("@comm", payload.Commission);
                                cmdRes.Parameters.AddWithValue("@notes", $"OTA Ref: {payload.OtaReservationId}");
                                
                                await cmdRes.ExecuteNonQueryAsync();
                            }

                            await tr.CommitAsync();
                            return Ok(new { Success = true, Message = "Rezervasyon sisteme işlendi." });
                        }
                        catch (Exception)
                        {
                            await tr.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OTA webhook");
                return StatusCode(500, "Webhook islenirken hata olustu.");
            }
        }
    }
}
