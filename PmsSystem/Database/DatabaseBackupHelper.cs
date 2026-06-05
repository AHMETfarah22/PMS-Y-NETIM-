using System;
using System.IO;
using System.Data;
using MySql.Data.MySqlClient;
using System.Text;

namespace PmsSystem.Database
{
    public static class DatabaseBackupHelper
    {
        public static string BackupDatabase()
        {
            try
            {
                string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

                string fileName = $"PMS_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                string filePath = Path.Combine(backupFolder, fileName);

                StringBuilder sql = new StringBuilder();
                sql.AppendLine("-- PMS System Database Backup");
                sql.AppendLine($"-- Generated at: {DateTime.Now}");
                sql.AppendLine("SET NAMES utf8mb4;");
                sql.AppendLine("SET FOREIGN_KEY_CHECKS = 0;");
                sql.AppendLine();

                string[] tables = { "USERS", "FLOORS", "ROOM_TYPES", "ROOMS", "CUSTOMERS", "RESERVATIONS", "PRODUCTS", "STORAGE_LOG", "SALES_LOG", "PAYMENTS", "EXPENSES", "ACTIVITY_LOG", "END_OF_DAY" };

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    foreach (string table in tables)
                    {
                        sql.AppendLine($"-- Table: {table}");
                        sql.AppendLine($"DROP TABLE IF EXISTS `{table}`;");
                        
                        // Get Create Table
                        using (var cmd = new MySqlCommand($"SHOW CREATE TABLE `{table}`", conn))
                        {
                            using (var dr = cmd.ExecuteReader())
                            {
                                if (dr.Read())
                                {
                                    sql.AppendLine(dr[1].ToString() + ";");
                                }
                            }
                        }

                        // Get Data
                        DataTable dt = new DataTable();
                        using (var cmd = new MySqlCommand($"SELECT * FROM `{table}`", conn))
                        {
                            using (var da = new MySqlDataAdapter(cmd))
                            {
                                da.Fill(dt);
                            }
                        }

                        if (dt.Rows.Count > 0)
                        {
                            sql.AppendLine($"INSERT INTO `{table}` VALUES ");
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                sql.Append("(");
                                for (int j = 0; j < dt.Columns.Count; j++)
                                {
                                    object val = dt.Rows[i][j];
                                    if (val == DBNull.Value) sql.Append("NULL");
                                    else if (val is string || val is DateTime) sql.Append("'" + MySqlHelper.EscapeString(val.ToString()) + "'");
                                    else if (val is bool b) sql.Append(b ? "1" : "0");
                                    else sql.Append(val.ToString().Replace(",", "."));

                                    if (j < dt.Columns.Count - 1) sql.Append(", ");
                                }
                                sql.Append(")");
                                if (i < dt.Rows.Count - 1) sql.AppendLine(",");
                                else sql.AppendLine(";");
                            }
                        }
                        sql.AppendLine();
                    }
                    sql.AppendLine("SET FOREIGN_KEY_CHECKS = 1;");
                }

                File.WriteAllText(filePath, sql.ToString(), Encoding.UTF8);
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception("Yedekleme başarısız: " + ex.Message);
            }
        }
    }
}
