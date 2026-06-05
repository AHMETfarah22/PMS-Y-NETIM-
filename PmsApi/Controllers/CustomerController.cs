using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PmsApi.Database;
using PmsApi.Models;

namespace PmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ILogger<CustomerController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Customer>>> GetAllCustomers()
        {
            var customers = new List<Customer>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                
                var query = @"
                    SELECT CustomerID, IdentityNumber, FirstName, LastName, Email, Phone, Address, Notes, CreatedAt 
                    FROM CUSTOMERS 
                    ORDER BY CreatedAt DESC";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    customers.Add(new Customer
                    {
                        CustomerID = Convert.ToInt32(reader["CustomerID"]),
                        IdentityNumber = reader["IdentityNumber"] == DBNull.Value ? null : reader["IdentityNumber"].ToString(),
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                        Phone = reader["Phone"] == DBNull.Value ? null : reader["Phone"].ToString(),
                        Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString(),
                        Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                    });
                }
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customers");
                return StatusCode(500, "Sunucu hatası: Müşteriler listelenirken bir sorun oluştu.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                Customer? customer = null;

                // 1. Get Customer details
                using (var cmd = new MySqlCommand("SELECT * FROM CUSTOMERS WHERE CustomerID = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        customer = new Customer
                        {
                            CustomerID = Convert.ToInt32(reader["CustomerID"]),
                            IdentityNumber = reader["IdentityNumber"] == DBNull.Value ? null : reader["IdentityNumber"].ToString(),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                            Phone = reader["Phone"] == DBNull.Value ? null : reader["Phone"].ToString(),
                            Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString(),
                            Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        };
                    }
                }

                if (customer == null) return NotFound("Müşteri bulunamadı.");

                // 2. Get Messages
                using (var cmd = new MySqlCommand("SELECT * FROM CUSTOMER_MESSAGES WHERE CustomerID = @id ORDER BY CreatedAt ASC", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        customer.Messages.Add(new CustomerMessage
                        {
                            MessageID = Convert.ToInt32(reader["MessageID"]),
                            CustomerID = Convert.ToInt32(reader["CustomerID"]),
                            MessageText = reader["MessageText"].ToString(),
                            Direction = reader["Direction"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }

                // 3. Get Reservations
                using (var cmd = new MySqlCommand(@"
                    SELECT r.ReservationID, rm.RoomNumber, r.CheckInDate, r.CheckOutDate, r.Status, r.TotalAmount 
                    FROM RESERVATIONS r 
                    LEFT JOIN ROOMS rm ON r.RoomID = rm.RoomID 
                    WHERE r.CustomerID = @id 
                    ORDER BY r.CheckInDate DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        customer.Reservations.Add(new CustomerReservation
                        {
                            ReservationID = Convert.ToInt32(reader["ReservationID"]),
                            RoomNumber = reader["RoomNumber"] == DBNull.Value ? "-" : reader["RoomNumber"].ToString(),
                            CheckInDate = Convert.ToDateTime(reader["CheckInDate"]),
                            CheckOutDate = Convert.ToDateTime(reader["CheckOutDate"]),
                            Status = reader["Status"].ToString(),
                            TotalAmount = reader["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalAmount"])
                        });
                    }
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer details");
                return StatusCode(500, "Sunucu hatası: Müşteri detayları alınırken bir sorun oluştu.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCustomer(int id, [FromBody] Customer updateReq)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                var query = @"
                    UPDATE CUSTOMERS 
                    SET FirstName = @fn, LastName = @ln, Email = @e, Phone = @p, Address = @a, Notes = @n 
                    WHERE CustomerID = @id";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@fn", updateReq.FirstName);
                cmd.Parameters.AddWithValue("@ln", updateReq.LastName);
                cmd.Parameters.AddWithValue("@e", (object?)updateReq.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p", (object?)updateReq.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@a", (object?)updateReq.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@n", (object?)updateReq.Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", id);

                var affected = await cmd.ExecuteNonQueryAsync();
                if (affected == 0) return NotFound("Müşteri bulunamadı.");

                return Ok(new { Success = true, Message = "Müşteri güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer");
                return StatusCode(500, "Sunucu hatası: Müşteri güncellenirken bir sorun oluştu.");
            }
        }

        [HttpPost("{id}/messages")]
        public async Task<ActionResult> AddMessage(int id, [FromBody] CustomerMessage messageReq)
        {
            if (string.IsNullOrWhiteSpace(messageReq.MessageText)) return BadRequest("Mesaj boş olamaz.");

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                var query = @"
                    INSERT INTO CUSTOMER_MESSAGES (CustomerID, MessageText, Direction) 
                    VALUES (@cid, @txt, @dir)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cid", id);
                cmd.Parameters.AddWithValue("@txt", messageReq.MessageText);
                cmd.Parameters.AddWithValue("@dir", messageReq.Direction ?? "Incoming");

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { Success = true, Message = "Mesaj eklendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding message");
                return StatusCode(500, "Sunucu hatası: Mesaj eklenirken bir sorun oluştu.");
            }
        }
    }
}
