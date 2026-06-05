using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PmsApi.Database;
using PmsApi.Models;
using PmsApi.Helpers;
using System.Data;

namespace PmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(ILogger<ReservationController> logger)
        {
            _logger = logger;
        }

        [HttpGet("available-rooms")]
        [ProducesResponseType(typeof(List<RoomResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RoomResponse>>> GetAvailableRooms([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start.Date < DateTime.Today) return BadRequest("Geçmiş tarihli rezervasyon yapılamaz.");
            if (end.Date <= start.Date) return BadRequest("Çıkış tarihi giriş tarihinden sonra olmalıdır.");

            var availableRooms = new List<RoomResponse>();
            
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    // Improved query to get room details and check occupancy
                    var query = @"
                        SELECT 
                            r.RoomNumber, 
                            rt.TypeName, 
                            rt.BasePrice, 
                            r.Capacity,
                            (SELECT COUNT(*) FROM RESERVATIONS res 
                             WHERE res.RoomID = r.RoomID 
                             AND res.Status IN ('CheckedIn', 'Reserved', 'Pending')
                             AND (res.CheckInDate < @co AND res.CheckOutDate > @ci)) as OccupiedCount,
                            rt.Description
                        FROM ROOMS r 
                        LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID = rt.RoomTypeID
                        WHERE r.Status != 'Maintenance'
                        HAVING OccupiedCount < r.Capacity
                        ORDER BY r.RoomNumber";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ci", start.Date);
                        cmd.Parameters.AddWithValue("@co", end.Date);
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                availableRooms.Add(new RoomResponse
                                {
                                    RoomNumber = reader.GetString("RoomNumber"),
                                    RoomType = reader.IsDBNull(reader.GetOrdinal("TypeName")) ? "Standart" : reader.GetString("TypeName"),
                                    Price = reader.GetDecimal("BasePrice"),
                                    TotalCapacity = reader.GetInt32("Capacity"),
                                    AvailableBedsCount = reader.GetInt32("Capacity") - reader.GetInt32("OccupiedCount"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                                    Amenities = new List<string> { "Wi-Fi", "Klima", "TV", "Mini Bar" } // Default amenities
                                });
                            }
                        }
                    }
                }
                return Ok(availableRooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available rooms");
                return StatusCode(500, "Sunucu hatası: Odalar listelenirken bir sorun oluştu.");
            }
        }

        [HttpGet("available-beds/{roomNumber}")]
        public async Task<ActionResult<List<int>>> GetAvailableBeds(string roomNumber, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    // 1) Get room capacity and ID
                    int capacity = 0;
                    int roomId = 0;
                    using (var cmdRoom = new MySqlCommand("SELECT RoomID, Capacity FROM ROOMS WHERE RoomNumber = @rn", conn))
                    {
                        cmdRoom.Parameters.AddWithValue("@rn", roomNumber);
                        using (var reader = await cmdRoom.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync()) return NotFound("Oda bulunamadı.");
                            roomId = reader.GetInt32(0);
                            capacity = reader.GetInt32(1);
                        }
                    }

                    // 2) Get occupied beds
                    var occupiedBeds = new List<int>();
                    using (var cmdOcc = new MySqlCommand(@"
                        SELECT BedNumber FROM RESERVATIONS 
                        WHERE RoomID = @rid 
                        AND Status IN ('CheckedIn', 'Reserved', 'Pending')
                        AND (CheckInDate < @co AND CheckOutDate > @ci)", conn))
                    {
                        cmdOcc.Parameters.AddWithValue("@rid", roomId);
                        cmdOcc.Parameters.AddWithValue("@ci", start.Date);
                        cmdOcc.Parameters.AddWithValue("@co", end.Date);
                        using (var reader = await cmdOcc.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync()) occupiedBeds.Add(reader.GetInt32(0));
                        }
                    }

                    // 3) Calculate available
                    var availableBeds = Enumerable.Range(1, capacity).Except(occupiedBeds).ToList();
                    return Ok(availableBeds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available beds for room {RoomNumber}", roomNumber);
                return StatusCode(500, "Yatak bilgisi alınamadı.");
            }
        }

        [HttpPost("book")]
        public async Task<ActionResult<BookingResult>> BookRoom([FromBody] OnlineBookingRequest request)
        {
            if (request == null) return BadRequest("Geçersiz istek.");
            if (string.IsNullOrEmpty(request.IdentityNumber)) return BadRequest("TC Kimlik numarası gereklidir.");

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    using (var tr = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            // NOT: Race condition önleme
                            // Müsaitlik kontrolü transaction İÇİNDE yapılmalı. Aksi halde iki kullanıcı aynı anda
                            // boş görüp aynı yatağı kaydedebilir. Burada oda satırını FOR UPDATE ile kilitleyip
                            // ardından çakışan rezervasyon var mı kontrol ediyoruz.

                            // 1) Find or Create Customer
                            int customerId = 0;
                            using (var cmdCust = new MySqlCommand("SELECT CustomerID FROM CUSTOMERS WHERE IdentityNumber = @id", conn, tr))
                            {
                                cmdCust.Parameters.AddWithValue("@id", request.IdentityNumber);
                                var result = await cmdCust.ExecuteScalarAsync();
                                if (result != null)
                                {
                                    customerId = Convert.ToInt32(result);
                                }
                                else
                                {
                                    using (var cmdIns = new MySqlCommand(@"
                                        INSERT INTO CUSTOMERS (FirstName, LastName, IdentityNumber, Phone, Email) 
                                        VALUES (@f, @l, @idn, @p, @e); 
                                        SELECT LAST_INSERT_ID();", conn, tr))
                                    {
                                        cmdIns.Parameters.AddWithValue("@f", request.FirstName);
                                        cmdIns.Parameters.AddWithValue("@l", request.LastName);
                                        cmdIns.Parameters.AddWithValue("@idn", request.IdentityNumber);
                                        cmdIns.Parameters.AddWithValue("@p", request.Phone);
                                        cmdIns.Parameters.AddWithValue("@e", request.Email);
                                        customerId = Convert.ToInt32(await cmdIns.ExecuteScalarAsync());
                                    }
                                }
                            }

                            // 2) Get Room ID and Price (lock the room row to prevent concurrent booking for same room)
                            int roomId = 0;
                            decimal price = 0;
                            using (var cmdRoom = new MySqlCommand(@"
                                SELECT RoomID, 
                                IFNULL((SELECT BasePrice FROM ROOM_TYPES rt WHERE rt.RoomTypeID = r.RoomTypeID), 0) as Price 
                                FROM ROOMS r WHERE RoomNumber = @rn
                                FOR UPDATE", conn, tr))
                            {
                                cmdRoom.Parameters.AddWithValue("@rn", request.RoomNumber);
                                using (var reader = await cmdRoom.ExecuteReaderAsync())
                                {
                                    if (!await reader.ReadAsync()) return BadRequest("Oda bulunamadı.");
                                    roomId = reader.GetInt32(0);
                                    price = reader.GetDecimal(1);
                                }
                            }

                            // 3) Availability check INSIDE transaction (lock matching rows if they exist)
                            // If there is any overlapping reservation for the same bed, reject.
                            using (var cmdChk = new MySqlCommand(@"
                                SELECT 1
                                FROM RESERVATIONS
                                WHERE RoomID = @rid
                                  AND BedNumber = @bn
                                  AND Status IN ('CheckedIn', 'Reserved', 'Pending')
                                  AND (CheckInDate < @co AND CheckOutDate > @ci)
                                LIMIT 1
                                FOR UPDATE;", conn, tr))
                            {
                                cmdChk.Parameters.AddWithValue("@rid", roomId);
                                cmdChk.Parameters.AddWithValue("@bn", request.BedNumber);
                                cmdChk.Parameters.AddWithValue("@ci", request.CheckInDate.Date);
                                cmdChk.Parameters.AddWithValue("@co", request.CheckOutDate.Date);

                                var exists = await cmdChk.ExecuteScalarAsync();
                                if (exists != null)
                                    return BadRequest(new BookingResult { Success = false, Message = "Seçilen yatak bu tarihler arasında artık müsait değil." });
                            }

                            // 3) Create Reservation
                            int nights = Math.Max(1, (request.CheckOutDate.Date - request.CheckInDate.Date).Days);
                            string reservationCode = "RSV-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                            using (var cmdRes = new MySqlCommand(@"
                                INSERT INTO RESERVATIONS 
                                (CustomerID, RoomID, BedNumber, CheckInDate, CheckOutDate, Status, TotalAmount, ChannelName, IsOnline, Notes) 
                                VALUES (@cid, @rid, @bn, @ci, @co, 'Pending', @amt, 'Web API', 1, @n)", conn, tr))
                            {
                                cmdRes.Parameters.AddWithValue("@cid", customerId);
                                cmdRes.Parameters.AddWithValue("@rid", roomId);
                                cmdRes.Parameters.AddWithValue("@bn", request.BedNumber);
                                cmdRes.Parameters.AddWithValue("@ci", request.CheckInDate.Date);
                                cmdRes.Parameters.AddWithValue("@co", request.CheckOutDate.Date);
                                cmdRes.Parameters.AddWithValue("@amt", price * nights);
                                cmdRes.Parameters.AddWithValue("@n", string.IsNullOrEmpty(request.Notes) ? $"Online Rezervasyon ({reservationCode})" : $"{request.Notes} ({reservationCode})");
                                
                                await cmdRes.ExecuteNonQueryAsync();
                            }

                            await tr.CommitAsync();

                            // 4) Send Confirmation Email
                            if (!string.IsNullOrEmpty(request.Email))
                            {
                                _ = Task.Run(() => EmailHelper.SendReservationReceivedEmailAsync(
                                    request.Email,
                                    $"{request.FirstName} {request.LastName}",
                                    request.RoomNumber,
                                    request.BedNumber,
                                    price,
                                    reservationCode));
                            }

                            return Ok(new BookingResult { Success = true, Message = "Rezervasyon talebiniz başarıyla oluşturuldu.", ReservationCode = reservationCode });
                        }
                        catch (Exception ex)
                        {
                            await tr.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during booking process");
                return StatusCode(500, "Rezervasyon sırasında teknik bir hata oluştu.");
            }
        }

        [HttpGet("status/{reservationCode}")]
        public async Task<ActionResult> GetStatus(string reservationCode)
        {
            if (string.IsNullOrEmpty(reservationCode)) return BadRequest("Rezervasyon kodu gereklidir.");

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    var query = @"
                        SELECT r.Status, r.CheckInDate, r.CheckOutDate, r.TotalAmount, r.PaidAmount, c.FirstName, c.LastName
                        FROM RESERVATIONS r
                        JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                        WHERE r.Notes LIKE @code"; // Assuming Notes contains the reservationCode (e.g. "Online Rezervasyon (RSV-12345678)")

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", $"%({reservationCode})%");
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Ok(new
                                {
                                    GuestName = $"{reader["FirstName"]} {reader["LastName"]}",
                                    CheckInDate = Convert.ToDateTime(reader["CheckInDate"]).ToString("yyyy-MM-dd"),
                                    CheckOutDate = Convert.ToDateTime(reader["CheckOutDate"]).ToString("yyyy-MM-dd"),
                                    Status = reader["Status"].ToString(),
                                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                    PaidAmount = Convert.ToDecimal(reader["PaidAmount"])
                                });
                            }
                            else
                            {
                                return NotFound("Rezervasyon bulunamadı.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reservation status");
                return StatusCode(500, "Sunucu hatası oluştu.");
            }
        }

        [HttpPost("cancel/{reservationCode}")]
        public async Task<ActionResult> CancelReservation(string reservationCode)
        {
            if (string.IsNullOrEmpty(reservationCode)) return BadRequest("Rezervasyon kodu gereklidir.");

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    var query = @"
                        UPDATE RESERVATIONS 
                        SET Status = 'Cancelled' 
                        WHERE Notes LIKE @code AND Status = 'Pending'"; 

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", $"%({reservationCode})%");
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { Success = true, Message = "Rezervasyon başarıyla iptal edildi." });
                        }
                        else
                        {
                            return BadRequest("Rezervasyon bulunamadı veya iptal edilemez (zaten onaylanmış/iptal edilmiş olabilir).");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling reservation");
                return StatusCode(500, "Sunucu hatası oluştu.");
            }
        }
    }
}
