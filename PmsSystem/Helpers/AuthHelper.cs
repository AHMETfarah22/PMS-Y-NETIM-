using MySql.Data.MySqlClient;
using PmsSystem.Database;
using PmsSystem.Models;

namespace PmsSystem.Helpers
{
    public static class AuthHelper //veritabanı ile formlar arasındaki iletişimi, SQL sorgularını ve oturum yönetimini (Session) üstlenir.
    {
        public static User? CurrentUser { get; private set; }

        public static bool Login(string username, string password, out string message)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "SELECT * FROM USERS WHERE Username=@u AND PasswordHash=@p AND IsActive=1", conn);
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            CurrentUser = new User
                            {
                                UserID = Convert.ToInt32(reader["UserID"]),
                                Username = reader["Username"].ToString()!,
                                FullName = reader["FullName"].ToString()!,
                                Email = reader["Email"]?.ToString() ?? "",
                                Role = reader["Role"].ToString()!
                            };
                            message = $"Hoş geldiniz, {CurrentUser.FullName}!";
                            return true;
                        }
                    }
                }
                message = "Hatalı kullanıcı adı veya şifre!";
                return false;
            }
            catch (Exception ex)
            {
                message = "Hata: " + ex.Message;
                return false;
            }
        }

        public static bool Register(string username, string fullName, string email,
                                    string password, string phone, out string message)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand(@"INSERT INTO USERS 
                        (Username, FullName, Email, PasswordHash, PhoneNumber, Role) 
                        VALUES (@u, @n, @e, @p, @ph, 'Kasiyer')", conn);
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@n", fullName);
                    cmd.Parameters.AddWithValue("@e", email);
                    cmd.Parameters.AddWithValue("@p", password);
                    cmd.Parameters.AddWithValue("@ph", phone);

                    int rows = cmd.ExecuteNonQuery();
                    message = rows > 0 ? "Kayıt başarılı! Giriş yapabilirsiniz." : "Kayıt başarısız.";
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.Contains("Duplicate")
                    ? "Bu kullanıcı adı zaten kullanılıyor!"
                    : "Hata: " + ex.Message;
                return false;
            }
        }

        public static void Logout() => CurrentUser = null;
    }
}

//AuthHelper sınıfının neden kullanıldığını ve tam olarak ne işe yaradığını madde madde açıklıyorum:
//1. Kodun Tekrarını Önlemek (DRY - Don't Repeat Yourself)
//
//Giriş yapma (Login) ve Kayıt olma (Register) işlemleri temelinde veritabanı sorgularıdır. Eğer bu kodları her formun içine ayrı ayrı yazsaydık:
//Hem LoginForm hem de RegisterForm içinde veritabanı bağlantı kodlarını kopyalamak zorunda kalırdık.
//Bir hata olduğunda veya veritabanı yapısı değiştiğinde her iki formu da tek tek güncellememiz gerekirdi. 
//AuthHelper
//tüm bu mantığı tek bir merkezde toplar.