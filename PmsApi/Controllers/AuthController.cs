using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using PmsApi.Database;
using PmsApi.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace PmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class LogoutRequest
        {
            public string Email { get; set; }
            public string FullName { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Kullanıcı adı ve şifre gereklidir.");

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    // Simple SHA256 hash check since PmsSystem uses SHA256 for passwords
                    string hash = ComputeSha256Hash(request.Password);

                    using (var cmd = new MySqlCommand("SELECT UserID, FullName, Role FROM USERS WHERE Username = @u AND PasswordHash = @p AND IsActive = 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@u", request.Username);
                        cmd.Parameters.AddWithValue("@p", hash);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int userId = reader.GetInt32(0);
                                string fullName = reader.GetString(1);
                                string role = reader.GetString(2);

                                var token = GenerateJwtToken(userId, request.Username, fullName, role);
                                return Ok(new { Token = token, FullName = fullName, Role = role });
                            }
                        }
                    }
                }
                
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Sunucu hatası: " + ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.FullName))
                return BadRequest("Email ve Ad-Soyad gereklidir.");

            try
            {
                // Müşteri çıkış emaili gönder
                var emailResult = await EmailHelper.SendLogoutEmailAsync(request.Email, request.FullName);
                
                if (emailResult.Success)
                    return Ok(new { Message = "Çıkış başarılı. Email gönderildi.", EmailStatus = "Gönderildi" });
                else
                    return Ok(new { Message = "Çıkış başarılı.", EmailStatus = "Email gönderilemedi", ErrorDetail = emailResult.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Çıkış işlemi başarısız", Error = ex.Message });
            }
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.FullName))
                return BadRequest("Email ve Ad-Soyad gereklidir.");

            try
            {
                var emailResult = await EmailHelper.SendLogoutEmailAsync(request.Email, request.FullName);
                return Ok(new { 
                    Success = emailResult.Success, 
                    Message = emailResult.Message,
                    TestTime = DateTime.Now,
                    ToEmail = request.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "Email test başarısız", 
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        private string GenerateJwtToken(int userId, string username, string fullName, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "BuCokGizliVeGuvenliBirPmsAnahtaridir12345!"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim("userId", userId.ToString()),
                new Claim("fullName", fullName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "PmsApi",
                audience: _config["Jwt:Audience"] ?? "PmsClients",
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
