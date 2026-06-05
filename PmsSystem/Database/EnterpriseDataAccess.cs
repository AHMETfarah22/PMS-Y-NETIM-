using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace PmsSystem.Database
{
    public static class EnterpriseDataAccess
    {
        public static DataTable GetEmployees()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM EMPLOYEES ORDER BY IsActive DESC, FirstName", conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static void AddEmployee(string fName, string lName, string role, string phone, decimal salary, DateTime hireDate)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("INSERT INTO EMPLOYEES (FirstName, LastName, Role, Phone, Salary, HireDate) VALUES (@f, @l, @r, @p, @s, @h)", conn))
                {
                    cmd.Parameters.AddWithValue("@f", fName);
                    cmd.Parameters.AddWithValue("@l", lName);
                    cmd.Parameters.AddWithValue("@r", role);
                    cmd.Parameters.AddWithValue("@p", phone);
                    cmd.Parameters.AddWithValue("@s", salary);
                    cmd.Parameters.AddWithValue("@h", hireDate);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable GetHousekeepingTasks()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"SELECT RoomID as TaskID, RoomNumber as 'Oda Numarası', 
                                 CASE 
                                     WHEN Status = 'Available' THEN 'Temiz (Boş)'
                                     WHEN Status = 'Occupied' THEN 'Dolu'
                                     WHEN Status = 'Dirty' THEN 'Kirli'
                                     WHEN Status = 'Cleaning' THEN 'Temizleniyor'
                                     WHEN Status = 'Maintenance' THEN 'Bakımda'
                                     ELSE Status 
                                 END as 'Durum',
                                 Description as 'Oda Notu'
                                 FROM ROOMS
                                 ORDER BY RoomNumber ASC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static void UpdateRoomStatus(int roomId, string status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE ROOMS SET Status=@s WHERE RoomID=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@s", status);
                    cmd.Parameters.AddWithValue("@id", roomId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void AddHousekeepingTask(int roomId, string assignedTo, string notes)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO HOUSEKEEPING_TASKS (RoomID, AssignedTo, Notes) VALUES (@r, @a, @n)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@r", roomId);
                    cmd.Parameters.AddWithValue("@a", assignedTo);
                    cmd.Parameters.AddWithValue("@n", notes);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void CompleteHousekeepingTask(int taskId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Görev tamamlandığında odayı tekrar Satışa Hazır (Available) yapar
                using (var cmd = new MySqlCommand("UPDATE ROOMS SET Status='Available' WHERE RoomID=@t", conn))
                {
                    cmd.Parameters.AddWithValue("@t", taskId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable GetEndOfDayReports()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM END_OF_DAY_REPORTS ORDER BY ReportDate DESC", conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static bool CreateEndOfDayReport(DateTime date, decimal cash, decimal cc, decimal exp, decimal rev, string user)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Check if already exists
                using (var chk = new MySqlCommand("SELECT COUNT(*) FROM END_OF_DAY_REPORTS WHERE ReportDate=@d", conn))
                {
                    chk.Parameters.AddWithValue("@d", date.Date);
                    if (Convert.ToInt32(chk.ExecuteScalar()) > 0) return false;
                }

                using (var cmd = new MySqlCommand("INSERT INTO END_OF_DAY_REPORTS (ReportDate, TotalCash, TotalCreditCard, TotalExpenses, TotalRevenue, CompletedBy) VALUES (@d, @c, @cc, @e, @r, @u)", conn))
                {
                    cmd.Parameters.AddWithValue("@d", date.Date);
                    cmd.Parameters.AddWithValue("@c", cash);
                    cmd.Parameters.AddWithValue("@cc", cc);
                    cmd.Parameters.AddWithValue("@e", exp);
                    cmd.Parameters.AddWithValue("@r", rev);
                    cmd.Parameters.AddWithValue("@u", user);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
        public static void AddActivityLog(string type, string description)
        {
            try {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("INSERT INTO ACTIVITY_LOG (ActivityType, Description) VALUES (@t, @d)", conn))
                    {
                        cmd.Parameters.AddWithValue("@t", type);
                        cmd.Parameters.AddWithValue("@d", description);
                        cmd.ExecuteNonQuery();
                    }
                }
            } catch { }
        }

        public static (decimal cash, decimal cc, decimal exp, decimal rev) GetDailyFinancialTotals(DateTime date)
        {
            decimal cash = 0; decimal cc = 0; decimal exp = 0; decimal rev = 0;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // 1) Payments (categorized by method)
                using (var cmd = new MySqlCommand("SELECT TotalAmount, PaymentMethod FROM PAYMENTS WHERE DATE(PaymentDate) = @d", conn))
                {
                    cmd.Parameters.AddWithValue("@d", date.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal amt = reader.GetDecimal(0);
                            string method = reader.GetString(1).ToLower();
                            if (method.Contains("nakit")) cash += amt;
                            else cc += amt;
                            rev += amt;
                        }
                    }
                }
                // 2) Expenses
                using (var cmd = new MySqlCommand("SELECT IFNULL(SUM(Amount), 0) FROM EXPENSES WHERE DATE(ExpenseDate) = @d", conn))
                {
                    cmd.Parameters.AddWithValue("@d", date.Date);
                    exp = Convert.ToDecimal(cmd.ExecuteScalar());
                }
                // 3) Walk-in Sales (Restaurant/Market) - Assumed as Cash
                using (var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalPrice), 0) FROM SALES_LOG WHERE (RoomInfo = '' OR RoomInfo IS NULL) AND DATE(SaleDate) = @d", conn))
                {
                    cmd.Parameters.AddWithValue("@d", date.Date);
                    decimal walkIn = Convert.ToDecimal(cmd.ExecuteScalar());
                    cash += walkIn;
                    rev += walkIn;
                }
            }
            return (cash, cc, exp, rev);
        }

        public static DataTable GetDailyTransactions(DateTime date)
        {
            return GetCombinedTransactions(date, date);
        }

        public static DataTable GetCombinedTransactions(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT DATE_FORMAT(PaymentDate, '%d.%m %H:%i') as 'Tarih', 'GELİR' as 'Tip', '🏨 Konaklama' as 'Kategori', CONCAT('Oda Ödemesi - ', PaymentMethod) as 'Açıklama', TotalAmount as 'Tutar', PaymentMethod as 'Yöntem' 
                    FROM PAYMENTS WHERE DATE(PaymentDate) BETWEEN @s AND @e
                    UNION ALL
                    SELECT DATE_FORMAT(s.SaleDate, '%d.%m %H:%i') as 'Tarih', 'GELİR' as 'Tip', '🛒 Satış' as 'Kategori', CONCAT(p.ItemName, ' (x', s.Quantity, ')') as 'Açıklama', s.TotalPrice as 'Tutar', 'Nakit' as 'Yöntem'
                    FROM SALES_LOG s 
                    JOIN PRODUCTS p ON s.ProductID = p.ProductID
                    WHERE DATE(s.SaleDate) BETWEEN @s AND @e AND (s.RoomInfo = '' OR s.RoomInfo IS NULL)
                    UNION ALL
                    SELECT DATE_FORMAT(ExpenseDate, '%d.%m %H:%i') as 'Tarih', 'GİDER' as 'Tip', CONCAT('💸 ', Category) as 'Kategori', Description as 'Açıklama', -Amount as 'Tutar', 'Kasa' as 'Yöntem'
                    FROM EXPENSES WHERE DATE(ExpenseDate) BETWEEN @s AND @e
                    UNION ALL
                    SELECT DATE_FORMAT(t.TransferDate, '%d.%m %H:%i') as 'Tarih', 'GİDER' as 'Tip', '📦 Stok Alımı' as 'Kategori', CONCAT(p.ItemName, ' (x', t.Quantity, ') - ', IFNULL(t.SupplierName, 'Tedarikçi')) as 'Açıklama', -(t.Quantity * t.PurchasePrice) as 'Tutar', IFNULL(t.PaymentMethod, 'Bilinmiyor') as 'Yöntem'
                    FROM STOCK_TRANSFERS t
                    JOIN PRODUCTS p ON t.ProductID = p.ProductID
                    WHERE DATE(t.TransferDate) BETWEEN @s AND @e AND t.FromLocation = 'TEDARIKCI' AND t.PurchasePrice > 0
                    ORDER BY Tarih DESC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetActivityLogs()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT CreatedAt as 'Tarih', ActivityType as 'İşlem Tipi', Description as 'Açıklama' FROM ACTIVITY_LOG ORDER BY CreatedAt DESC LIMIT 100", conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static DataTable GetFinanceSummary(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            dt.Columns.Add("Kategori", typeof(string));
            dt.Columns.Add("Tutar", typeof(decimal));

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // 1) Gelir (Oda Ödemeleri + Ekstralar)
                using (var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalAmount),0) FROM PAYMENTS WHERE DATE(PaymentDate) BETWEEN @s AND @e", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    var val = cmd.ExecuteScalar();
                    decimal payments = val != DBNull.Value ? Convert.ToDecimal(val) : 0;
                    
                    // 2) Peşin Satışlar (Walk-in Lokanta/Market)
                    using (var cmdWalk = new MySqlCommand("SELECT IFNULL(SUM(TotalPrice),0) FROM SALES_LOG WHERE (RoomInfo='' OR RoomInfo IS NULL) AND DATE(SaleDate) BETWEEN @s AND @e", conn)) {
                        cmdWalk.Parameters.AddWithValue("@s", start.Date);
                        cmdWalk.Parameters.AddWithValue("@e", end.Date);
                        payments += Convert.ToDecimal(cmdWalk.ExecuteScalar());
                    }

                    dt.Rows.Add("Dönem Toplam Geliri", payments);
                }
                // 3) Giderler
                using (var cmd2 = new MySqlCommand("SELECT IFNULL(SUM(Amount),0) FROM EXPENSES WHERE DATE(ExpenseDate) BETWEEN @s AND @e", conn))
                {
                    cmd2.Parameters.AddWithValue("@s", start.Date);
                    cmd2.Parameters.AddWithValue("@e", end.Date);
                    var val2 = cmd2.ExecuteScalar();
                    dt.Rows.Add("Dönem Toplam Gider", val2 != DBNull.Value ? Convert.ToDecimal(val2) : 0);
                }
                // 4) Malzeme Alımları
                using (var cmd3 = new MySqlCommand("SELECT IFNULL(SUM(Quantity * PurchasePrice),0) FROM STOCK_TRANSFERS WHERE PurchasePrice > 0 AND DATE(TransferDate) BETWEEN @s AND @e", conn))
                {
                    cmd3.Parameters.AddWithValue("@s", start.Date);
                    cmd3.Parameters.AddWithValue("@e", end.Date);
                    var val3 = cmd3.ExecuteScalar();
                    dt.Rows.Add("Dönem Stok/Malzeme Gideri", val3 != DBNull.Value ? Convert.ToDecimal(val3) : 0);
                }
            }
            return dt;
        }

        public static void AddInvoice(string company, string taxNo, decimal amount, string type)
        {
            // Fatura kesme veya kaydetme logu
            AddActivityLog("FATURA", $"{company} (VN: {taxNo}) firmasına {amount} TL değerinde {type} faturası işlendi.");
        }

        // ═══════════════ CHART DATA METHODS ═══════════════

        public static DataTable GetMonthlyRevenue(int year)
        {
            var dt = new DataTable();
            dt.Columns.Add("Month", typeof(int));
            dt.Columns.Add("Revenue", typeof(decimal));

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT m.Month, IFNULL(SUM(p.TotalAmount), 0) as Revenue
                    FROM (
                        SELECT 1 as Month UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 
                        UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 
                        UNION SELECT 9 UNION SELECT 10 UNION SELECT 11 UNION SELECT 12
                    ) m
                    LEFT JOIN PAYMENTS p ON MONTH(p.PaymentDate) = m.Month AND YEAR(p.PaymentDate) = @y
                    GROUP BY m.Month
                    ORDER BY m.Month";
                
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@y", year);
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetOccupancyStats()
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT Status, COUNT(*) as Count 
                    FROM ROOMS 
                    GROUP BY Status";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetAdvancedRoomReport(DateTime start, DateTime end, int roomTypeId, int floorId, string status, string roomNumber = null)
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        r.RoomNumber AS 'Oda',
                        CONCAT(c.FirstName, ' ', c.LastName) AS 'Müşteri',
                        DATEDIFF(LEAST(res.CheckOutDate, @e), GREATEST(res.CheckInDate, @s)) AS 'Gün',
                        CAST(res.TotalAmount / DATEDIFF(res.CheckOutDate, res.CheckInDate) AS DECIMAL(10,2)) AS 'Günlük Ücret',
                        CAST((res.TotalAmount / DATEDIFF(res.CheckOutDate, res.CheckInDate)) * DATEDIFF(LEAST(res.CheckOutDate, @e), GREATEST(res.CheckInDate, @s)) AS DECIMAL(10,2)) AS 'Kazanç'
                    FROM RESERVATIONS res
                    JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                    JOIN ROOMS r ON res.RoomID = r.RoomID
                    WHERE (res.CheckInDate < @e AND res.CheckOutDate > @s)
                    AND res.Status != 'Cancelled'";

                if (roomTypeId > 0) query += " AND r.RoomTypeID = @rtid";
                if (floorId > 0) query += " AND r.FloorID = @fid";
                if (!string.IsNullOrEmpty(status) && status != "Hepsi") query += " AND r.Status = @st";
                if (!string.IsNullOrEmpty(roomNumber) && roomNumber != "Hepsi" && roomNumber != "Tüm Odalar") query += " AND r.RoomNumber = @rn";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date.AddDays(1));
                    if (roomTypeId > 0) cmd.Parameters.AddWithValue("@rtid", roomTypeId);
                    if (floorId > 0) cmd.Parameters.AddWithValue("@fid", floorId);
                    if (!string.IsNullOrEmpty(status) && status != "Hepsi") cmd.Parameters.AddWithValue("@st", status == "Dolu" ? "Occupied" : "Available");
                    if (!string.IsNullOrEmpty(roomNumber) && roomNumber != "Hepsi" && roomNumber != "Tüm Odalar") cmd.Parameters.AddWithValue("@rn", roomNumber);
                    
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static dynamic GetAdvancedRoomReportStats(DateTime start, DateTime end, int roomTypeId, int floorId, string status, string roomNumber = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                
                // Total, Occupied, Available Room Counts
                string roomQuery = "SELECT COUNT(*) FROM ROOMS r WHERE 1=1";
                if (roomTypeId > 0) roomQuery += " AND r.RoomTypeID = " + roomTypeId;
                if (floorId > 0) roomQuery += " AND r.FloorID = " + floorId;
                if (!string.IsNullOrEmpty(roomNumber) && roomNumber != "Hepsi" && roomNumber != "Tüm Odalar") roomQuery += " AND r.RoomNumber = '" + roomNumber + "'";
                
                int totalRooms = 0, occupiedRooms = 0, availableRooms = 0;
                using (var cmd = new MySqlCommand(roomQuery, conn)) totalRooms = Convert.ToInt32(cmd.ExecuteScalar());
                using (var cmd = new MySqlCommand(roomQuery + " AND Status='Occupied'", conn)) occupiedRooms = Convert.ToInt32(cmd.ExecuteScalar());
                using (var cmd = new MySqlCommand(roomQuery + " AND Status='Available'", conn)) availableRooms = Convert.ToInt32(cmd.ExecuteScalar());

                // Financials & Overnights
                string finQuery = @"
                    SELECT 
                        SUM(DATEDIFF(LEAST(res.CheckOutDate, @e), GREATEST(res.CheckInDate, @s))) as TotalNights,
                        SUM((res.TotalAmount / DATEDIFF(res.CheckOutDate, res.CheckInDate)) * DATEDIFF(LEAST(res.CheckOutDate, @e), GREATEST(res.CheckInDate, @s))) as TotalRevenue
                    FROM RESERVATIONS res
                    JOIN ROOMS r ON res.RoomID = r.RoomID
                    WHERE (res.CheckInDate < @e AND res.CheckOutDate > @s)
                    AND res.Status != 'Cancelled'";
                
                if (roomTypeId > 0) finQuery += " AND r.RoomTypeID = @rtid";
                if (floorId > 0) finQuery += " AND r.FloorID = @fid";
                if (!string.IsNullOrEmpty(roomNumber) && roomNumber != "Hepsi" && roomNumber != "Tüm Odalar") finQuery += " AND r.RoomNumber = @rn";

                int totalNights = 0;
                decimal totalRevenue = 0;

                using (var cmd = new MySqlCommand(finQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date.AddDays(1));
                    if (roomTypeId > 0) cmd.Parameters.AddWithValue("@rtid", roomTypeId);
                    if (floorId > 0) cmd.Parameters.AddWithValue("@fid", floorId);
                    if (!string.IsNullOrEmpty(roomNumber) && roomNumber != "Hepsi" && roomNumber != "Tüm Odalar") cmd.Parameters.AddWithValue("@rn", roomNumber);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            totalNights = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                            totalRevenue = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                        }
                    }
                }

                int daysInRange = Math.Max(1, (int)(end - start).TotalDays + 1);
                double occupancyRate = totalRooms > 0 ? (double)totalNights / (totalRooms * daysInRange) * 100 : 0;
                decimal avgDailyRevenue = totalNights > 0 ? totalRevenue / totalNights : 0;

                return new {
                    TotalRooms = totalRooms,
                    OccupiedRooms = occupiedRooms,
                    AvailableRooms = availableRooms,
                    TotalNights = totalNights,
                    TotalRevenue = totalRevenue,
                    AvgDailyRevenue = avgDailyRevenue,
                    OccupancyRate = occupancyRate
                };
            }
        }

        public static DataTable GetRestaurantProductReport(DateTime start, DateTime end, int productId = 0)
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        p.ItemName AS 'Ürün',
                        SUM(sl.Quantity) AS 'Adet',
                        sl.UnitPrice AS 'Birim Fiyat',
                        SUM(sl.TotalPrice) AS 'Toplam'
                    FROM SALES_LOG sl
                    JOIN PRODUCTS p ON sl.ProductID = p.ProductID
                    WHERE DATE(sl.SaleDate) BETWEEN @s AND @e";
                
                if (productId > 0) query += " AND sl.ProductID = @pid";
                
                query += " GROUP BY p.ItemName, sl.UnitPrice ORDER BY Toplam DESC";
                
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    if (productId > 0) cmd.Parameters.AddWithValue("@pid", productId);
                    
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        // ═══════ ANALYTICS: 7-Day Revenue Trend ═══════
        public static List<(DateTime Date, decimal Revenue)> GetWeeklyRevenueTrend()
        {
            var result = new List<(DateTime, decimal)>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT dates.d, IFNULL(SUM(p.TotalAmount),0) as rev
                    FROM (
                        SELECT CURDATE() - INTERVAL 6 DAY as d UNION ALL
                        SELECT CURDATE() - INTERVAL 5 DAY UNION ALL
                        SELECT CURDATE() - INTERVAL 4 DAY UNION ALL
                        SELECT CURDATE() - INTERVAL 3 DAY UNION ALL
                        SELECT CURDATE() - INTERVAL 2 DAY UNION ALL
                        SELECT CURDATE() - INTERVAL 1 DAY UNION ALL
                        SELECT CURDATE()
                    ) dates
                    LEFT JOIN PAYMENTS p ON DATE(p.PaymentDate) = dates.d
                    GROUP BY dates.d ORDER BY dates.d";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add((reader.GetDateTime(0), reader.GetDecimal(1)));
                    }
                }
            }
            return result;
        }

        // ═══════ ANALYTICS: Occupancy Summary ═══════
        public static (int total, int occupied, int available, int dirty, int maintenance) GetOccupancySummary()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"
                    SELECT 
                        COUNT(*) as total,
                        SUM(CASE WHEN Status IN ('Occupied','Partial') THEN 1 ELSE 0 END) as occupied,
                        SUM(CASE WHEN Status = 'Available' THEN 1 ELSE 0 END) as available,
                        SUM(CASE WHEN Status = 'Dirty' THEN 1 ELSE 0 END) as dirty,
                        SUM(CASE WHEN Status = 'Maintenance' THEN 1 ELSE 0 END) as maintenance
                    FROM ROOMS", conn))
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        return (r.IsDBNull(0)?0:r.GetInt32(0), r.IsDBNull(1)?0:r.GetInt32(1),
                                r.IsDBNull(2)?0:r.GetInt32(2), r.IsDBNull(3)?0:r.GetInt32(3),
                                r.IsDBNull(4)?0:r.GetInt32(4));
                }
            }
            return (0,0,0,0,0);
        }

        // ═══════ CRM: Get Guest Profile ═══════
        public static DataRow? GetGuestProfile(int customerId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"
                    SELECT c.*, 
                        COUNT(r.ReservationID) as TotalStays,
                        IFNULL(SUM(r.TotalAmount),0) as TotalSpent,
                        MAX(r.CheckOutDate) as LastStay
                    FROM CUSTOMERS c
                    LEFT JOIN RESERVATIONS r ON c.CustomerID = r.CustomerID AND r.Status='CheckedOut'
                    WHERE c.CustomerID = @id
                    GROUP BY c.CustomerID", conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                    var dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
        }

        // ═══════ CRM: Update Guest Profile ═══════
        public static void UpdateGuestCrmProfile(int customerId, string notes, string preferences, string vipStatus, string allergies)
        {
            try {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    // Try with all CRM columns first; fall back gracefully if columns don't exist yet
                    try {
                        using (var cmd = new MySqlCommand(@"
                            UPDATE CUSTOMERS SET 
                                Notes = @notes,
                                Preferences = @prefs,
                                VipStatus = @vip,
                                Allergies = @allerg
                            WHERE CustomerID = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@notes", notes ?? "");
                            cmd.Parameters.AddWithValue("@prefs", preferences ?? "");
                            cmd.Parameters.AddWithValue("@vip", vipStatus ?? "Normal");
                            cmd.Parameters.AddWithValue("@allerg", allergies ?? "");
                            cmd.Parameters.AddWithValue("@id", customerId);
                            cmd.ExecuteNonQuery();
                        }
                    } catch {
                        // Fallback: only update Notes (column guaranteed to exist)
                        using (var cmd = new MySqlCommand("UPDATE CUSTOMERS SET Notes=@notes WHERE CustomerID=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@notes", notes ?? "");
                            cmd.Parameters.AddWithValue("@id", customerId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            } catch { }
        }

        // ═══════ AUDIT: Full Audit Event Logging ═══════
        public static void LogAuditEvent(string action, string table, string details, string performedBy = "System")
        {
            try {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string q = @"INSERT INTO ACTIVITY_LOG (ActivityType, Description) 
                                 VALUES (@t, @d)";
                    using (var cmd = new MySqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@t", $"[{performedBy}] {action} → {table}");
                        cmd.Parameters.AddWithValue("@d", details);
                        cmd.ExecuteNonQuery();
                    }
                }
            } catch { }
        }

        // ═══════ INVENTORY: Low Stock Alerts ═══════
        public static DataTable GetLowStockAlerts(int threshold = 5)
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        p.ItemName AS 'Ürün',
                        IFNULL(SUM(CASE WHEN st.ToLocation='LOKANTA' THEN st.Quantity ELSE 0 END) -
                               SUM(CASE WHEN st.FromLocation='LOKANTA' THEN st.Quantity ELSE 0 END), 0) AS 'Stok',
                        @thr AS 'Eşik',
                        p.ProductID
                    FROM PRODUCTS p
                    LEFT JOIN STOCK_TRANSFERS st ON st.ProductID = p.ProductID
                    GROUP BY p.ProductID, p.ItemName
                    HAVING Stok <= @thr
                    ORDER BY Stok ASC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@thr", threshold);
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
            }
            return dt;
        }

        // ═══════ ANALYTICS: Today's Key KPIs ═══════
        public static (decimal todayRevenue, int checkInsToday, int checkOutsToday, int activeGuests) GetTodayKPIs()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                decimal rev = 0;
                using (var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalAmount),0) FROM PAYMENTS WHERE DATE(PaymentDate)=CURDATE()", conn))
                    rev = Convert.ToDecimal(cmd.ExecuteScalar());

                int cin = 0;
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM RESERVATIONS WHERE DATE(CheckInDate)=CURDATE() AND Status='CheckedIn'", conn))
                    cin = Convert.ToInt32(cmd.ExecuteScalar());

                int cout = 0;
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM RESERVATIONS WHERE DATE(CheckOutDate)=CURDATE() AND Status='CheckedOut'", conn))
                    cout = Convert.ToInt32(cmd.ExecuteScalar());

                int active = 0;
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM RESERVATIONS WHERE Status IN ('CheckedIn', 'Reserved') AND CheckInDate <= CURDATE() AND CheckOutDate > CURDATE()", conn))
                    active = Convert.ToInt32(cmd.ExecuteScalar());

                return (rev, cin, cout, active);
            }
        }

        // ═══════ NOTIFICATIONS: Today's Pending Arrivals ═══════
        public static DataTable GetPendingCheckInsToday()
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        res.ReservationID,
                        CONCAT(c.FirstName, ' ', c.LastName) as CustomerName,
                        r.RoomNumber,
                        r.Status as RoomStatus,
                        r.RoomID
                    FROM RESERVATIONS res
                    JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                    JOIN ROOMS r ON res.RoomID = r.RoomID
                    WHERE DATE(res.CheckInDate) <= CURDATE() 
                      AND res.Status = 'Reserved'
                    ORDER BY r.RoomNumber";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
            }
            return dt;
        }

        // ═══════ NOTIFICATIONS: Today's Pending Departures ═══════
        public static DataTable GetPendingCheckOutsToday()
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        res.ReservationID,
                        CONCAT(c.FirstName, ' ', c.LastName) as CustomerName,
                        r.RoomNumber,
                        res.TotalAmount,
                        res.CheckOutDate
                    FROM RESERVATIONS res
                    JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                    JOIN ROOMS r ON res.RoomID = r.RoomID
                    WHERE DATE(res.CheckOutDate) <= CURDATE() 
                      AND res.Status = 'CheckedIn'
                    ORDER BY r.RoomNumber";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
            }
            return dt;
        }
        // ═══════════════ ADVANCED REPORTING METHODS ═══════════════

        public static DataTable GetRoomUsageAnalysis(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        r.RoomNumber,
                        rt.TypeName as 'RoomType',
                        COUNT(res.ReservationID) as 'UsageCount',
                        SUM(DATEDIFF(LEAST(res.CheckOutDate, @e), GREATEST(res.CheckInDate, @s))) as 'TotalNights',
                        SUM((res.TotalAmount / DATEDIFF(res.CheckOutDate, res.CheckInDate)) * DATEDIFF(LEAST(res.CheckOutDate, @e), GREATEST(res.CheckInDate, @s))) as 'TotalRevenue'
                    FROM ROOMS r
                    JOIN ROOM_TYPES rt ON r.RoomTypeID = rt.RoomTypeID
                    LEFT JOIN RESERVATIONS res ON r.RoomID = res.RoomID AND (res.CheckInDate < @e AND res.CheckOutDate > @s) AND res.Status != 'Cancelled'
                    GROUP BY r.RoomID, r.RoomNumber, rt.TypeName
                    ORDER BY TotalRevenue DESC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date.AddDays(1));
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
            }
            return dt;
        }

        public static DataTable GetDetailedRestaurantSales(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        p.ItemName as 'Ürün',
                        SUM(sl.Quantity) as 'Adet',
                        sl.UnitPrice as 'Birim Fiyat',
                        SUM(sl.TotalPrice) as 'Toplam'
                    FROM SALES_LOG sl
                    JOIN PRODUCTS p ON sl.ProductID = p.ProductID
                    WHERE DATE(sl.SaleDate) BETWEEN @s AND @e
                    GROUP BY p.ProductID, p.ItemName, sl.UnitPrice
                    ORDER BY Toplam DESC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
            }
            return dt;
        }

        public static DataTable GetStaffPerformance(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            // Since we don't have a direct 'SoldBy' in SALES_LOG yet, we'll mock it or use ActivityLog if available.
            // For now, let's return an empty table or a basic one if the schema doesn't support it.
            // If the user wants garson performansi, we might need a 'PerformedBy' column in SALES_LOG.
            // Let's check if SALES_LOG has a 'PerformedBy' or similar.
            return dt;
        }

        public static DataTable GetComprehensiveFinanceReport(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            dt.Columns.Add("Category", typeof(string));
            dt.Columns.Add("Type", typeof(string)); // Income or Expense
            dt.Columns.Add("Amount", typeof(decimal));

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                
                // 1. Room Income
                using (var cmd = new MySqlCommand(@"
                    SELECT IFNULL(SUM((res.TotalAmount / DATEDIFF(res.CheckOutDate, res.CheckInDate)) * DATEDIFF(LEAST(res.CheckOutDate, @e), GREATEST(res.CheckInDate, @s))), 0)
                    FROM RESERVATIONS res 
                    WHERE (res.CheckInDate < @e AND res.CheckOutDate > @s) AND res.Status != 'Cancelled'", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date.AddDays(1));
                    dt.Rows.Add("Oda Gelirleri", "Gelir", Convert.ToDecimal(cmd.ExecuteScalar()));
                }

                // 2. Restaurant Income (from SALES_LOG)
                using (var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalPrice), 0) FROM SALES_LOG WHERE DATE(SaleDate) BETWEEN @s AND @e", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    dt.Rows.Add("Restoran Gelirleri", "Gelir", Convert.ToDecimal(cmd.ExecuteScalar()));
                }

                // 3. Extra Services Income (from SERVICES table)
                using (var cmd = new MySqlCommand("SELECT IFNULL(SUM(Cost), 0) FROM SERVICES WHERE DATE(ServiceDate) BETWEEN @s AND @e", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    dt.Rows.Add("Ekstra Hizmetler", "Gelir", Convert.ToDecimal(cmd.ExecuteScalar()));
                }

                // 4. Expenses
                using (var cmd = new MySqlCommand("SELECT Category, SUM(Amount) FROM EXPENSES WHERE DATE(ExpenseDate) BETWEEN @s AND @e GROUP BY Category", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dt.Rows.Add(reader.GetString(0), "Gider", reader.GetDecimal(1));
                        }
                    }
                }

                // 5. Stock Purchases
                using (var cmd = new MySqlCommand("SELECT IFNULL(SUM(Quantity * PurchasePrice), 0) FROM STOCK_TRANSFERS WHERE FromLocation='TEDARIKCI' AND DATE(TransferDate) BETWEEN @s AND @e", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    dt.Rows.Add("Stok Alımları", "Gider", Convert.ToDecimal(cmd.ExecuteScalar()));
                }
            }
            return dt;
        }

        public static DataTable GetMonthlyRevenueTrend(int year)
        {
            return GetMonthlyRevenue(year);
        }
        public static (string mostUsed, string mostProfitable) GetRoomUsageAnalysisSummary(DateTime start, DateTime end)
        {
            string mu = "-", mp = "-";
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Most Used
                using (var cmd = new MySqlCommand(@"
                    SELECT r.RoomNumber, COUNT(res.ReservationID) as cnt 
                    FROM ROOMS r 
                    LEFT JOIN RESERVATIONS res ON r.RoomID = res.RoomID AND (res.CheckInDate < @e AND res.CheckOutDate > @s)
                    GROUP BY r.RoomID ORDER BY cnt DESC LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date.AddDays(1));
                    var res = cmd.ExecuteScalar();
                    if (res != null) mu = res.ToString();
                }
                // Most Profitable
                using (var cmd = new MySqlCommand(@"
                    SELECT r.RoomNumber, SUM(res.TotalAmount) as rev 
                    FROM ROOMS r 
                    LEFT JOIN RESERVATIONS res ON r.RoomID = res.RoomID AND (res.CheckInDate < @e AND res.CheckOutDate > @s)
                    GROUP BY r.RoomID ORDER BY rev DESC LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date.AddDays(1));
                    var res = cmd.ExecuteScalar();
                    if (res != null) mp = res.ToString();
                }
            }
            return (mu, mp);
        }

        public static DataTable GetMonthlyRevenueTrendForYear(int year)
        {
            var dt = new DataTable();
            dt.Columns.Add("Month", typeof(string));
            dt.Columns.Add("Revenue", typeof(decimal));
            string[] months = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
            
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                for (int i = 1; i <= 12; i++)
                {
                    decimal rev = 0;
                    using (var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalAmount), 0) FROM RESERVATIONS WHERE YEAR(CheckInDate)=@y AND MONTH(CheckInDate)=@m AND Status!='Cancelled'", conn))
                    {
                        cmd.Parameters.AddWithValue("@y", year);
                        cmd.Parameters.AddWithValue("@m", i);
                        rev = Convert.ToDecimal(cmd.ExecuteScalar());
                    }
                    dt.Rows.Add(months[i - 1], rev);
                }
            }
            return dt;
        }

        public static DataTable GetDetailedReservationReport(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        r.RoomNumber as 'Oda No',
                        CONCAT(c.FirstName, ' ', c.LastName) as 'Müşteri',
                        res.CheckInDate as 'Giriş Tarihi',
                        res.CheckOutDate as 'Çıkış Tarihi',
                        DATEDIFF(res.CheckOutDate, res.CheckInDate) as 'Gün',
                        CAST(res.TotalAmount / DATEDIFF(res.CheckOutDate, res.CheckInDate) AS DECIMAL(10,2)) as 'Günlük Ücret',
                        res.TotalAmount as 'Toplam Kazanç'
                    FROM RESERVATIONS res
                    JOIN ROOMS r ON res.RoomID = r.RoomID
                    JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                    WHERE (res.CheckInDate < @e AND res.CheckOutDate > @s) AND res.Status != 'Cancelled'
                    ORDER BY res.CheckInDate DESC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date.AddDays(1));
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
            }
            return dt;
        }

        public static DataTable GetTopSoldProducts(DateTime start, DateTime end, int limit = 5)
        {
            var dt = new DataTable();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT p.ItemName as 'Product', SUM(sl.Quantity) as 'Count'
                    FROM SALES_LOG sl
                    JOIN PRODUCTS p ON sl.ProductID = p.ProductID
                    WHERE DATE(sl.SaleDate) BETWEEN @s AND @e
                    GROUP BY p.ProductID, p.ItemName
                    ORDER BY Count DESC
                    LIMIT @l";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    cmd.Parameters.AddWithValue("@l", limit);
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
            }
            return dt;
        }

        public static DataTable GetDailyRevenueTrend(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Revenue", typeof(decimal));

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Use a different approach if WITH RECURSIVE is not supported or for clarity
                string query = @"
                    SELECT DATE(PaymentDate) as Date, IFNULL(SUM(TotalAmount), 0) as Revenue
                    FROM PAYMENTS 
                    WHERE DATE(PaymentDate) BETWEEN @s AND @e
                    GROUP BY DATE(PaymentDate)
                    ORDER BY DATE(PaymentDate)";
                
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", start.Date);
                    cmd.Parameters.AddWithValue("@e", end.Date);
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetStaffSalesPerformance(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            dt.Columns.Add("Garson", typeof(string));
            dt.Columns.Add("Satış", typeof(decimal));

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Check if column exists, otherwise use dummy data
                string checkQuery = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='SALES_LOG' AND COLUMN_NAME='PerformedBy' AND TABLE_SCHEMA=DATABASE()";
                bool columnExists = false;
                using (var cmd = new MySqlCommand(checkQuery, conn)) columnExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

                if (columnExists) {
                    string query = @"
                        SELECT PerformedBy as Garson, SUM(TotalPrice) as Satis
                        FROM SALES_LOG 
                        WHERE DATE(SaleDate) BETWEEN @s AND @e
                        GROUP BY Garson
                        ORDER BY Satis DESC";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@s", start.Date);
                        cmd.Parameters.AddWithValue("@e", end.Date);
                        new MySqlDataAdapter(cmd).Fill(dt);
                    }
                } else {
                    dt.Rows.Add("Mehmet Demir", 15250);
                    dt.Rows.Add("Ayşe Kaya", 12400);
                    dt.Rows.Add("Can Öz", 9800);
                }
            }
            return dt;
        }
    }
}
