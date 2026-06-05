using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PmsApi.Database;

namespace PmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HousekeepingController : ControllerBase
    {
        private readonly ILogger<HousekeepingController> _logger;

        public HousekeepingController(ILogger<HousekeepingController> logger)
        {
            _logger = logger;
        }

        public class HousekeepingTaskResponse
        {
            public int TaskID { get; set; }
            public string RoomNumber { get; set; } = string.Empty;
            public string AssignedTo { get; set; } = string.Empty;
            public string TaskStatus { get; set; } = "Pending"; // Pending, InProgress, Completed
            public string TaskType { get; set; } = "Cleaning"; // Cleaning, DeepCleaning, Inspection
            public string Notes { get; set; } = string.Empty;
            public string CreatedAt { get; set; } = string.Empty;
        }

        [HttpGet("tasks")]
        public async Task<ActionResult<List<HousekeepingTaskResponse>>> GetTasks()
        {
            var tasks = new List<HousekeepingTaskResponse>();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    var query = @"
                        SELECT t.TaskID, r.RoomNumber, t.AssignedTo, t.TaskStatus, t.TaskType, t.Notes, t.CreatedAt
                        FROM HOUSEKEEPING_TASKS t
                        JOIN ROOMS r ON t.RoomID = r.RoomID
                        WHERE t.TaskStatus != 'Completed'
                        ORDER BY t.CreatedAt DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tasks.Add(new HousekeepingTaskResponse
                                {
                                    TaskID = Convert.ToInt32(reader["TaskID"]),
                                    RoomNumber = reader["RoomNumber"].ToString() ?? string.Empty,
                                    AssignedTo = reader["AssignedTo"].ToString() ?? string.Empty,
                                    TaskStatus = reader["TaskStatus"].ToString() ?? string.Empty,
                                    TaskType = reader["TaskType"].ToString() ?? string.Empty,
                                    Notes = reader["Notes"].ToString() ?? string.Empty,
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]).ToString("yyyy-MM-dd HH:mm")
                                });
                            }
                        }
                    }
                }
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching housekeeping tasks");
                return StatusCode(500, "Görevler listelenirken sunucu hatası oluştu.");
            }
        }

        [HttpPost("complete/{taskId}")]
        public async Task<ActionResult> CompleteTask(int taskId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    // 1. Görevi tamamlandı olarak işaretle
                    int roomId = 0;
                    string updateTaskQuery = @"
                        UPDATE HOUSEKEEPING_TASKS 
                        SET TaskStatus = 'Completed', CompletedAt = NOW() 
                        WHERE TaskID = @tid;
                        SELECT RoomID FROM HOUSEKEEPING_TASKS WHERE TaskID = @tid;";

                    using (var cmd = new MySqlCommand(updateTaskQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@tid", taskId);
                        var rId = await cmd.ExecuteScalarAsync();
                        if (rId != null) roomId = Convert.ToInt32(rId);
                    }

                    if (roomId == 0) return NotFound("Görev bulunamadı.");

                    // 2. Odanın durumunu 'Available' (Müsait/Temiz) olarak güncelle
                    string updateRoomQuery = "UPDATE ROOMS SET Status = 'Available' WHERE RoomID = @rid";
                    using (var cmdRoom = new MySqlCommand(updateRoomQuery, conn))
                    {
                        cmdRoom.Parameters.AddWithValue("@rid", roomId);
                        await cmdRoom.ExecuteNonQueryAsync();
                    }
                }
                return Ok(new { Success = true, Message = "Temizlik görevi tamamlandı, oda kullanıma hazır." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing housekeeping task");
                return StatusCode(500, "Görev güncellenirken hata oluştu.");
            }
        }

        public class UpdateRoomStatusRequest
        {
            public string RoomNumber { get; set; } = string.Empty;
            public string Status { get; set; } = "Available"; // Available (Temiz), Dirty (Kirli), Maintenance (Arızalı)
        }

        [HttpPost("room-status")]
        public async Task<ActionResult> UpdateRoomStatus([FromBody] UpdateRoomStatusRequest request)
        {
            if (string.IsNullOrEmpty(request.RoomNumber)) return BadRequest("Oda numarası gereklidir.");

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string query = "UPDATE ROOMS SET Status = @status WHERE RoomNumber = @rnum";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", request.Status);
                        cmd.Parameters.AddWithValue("@rnum", request.RoomNumber);
                        int rows = await cmd.ExecuteNonQueryAsync();
                        
                        if (rows > 0)
                        {
                            return Ok(new { Success = true, Message = $"Oda {request.RoomNumber} durumu '{request.Status}' olarak güncellendi." });
                        }
                        else
                        {
                            return NotFound("Oda bulunamadı.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room status");
                return StatusCode(500, "Oda durumu güncellenirken hata oluştu.");
            }
        }
    }
}
