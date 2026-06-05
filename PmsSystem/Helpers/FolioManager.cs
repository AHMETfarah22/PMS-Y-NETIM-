using System;
using System.Data;
using MySql.Data.MySqlClient;
using PmsSystem.Database;

namespace PmsSystem.Helpers
{
    public static class FolioManager
    {
        public class FolioItem
        {
            public int ItemID { get; set; }
            public string Description { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Category { get; set; } = "Konaklama"; // Konaklama, Restoran, Market, Diger
            public string Owner { get; set; } = "Misafir"; // Misafir, Sirket
            public DateTime DateAdded { get; set; } = DateTime.Now;
        }

        // 1. SPLIT FOLIO: Rezervasyon ödemesini Şirket ve Şahıs olarak veya parçalı ödeme yöntemleriyle yapma
        public static bool ProcessSplitPayment(int reservationId, decimal cashAmount, decimal cardAmount, decimal bankTransferAmount, string guestOwner, string companyOwner, out string message)
        {
            decimal totalPaid = cashAmount + cardAmount + bankTransferAmount;
            if (totalPaid <= 0)
            {
                message = "Ödeme tutarı 0'dan büyük olmalıdır.";
                return false;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var tr = conn.BeginTransaction())
                    {
                        try
                        {
                            // Ayrı ayrı ödeme yöntemleri için PAYMENTS tablosuna kayıtlar ekleyelim
                            if (cashAmount > 0)
                            {
                                string sql = "INSERT INTO PAYMENTS (ReservationID, TotalAmount, PaymentMethod, PaymentDate) VALUES (@rid, @amt, 'Nakit', NOW())";
                                using (var cmd = new MySqlCommand(sql, conn, tr))
                                {
                                    cmd.Parameters.AddWithValue("@rid", reservationId);
                                    cmd.Parameters.AddWithValue("@amt", cashAmount);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            if (cardAmount > 0)
                            {
                                string sql = "INSERT INTO PAYMENTS (ReservationID, TotalAmount, PaymentMethod, PaymentDate) VALUES (@rid, @amt, 'Kredi Kartı', NOW())";
                                using (var cmd = new MySqlCommand(sql, conn, tr))
                                {
                                    cmd.Parameters.AddWithValue("@rid", reservationId);
                                    cmd.Parameters.AddWithValue("@amt", cardAmount);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            if (bankTransferAmount > 0)
                            {
                                string sql = "INSERT INTO PAYMENTS (ReservationID, TotalAmount, PaymentMethod, PaymentDate) VALUES (@rid, @amt, 'Havale/EFT', NOW())";
                                using (var cmd = new MySqlCommand(sql, conn, tr))
                                {
                                    cmd.Parameters.AddWithValue("@rid", reservationId);
                                    cmd.Parameters.AddWithValue("@amt", bankTransferAmount);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // RESERVATIONS tablosundaki PaidAmount değerini güncelle
                            string sqlUpdate = "UPDATE RESERVATIONS SET PaidAmount = PaidAmount + @totalPaid WHERE ReservationID = @rid";
                            using (var cmd = new MySqlCommand(sqlUpdate, conn, tr))
                            {
                                cmd.Parameters.AddWithValue("@totalPaid", totalPaid);
                                cmd.Parameters.AddWithValue("@rid", reservationId);
                                cmd.ExecuteNonQuery();
                            }

                            // Opsiyonel: Eğer borç sıfırlanırsa Rezervasyon durumunu güncelle
                            string sqlCheck = "SELECT TotalAmount, ExtraAmount, PaidAmount FROM RESERVATIONS WHERE ReservationID = @rid";
                            decimal totalAmount = 0, extraAmount = 0, paidAmount = 0;
                            using (var cmd = new MySqlCommand(sqlCheck, conn, tr))
                            {
                                cmd.Parameters.AddWithValue("@rid", reservationId);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        totalAmount = reader.GetDecimal(0);
                                        extraAmount = reader.GetDecimal(1);
                                        paidAmount = reader.GetDecimal(2);
                                    }
                                }
                            }

                            if (paidAmount >= (totalAmount + extraAmount))
                            {
                                string sqlStatus = "UPDATE RESERVATIONS SET Status = 'CheckedOut' WHERE ReservationID = @rid AND Status = 'CheckedIn'";
                                using (var cmd = new MySqlCommand(sqlStatus, conn, tr))
                                {
                                    cmd.Parameters.AddWithValue("@rid", reservationId);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            tr.Commit();
                            message = "Parçalı ödeme başarıyla işlendi.";
                            return true;
                        }
                        catch (Exception ex)
                        {
                            tr.Rollback();
                            message = $"Hata oluştu: {ex.Message}";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"Veritabanı bağlantı hatası: {ex.Message}";
                return false;
            }
        }

        // 2. ERKEN GİRİŞ / GEÇ ÇIKIŞ OTOMASYONU: Saatlik bazda ekstra ücret yansıtma
        public static bool CheckAndApplyEarlyOrLateFees(int reservationId, DateTime actualTime, out string logMessage)
        {
            logMessage = string.Empty;
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    DateTime checkInDate = DateTime.MinValue;
                    DateTime checkOutDate = DateTime.MinValue;
                    string status = string.Empty;
                    decimal basePrice = 0;

                    // Rezervasyon bilgilerini çek
                    string query = @"
                        SELECT res.CheckInDate, res.CheckOutDate, res.Status, 
                               IFNULL((SELECT BasePrice FROM ROOM_TYPES rt JOIN ROOMS r ON r.RoomTypeID = rt.RoomTypeID WHERE r.RoomID = res.RoomID), 0) as RoomPrice
                        FROM RESERVATIONS res
                        WHERE res.ReservationID = @rid";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@rid", reservationId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                checkInDate = reader.GetDateTime(0);
                                checkOutDate = reader.GetDateTime(1);
                                status = reader.GetString(2);
                                basePrice = reader.GetDecimal(3);
                            }
                        }
                    }

                    if (checkInDate == DateTime.MinValue) return false;

                    // Standart Kurallar: Check-in saati 14:00, Check-out saati 12:00 olsun.
                    // Erken check-in: Giriş günü saat 10:00'dan önceyse günlük fiyatın %50'si, 10:00-14:00 arasıysa %25'i eklenir.
                    // Geç check-out: Çıkış günü saat 12:00-15:00 arasıysa günlük fiyatın %25'i, 15:00-18:00 arasıysa %50'si, 18:00'den sonraysa %100'ü (tam gün) eklenir.

                    decimal extraCharge = 0;
                    string description = string.Empty;

                    if (status == "Pending" || status == "Reserved") // Giriş esnasında
                    {
                        if (actualTime.Date == checkInDate.Date)
                        {
                            int hour = actualTime.Hour;
                            if (hour < 10)
                            {
                                extraCharge = basePrice * 0.50m;
                                description = $"Erken Giriş Ücreti (Saat {hour:D2}:00 - %50 Oda Bedeli)";
                            }
                            else if (hour >= 10 && hour < 14)
                            {
                                extraCharge = basePrice * 0.25m;
                                description = $"Erken Giriş Ücreti (Saat {hour:D2}:00 - %25 Oda Bedeli)";
                            }
                        }
                    }
                    else if (status == "CheckedIn") // Çıkış esnasında
                    {
                        if (actualTime.Date == checkOutDate.Date)
                        {
                            int hour = actualTime.Hour;
                            if (hour >= 12 && hour < 15)
                            {
                                extraCharge = basePrice * 0.25m;
                                description = $"Geç Çıkış Ücreti (Saat {hour:D2}:00 - %25 Oda Bedeli)";
                            }
                            else if (hour >= 15 && hour < 18)
                            {
                                extraCharge = basePrice * 0.50m;
                                description = $"Geç Çıkış Ücreti (Saat {hour:D2}:00 - %50 Oda Bedeli)";
                            }
                            else if (hour >= 18)
                            {
                                extraCharge = basePrice * 1.00m;
                                description = $"Geç Çıkış Ücreti (Saat {hour:D2}:00 - Tam Gün Oda Bedeli)";
                            }
                        }
                    }

                    if (extraCharge > 0)
                    {
                        // Ekstra tutarı veritabanına ekle
                        string sqlUpdate = @"
                            UPDATE RESERVATIONS 
                            SET ExtraAmount = ExtraAmount + @charge,
                                Notes = CONCAT(IFNULL(Notes,''), '\n', @desc)
                            WHERE ReservationID = @rid;

                            INSERT INTO SERVICES (ReservationID, ServiceName, Cost, Description)
                            VALUES (@rid, @desc, @charge, @desc);
                        ";

                        using (var cmdUpdate = new MySqlCommand(sqlUpdate, conn))
                        {
                            cmdUpdate.Parameters.AddWithValue("@charge", extraCharge);
                            cmdUpdate.Parameters.AddWithValue("@desc", description);
                            cmdUpdate.Parameters.AddWithValue("@rid", reservationId);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        logMessage = $"{description} uygulandı: {extraCharge:C2}";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage = $"Hata: {ex.Message}";
            }

            return false;
        }
    }
}
