using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace PmsApi.Database
{
    public static class DatabaseHelper
    {
        private static string _connectionString;

        public static void Initialize(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(_connectionString))
            {
                var server = configuration["DatabaseConfig:Server"] ?? "127.0.0.1";
                var dbName = configuration["DatabaseConfig:Database"] ?? "pms_system";
                var user = configuration["DatabaseConfig:User"] ?? "root";
                var password = configuration["DatabaseConfig:Password"] ?? "";
                var port = configuration["DatabaseConfig:Port"] ?? "3306";

                _connectionString = $"Server={server};Port={port};Database={dbName};Uid={user};Pwd={password};Allow User Variables=true;CharSet=utf8mb4;Connection Timeout=10;";
            }

            RunMigrations();
        }

        private static void RunMigrations()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                using var cmd = new MySqlCommand(@"
                    CREATE TABLE IF NOT EXISTS CUSTOMER_MESSAGES (
                        MessageID INT AUTO_INCREMENT PRIMARY KEY,
                        CustomerID INT NOT NULL,
                        MessageText TEXT NOT NULL,
                        Direction VARCHAR(20) DEFAULT 'Incoming',
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (CustomerID) REFERENCES CUSTOMERS(CustomerID) ON DELETE CASCADE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
                ", conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                // Ignored for now
            }
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
