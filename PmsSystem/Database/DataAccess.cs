using MySql.Data.MySqlClient;
using System.Data;
using PmsSystem.Helpers;

namespace PmsSystem.Database
{
    public static class DataAccess
    {
        // ═══════ ROOMS ═══════
        public static DataTable GetAllRooms()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"SELECT r.RoomID, r.RoomNumber, r.Capacity, r.OccupiedBeds, r.Status, 
                IFNULL(rt.TypeName,'Standart') AS OdaTipi, IFNULL(rt.BasePrice,0) AS Fiyat,
                f.FloorNumber,
                (SELECT GROUP_CONCAT(CONCAT(FirstName, ' ', LastName) SEPARATOR ', ') 
                 FROM CUSTOMERS c 
                 JOIN RESERVATIONS res ON c.CustomerID = res.CustomerID 
                 WHERE res.RoomID = r.RoomID AND res.Status = 'CheckedIn') AS Musteriler,
                (SELECT MIN(CheckInDate) FROM RESERVATIONS res WHERE res.RoomID = r.RoomID AND res.Status = 'CheckedIn') as GirisTarihi,
                (SELECT MAX(CheckOutDate) FROM RESERVATIONS res WHERE res.RoomID = r.RoomID AND res.Status = 'CheckedIn') as CikisTarihi,
                IFNULL((SELECT SUM(res.TotalAmount) FROM RESERVATIONS res WHERE res.RoomID = r.RoomID AND res.Status = 'CheckedIn'), 0) AS ToplamTutar,
                IFNULL((SELECT DATEDIFF(MIN(CheckOutDate), MIN(CheckInDate)) FROM RESERVATIONS res WHERE res.RoomID = r.RoomID AND res.Status = 'CheckedIn'), 0) AS KalinanGun,
                (SELECT MIN(CheckInDate) FROM RESERVATIONS res WHERE res.RoomID = r.RoomID AND res.Status = 'Reserved' AND CheckInDate >= CURRENT_DATE) AS NextResDate
                FROM ROOMS r 
                LEFT JOIN FLOORS f ON r.FloorID = f.FloorID
                LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID=rt.RoomTypeID 
                ORDER BY f.FloorNumber, r.RoomNumber", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static async Task<DataTable> GetAllRoomsAsync() 
        {
            return await Task.Run(() => GetAllRooms());
        }

        public static DataTable GetAvailableRooms()
        {
            return GetAvailableRoomsForDates(DateTime.Today, DateTime.Today.AddDays(1));
        }

        public static DataTable GetAvailableRoomsForDates(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // Bir odanın o tarihlerdeki dolu yatak sayısını sayıp kapasiteden küçük olanları getiriyoruz
            using var cmd = new MySqlCommand(@"
                SELECT r.RoomNumber, f.FloorNumber, 
                       IFNULL(rt.TypeName,'Standart') as TypeName, 
                       IFNULL(rt.BasePrice,0) as Price
                FROM ROOMS r 
                JOIN FLOORS f ON r.FloorID = f.FloorID
                LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE (
                    SELECT COUNT(*) FROM RESERVATIONS res
                    WHERE res.RoomID = r.RoomID 
                    AND res.Status IN ('CheckedIn', 'Reserved')
                    AND (res.CheckInDate < @co AND res.CheckOutDate > @ci)
                ) < r.Capacity
                AND r.Status != 'Maintenance'
                ORDER BY r.RoomNumber", conn);
            cmd.Parameters.AddWithValue("@ci", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@co", end.ToString("yyyy-MM-dd"));
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void UpdateRoomOccupancy(string roomNumber, int newOccupied)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string status = newOccupied <= 0 ? "Available" : "Partial";

            // Check if fully occupied
            using var cmdCap = new MySqlCommand("SELECT Capacity FROM ROOMS WHERE RoomNumber=@r", conn);
            cmdCap.Parameters.AddWithValue("@r", roomNumber);
            int cap = Convert.ToInt32(cmdCap.ExecuteScalar());
            if (newOccupied >= cap) status = "Occupied";

            using var cmd = new MySqlCommand("UPDATE ROOMS SET OccupiedBeds=@o, Status=@s WHERE RoomNumber=@r", conn);
            cmd.Parameters.AddWithValue("@o", newOccupied);
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@r", roomNumber);
            cmd.ExecuteNonQuery();
        }

        public static int GetRoomCapacity(string roomNumber)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT Capacity FROM ROOMS WHERE RoomNumber=@r", conn);
            cmd.Parameters.AddWithValue("@r", roomNumber);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static int GetTotalRoomCount()
        {
            try {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new MySqlCommand("SELECT COUNT(*) FROM ROOMS", conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            } catch { return 0; }
        }

        // ═══════ CUSTOMERS ═══════
        public static int AddCustomer(string firstName, string lastName, string phone, string email, string roomNumber, int bedNumber, string address = "", string idNo = "", string fatherName = "", string motherName = "", string birthPlace = "", DateTime? birthDate = null, string gender = "Erkek", string nationality = "Türkiye")
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            if (!string.IsNullOrWhiteSpace(idNo)) {
                using var chk = new MySqlCommand("SELECT CustomerID FROM CUSTOMERS WHERE IdentityNumber=@idn", conn);
                chk.Parameters.AddWithValue("@idn", idNo);
                var existingId = chk.ExecuteScalar();
                if (existingId != null) {
                    using var upd = new MySqlCommand(@"UPDATE CUSTOMERS SET FirstName=@fn, LastName=@ln, Phone=@ph, Email=@em, RoomNumber=@rn, BedNumber=@bn, Address=@ad, FatherName=@fan, MotherName=@mon, BirthPlace=@bp, BirthDate=@bd, Gender=@gn, Nationality=@nat WHERE CustomerID=@cid", conn);
                    upd.Parameters.AddWithValue("@fn", firstName);
                    upd.Parameters.AddWithValue("@ln", lastName);
                    upd.Parameters.AddWithValue("@ph", phone);
                    upd.Parameters.AddWithValue("@em", email);
                    upd.Parameters.AddWithValue("@rn", roomNumber);
                    upd.Parameters.AddWithValue("@bn", bedNumber);
                    upd.Parameters.AddWithValue("@ad", address);
                    upd.Parameters.AddWithValue("@fan", fatherName);
                    upd.Parameters.AddWithValue("@mon", motherName);
                    upd.Parameters.AddWithValue("@bp", birthPlace);
                    upd.Parameters.AddWithValue("@bd", birthDate ?? (object)DBNull.Value);
                    upd.Parameters.AddWithValue("@gn", gender);
                    upd.Parameters.AddWithValue("@nat", nationality);
                    upd.Parameters.AddWithValue("@cid", existingId);
                    upd.ExecuteNonQuery();
                    return Convert.ToInt32(existingId);
                }
            }

            using var cmd = new MySqlCommand(@"INSERT INTO CUSTOMERS (FirstName, LastName, Phone, Email, RoomNumber, BedNumber, Address, IdentityNumber, FatherName, MotherName, BirthPlace, BirthDate, Gender, Nationality) 
                VALUES (@fn, @ln, @ph, @em, @rn, @bn, @ad, @idn, @fan, @mon, @bp, @bd, @gn, @nat); SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@fn", firstName);
            cmd.Parameters.AddWithValue("@ln", lastName);
            cmd.Parameters.AddWithValue("@ph", phone);
            cmd.Parameters.AddWithValue("@em", email);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            cmd.Parameters.AddWithValue("@bn", bedNumber);
            cmd.Parameters.AddWithValue("@ad", address);
            cmd.Parameters.AddWithValue("@idn", idNo);
            cmd.Parameters.AddWithValue("@fan", fatherName);
            cmd.Parameters.AddWithValue("@mon", motherName);
            cmd.Parameters.AddWithValue("@bp", birthPlace);
            cmd.Parameters.AddWithValue("@bd", birthDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@gn", gender);
            cmd.Parameters.AddWithValue("@nat", nationality);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static DataTable GetCustomerByID(int id)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM CUSTOMERS WHERE CustomerID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataRow? GetCustomerByIdentity(string idNo)
        {
            if (string.IsNullOrWhiteSpace(idNo)) return null;
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM CUSTOMERS WHERE IdentityNumber=@id LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@id", idNo);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static DataTable GetCustomersByIdentityPrefix(string prefix)
        {
            var dt = new DataTable();
            if (string.IsNullOrWhiteSpace(prefix)) return dt;
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM CUSTOMERS WHERE IdentityNumber LIKE @p LIMIT 10", conn);
            cmd.Parameters.AddWithValue("@p", prefix + "%");
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetCustomersByName(string fname, string lname)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM CUSTOMERS WHERE FirstName LIKE @fn AND LastName LIKE @ln LIMIT 5", conn);
            cmd.Parameters.AddWithValue("@fn", fname + "%");
            cmd.Parameters.AddWithValue("@ln", lname + "%");
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetCustomersByNamePrefix(string prefix)
        {
            var dt = new DataTable();
            if (string.IsNullOrWhiteSpace(prefix)) return dt;
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM CUSTOMERS WHERE FirstName LIKE @p OR LastName LIKE @p ORDER BY FirstName LIMIT 10", conn);
            cmd.Parameters.AddWithValue("@p", prefix + "%");
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void UpdateCustomer(string idNo, string firstName, string lastName, string phone, string email, string address, string fatherName = "", string motherName = "", string birthPlace = "", DateTime? birthDate = null, string gender = "Erkek", string nationality = "Türkiye")
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"UPDATE CUSTOMERS SET FirstName=@fn, LastName=@ln, Phone=@ph, Email=@em, Address=@ad, FatherName=@fan, MotherName=@mon, BirthPlace=@bp, BirthDate=@bd, Gender=@gn, Nationality=@nat WHERE IdentityNumber=@idn", conn);
            cmd.Parameters.AddWithValue("@fn", firstName);
            cmd.Parameters.AddWithValue("@ln", lastName);
            cmd.Parameters.AddWithValue("@ph", phone);
            cmd.Parameters.AddWithValue("@em", email);
            cmd.Parameters.AddWithValue("@ad", address);
            cmd.Parameters.AddWithValue("@fan", fatherName);
            cmd.Parameters.AddWithValue("@mon", motherName);
            cmd.Parameters.AddWithValue("@bp", birthPlace);
            cmd.Parameters.AddWithValue("@bd", birthDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@gn", gender);
            cmd.Parameters.AddWithValue("@nat", nationality);
            cmd.Parameters.AddWithValue("@idn", idNo);
            cmd.ExecuteNonQuery();
        }

        public static DataTable GetCustomerHistory(int customerId)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT r.CheckInDate, r.CheckOutDate, rm.RoomNumber, r.TotalAmount, r.Status
                FROM RESERVATIONS r
                JOIN ROOMS rm ON r.RoomID = rm.RoomID
                WHERE r.CustomerID = @id
                ORDER BY r.CheckInDate DESC
            ", conn);
            cmd.Parameters.AddWithValue("@id", customerId);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static decimal GetCustomerTotalSpent(int customerId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalAmount), 0) FROM RESERVATIONS WHERE CustomerID = @id AND Status != 'Cancelled'", conn);
            cmd.Parameters.AddWithValue("@id", customerId);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static void UpdateCustomerNotes(int customerId, string notes)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("UPDATE CUSTOMERS SET Notes = @n WHERE CustomerID = @id", conn);
            cmd.Parameters.AddWithValue("@n", notes);
            cmd.Parameters.AddWithValue("@id", customerId);
            cmd.ExecuteNonQuery();
        }

        public static DataTable GetDailyPoliceReport(DateTime date)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // Get all customers who are checked in on this date
            using var cmd = new MySqlCommand(@"
                SELECT c.*, r.CheckInDate, r.CheckOutDate, rm.RoomNumber
                FROM CUSTOMERS c
                JOIN RESERVATIONS r ON c.CustomerID = r.CustomerID
                JOIN ROOMS rm ON r.RoomID = rm.RoomID
                WHERE DATE(r.CheckInDate) <= DATE(@d) AND DATE(r.CheckOutDate) >= DATE(@d)
                AND r.Status != 'Cancelled'
            ", conn);
            cmd.Parameters.AddWithValue("@d", date);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static bool IsCustomerStaying(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber)) return false;
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM RESERVATIONS r
                JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                WHERE c.IdentityNumber = @idn AND r.Status = 'CheckedIn'
                AND @today >= r.CheckInDate AND @today < r.CheckOutDate", conn);
            cmd.Parameters.AddWithValue("@idn", identityNumber);
            cmd.Parameters.AddWithValue("@today", DateTime.Today.ToString("yyyy-MM-dd"));
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public static bool HasOverlappingReservation(string identityNumber, DateTime checkIn, DateTime checkOut)
        {
            if (string.IsNullOrWhiteSpace(identityNumber)) return false;
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM RESERVATIONS r
                JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                WHERE c.IdentityNumber = @idn 
                AND r.Status = 'CheckedIn'
                AND (r.CheckInDate < @co AND r.CheckOutDate > @ci)", conn);
            cmd.Parameters.AddWithValue("@idn", identityNumber);
            cmd.Parameters.AddWithValue("@ci", checkIn.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@co", checkOut.ToString("yyyy-MM-dd"));
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public static DataTable GetAllCustomers()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // JOIN with active reservations to get CURRENT room/bed. 
            // If no active reservation, it shows the values from the CUSTOMERS table itself (or 0).
            using var cmd = new MySqlCommand(@"
                SELECT 
                    c.CustomerID, c.IdentityNumber, c.FirstName, c.LastName, c.Phone, c.Email, 
                    GROUP_CONCAT(DISTINCT r.RoomNumber SEPARATOR ', ') as RoomNumbers,
                    GROUP_CONCAT(DISTINCT res.BedNumber SEPARATOR ', ') as BedNumbers,
                    MAX(CASE WHEN res.Status = 'CheckedIn' THEN 'Aktif' ELSE 'Kaydı Var' END) as ResStatus,
                    c.Address, c.CreatedAt 
                FROM CUSTOMERS c
                LEFT JOIN RESERVATIONS res ON c.CustomerID = res.CustomerID AND res.Status = 'CheckedIn'
                LEFT JOIN ROOMS r ON res.RoomID = r.RoomID
                GROUP BY c.CustomerID, c.IdentityNumber, c.FirstName, c.LastName, c.Phone, c.Email, c.Address, c.CreatedAt
                ORDER BY c.CreatedAt DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetCustomerTableData()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT 
                    c.CustomerID AS 'Müşteri ID',
                    c.IdentityNumber AS 'TC/Pasaport',
                    CONCAT(c.FirstName, ' ', c.LastName) AS 'Ad Soyad',
                    IFNULL(rm.RoomNumber, '-') AS 'Oda No',
                    IFNULL(DATE_FORMAT(r.CheckInDate, '%d/%m/%Y'), '-') AS 'Giriş Tarihi',
                    IFNULL(DATE_FORMAT(r.CheckOutDate, '%d/%m/%Y'), '-') AS 'Çıkış Tarihi',
                    IFNULL(DATEDIFF(r.CheckOutDate, r.CheckInDate), 0) AS 'Gün',
                    IFNULL(r.TotalAmount, 0) AS 'Toplam Tutar',
                    IFNULL((SELECT SUM(TotalAmount) FROM PAYMENTS p WHERE p.ReservationID = r.ReservationID), 0) AS 'Ödenen',
                    CASE 
                        WHEN r.Status = 'CheckedIn' THEN 'Aktif Misafir'
                        WHEN r.Status = 'CheckedOut' THEN 'Arşiv (Çıkış Yaptı)'
                        ELSE 'Kayıtlı Misafir'
                    END AS 'Durum'
                FROM CUSTOMERS c
                LEFT JOIN RESERVATIONS r ON c.CustomerID = r.CustomerID AND r.ReservationID = (
                    SELECT MAX(ReservationID) FROM RESERVATIONS WHERE CustomerID = c.CustomerID
                )
                LEFT JOIN ROOMS rm ON r.RoomID = rm.RoomID
                ORDER BY c.CustomerID DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static async Task<DataTable> GetAllCustomersAsync() 
        {
            return await Task.Run(() => GetAllCustomers());
        }

        // ═══════ RESERVATIONS ═══════
        public static bool IsBedOccupied(string roomNumber, int bedNumber, DateTime checkIn, DateTime checkOut)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM RESERVATIONS r
                JOIN ROOMS rm ON r.RoomID = rm.RoomID
                WHERE rm.RoomNumber = @rn 
                AND r.BedNumber = @bn 
                AND r.Status IN ('CheckedIn', 'Reserved')
                AND (r.CheckInDate < @co AND r.CheckOutDate > @ci)", conn);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            cmd.Parameters.AddWithValue("@bn", bedNumber);
            cmd.Parameters.AddWithValue("@ci", checkIn.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@co", checkOut.ToString("yyyy-MM-dd"));
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public static string GetBedOccupantName(string roomNumber, int bedNumber)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT CONCAT(c.FirstName, ' ', c.LastName) AS GuestName
                FROM RESERVATIONS r
                JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                JOIN ROOMS rm ON r.RoomID = rm.RoomID
                WHERE rm.RoomNumber = @rn AND r.BedNumber = @bn AND r.Status = 'CheckedIn'
                LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            cmd.Parameters.AddWithValue("@bn", bedNumber);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "";
        }

        public static int AddReservation(int customerId, string roomNumber, int bedNumber, DateTime checkIn, DateTime checkOut, string channel = "Direkt", decimal commission = 0, int? companyId = null, decimal? totalAmount = null, string notes = "", decimal extraAmount = 0, string status = null)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            int newId = 0;
            using var tr = conn.BeginTransaction();
            try {
                // Get RoomID and Price (Respecting Current Price)
                int roomId = 0; decimal price = 0;
                using (var cmdRoom = new MySqlCommand(@"
                    SELECT r.RoomID, 
                           IFNULL(
                               (SELECT Price FROM ROOM_PRICES rp WHERE rp.RoomID = r.RoomID ORDER BY rp.StartDate DESC LIMIT 1),
                               IFNULL(rt.BasePrice, 0)
                           ) AS Price
                    FROM ROOMS r 
                    LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID=rt.RoomTypeID 
                    WHERE r.RoomNumber=@r", conn, tr)) {
                    cmdRoom.Parameters.AddWithValue("@r", roomNumber);
                    using var reader = cmdRoom.ExecuteReader();
                    if (reader.Read()) { roomId = reader.GetInt32(0); price = reader.GetDecimal(1); }
                }

                // Calculate stay duration (nights)
                int days = (checkOut.Date - checkIn.Date).Days;
                if (days <= 0) days = 1; 

                // Use provided totalAmount if available (for multi-guest split), otherwise calculate
                decimal finalTotal = totalAmount ?? (price * days);

                // Determine Status based on explicit parameter or fallback to date
                string resStatus = status ?? (checkIn.Date <= DateTime.Today ? "CheckedIn" : "Reserved");

                // Create Reservation
                using (var cmd = new MySqlCommand(@"INSERT INTO RESERVATIONS (CustomerID, RoomID, BedNumber, CheckInDate, CheckOutDate, Status, TotalAmount, ChannelName, CommissionAmount, CompanyID, Notes, ExtraAmount) 
                    VALUES (@cid, @rid, @bn, @ci, @co, @stat, @amt, @ch, @comm, @cmp, @notes, @extra); SELECT LAST_INSERT_ID();", conn, tr)) {
                    cmd.Parameters.AddWithValue("@cid", customerId);
                    cmd.Parameters.AddWithValue("@rid", roomId);
                    cmd.Parameters.AddWithValue("@bn", bedNumber);
                    cmd.Parameters.AddWithValue("@ci", checkIn.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@co", checkOut.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@stat", resStatus);
                    cmd.Parameters.AddWithValue("@amt", finalTotal);
                    cmd.Parameters.AddWithValue("@ch", channel);
                    cmd.Parameters.AddWithValue("@comm", commission);
                    cmd.Parameters.AddWithValue("@cmp", (object?)companyId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@notes", notes);
                    cmd.Parameters.AddWithValue("@extra", extraAmount);
                    newId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Log Activity
                string logType = resStatus == "CheckedIn" ? "Giriş" : "Rezervasyon";
                LogActivity(logType, $"{roomNumber} nolu odaya müşteri {logType.ToLower()} işlemi yapıldı ({bedNumber}. yatak). Tarih: {checkIn:dd.MM} - {checkOut:dd.MM}");

                // Update room occupancy (Recalculate instead of simple increment)
                UpdateRoomOccupancyInternal(conn, tr, roomNumber);
                
                tr.Commit();
                return newId;
            } catch { tr.Rollback(); throw; }
        }

        private static void UpdateRoomOccupancyInternal(MySqlConnection conn, MySqlTransaction tr, string roomNumber)
        {
            // 1) Count current active reservations for this room (Today)
            int activeCount = 0;
            using (var cmdCount = new MySqlCommand(@"SELECT COUNT(*) FROM RESERVATIONS r 
                JOIN ROOMS rm ON r.RoomID = rm.RoomID 
                WHERE rm.RoomNumber=@r 
                AND r.Status IN ('CheckedIn', 'Reserved')
                AND (r.CheckInDate <= CURRENT_DATE AND r.CheckOutDate > CURRENT_DATE)", conn, tr)) {
                cmdCount.Parameters.AddWithValue("@r", roomNumber);
                activeCount = Convert.ToInt32(cmdCount.ExecuteScalar());
            }

            // 2) Get Capacity and current status
            int capacity = 0; string currentStatus = "";
            using (var cmdInfo = new MySqlCommand("SELECT Capacity, Status FROM ROOMS WHERE RoomNumber=@r", conn, tr)) {
                cmdInfo.Parameters.AddWithValue("@r", roomNumber);
                using var reader = cmdInfo.ExecuteReader();
                if (reader.Read()) {
                    capacity = reader.GetInt32(0);
                    currentStatus = reader.GetString(1);
                }
            }

            // 3) Determine Status
            // If room is Maintenance or Dirty, it stays that way unless we are adding a reservation (activeCount > 0)
            string status = currentStatus;
            if (activeCount > 0) {
                status = (activeCount >= capacity) ? "Occupied" : "Partial";
            } else {
                // If it was occupied and now empty, it should be Dirty.
                // If it was already Maintenance, keep it.
                if (currentStatus == "Occupied" || currentStatus == "Partial") {
                    status = "Dirty";
                } else if (currentStatus == "Available") {
                    // stays available
                }
            }

            // 4) Update Room
            using (var cmdFinal = new MySqlCommand("UPDATE ROOMS SET OccupiedBeds=@o, Status=@s WHERE RoomNumber=@r", conn, tr)) {
                cmdFinal.Parameters.AddWithValue("@o", activeCount);
                cmdFinal.Parameters.AddWithValue("@s", status);
                cmdFinal.Parameters.AddWithValue("@r", roomNumber);
                cmdFinal.ExecuteNonQuery();
            }
        }


        public static void CompleteReservation(int reservationId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // 1) Get RoomNumber
                string roomNumber = "";
                using (var cmdInfo = new MySqlCommand(@"SELECT rm.RoomNumber FROM RESERVATIONS r 
                    JOIN ROOMS rm ON r.RoomID=rm.RoomID WHERE r.ReservationID=@id", conn, tr)) {
                    cmdInfo.Parameters.AddWithValue("@id", reservationId);
                    roomNumber = cmdInfo.ExecuteScalar()?.ToString() ?? "";
                }

                // 2) Update Reservation Status
                using (var cmdUpd = new MySqlCommand("UPDATE RESERVATIONS SET Status='CheckedOut' WHERE ReservationID=@id", conn, tr)) {
                    cmdUpd.Parameters.AddWithValue("@id", reservationId);
                    cmdUpd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(roomNumber)) {
                    // Force set room to Dirty if it is now empty
                    UpdateRoomOccupancyInternal(conn, tr, roomNumber);
                    
                    // Re-check: if occupied beds is 0, mark as Dirty
                    using var cmdCheck = new MySqlCommand("SELECT OccupiedBeds FROM ROOMS WHERE RoomNumber=@r", conn, tr);
                    cmdCheck.Parameters.AddWithValue("@r", roomNumber);
                    if (Convert.ToInt32(cmdCheck.ExecuteScalar()) == 0) {
                        using var cmdDirty = new MySqlCommand("UPDATE ROOMS SET Status='Dirty' WHERE RoomNumber=@r", conn, tr);
                        cmdDirty.Parameters.AddWithValue("@r", roomNumber);
                        cmdDirty.ExecuteNonQuery();
                    }
                }
                tr.Commit();
                LogActivity("Çıkış", $"{roomNumber} nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: {reservationId}).");
            } catch { tr.Rollback(); throw; }
        }

        public static void ConfirmCheckInToday(int reservationId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // 1) Update Reservation Status
                using (var cmdUpd = new MySqlCommand("UPDATE RESERVATIONS SET Status='CheckedIn' WHERE ReservationID=@id", conn, tr)) {
                    cmdUpd.Parameters.AddWithValue("@id", reservationId);
                    cmdUpd.ExecuteNonQuery();
                }

                // 2) Get RoomNumber to update occupancy
                string roomNumber = "";
                using (var cmdInfo = new MySqlCommand(@"SELECT rm.RoomNumber FROM RESERVATIONS r 
                    JOIN ROOMS rm ON r.RoomID=rm.RoomID WHERE r.ReservationID=@id", conn, tr)) {
                    cmdInfo.Parameters.AddWithValue("@id", reservationId);
                    roomNumber = cmdInfo.ExecuteScalar()?.ToString() ?? "";
                }

                if (!string.IsNullOrEmpty(roomNumber)) {
                    UpdateRoomOccupancyInternal(conn, tr, roomNumber);
                }

                tr.Commit();
                LogActivity("Giriş", $"{roomNumber} nolu oda için bekleyen rezervasyon onaylandı.");
            } catch { tr.Rollback(); throw; }
        }

        public static void SetRoomStatus(string roomNumber, string status)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("UPDATE ROOMS SET Status=@s WHERE RoomNumber=@r", conn);
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@r", roomNumber);
            cmd.ExecuteNonQuery();
            LogActivity("Oda Durumu", $"{roomNumber} nolu oda durumu {status} olarak güncellendi.");
        }

        public static void MarkAsNoShow(int reservationId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // 1) Get RoomNumber
                string roomNumber = "";
                using (var cmdInfo = new MySqlCommand(@"SELECT rm.RoomNumber FROM RESERVATIONS r 
                    JOIN ROOMS rm ON r.RoomID=rm.RoomID WHERE r.ReservationID=@id", conn, tr)) {
                    cmdInfo.Parameters.AddWithValue("@id", reservationId);
                    roomNumber = cmdInfo.ExecuteScalar()?.ToString() ?? "";
                }

                // 2) Update Reservation Status
                using (var cmdUpd = new MySqlCommand("UPDATE RESERVATIONS SET Status='NoShow' WHERE ReservationID=@id", conn, tr)) {
                    cmdUpd.Parameters.AddWithValue("@id", reservationId);
                    cmdUpd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(roomNumber)) {
                    // Recalculate room occupancy
                    UpdateRoomOccupancyInternal(conn, tr, roomNumber);
                }
                tr.Commit();
                LogActivity("No-Show", $"{roomNumber} nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.");
            } catch { tr.Rollback(); throw; }
        }

        public static void ProcessNoShowReservations()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // Find reservations that are 'Reserved' or 'Pending' and CheckInDate < Today
            using var cmd = new MySqlCommand(@"
                SELECT ReservationID FROM RESERVATIONS 
                WHERE (Status='Reserved' OR Status='Pending') 
                AND CheckInDate < CURRENT_DATE", conn);
            
            var ids = new List<int>();
            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) ids.Add(reader.GetInt32(0));
            }

            foreach (var id in ids) {
                MarkAsNoShow(id);
            }
        }


        public static DataTable GetPendingReservations()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT r.ReservationID, 
                    CONCAT(c.FirstName, ' ', c.LastName) AS Musteri,
                    rm.RoomNumber AS Oda,
                    r.BedNumber AS Yatak,
                    r.CheckInDate AS Giris,
                    r.CheckOutDate AS Cikis,
                    r.TotalAmount AS Tutar,
                    r.Notes AS Notlar,
                    r.IsOnline
                FROM RESERVATIONS r
                JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                JOIN ROOMS rm ON r.RoomID = rm.RoomID
                WHERE r.Status = 'Pending'
                ORDER BY r.CreatedAt DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void ConfirmReservation(int reservationId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // 1) Get reservation details for email
                string roomNumber = "", customerEmail = "", customerName = "";
                int bedNumber = 0, floorNumber = 0;
                DateTime checkIn = DateTime.MinValue, checkOut = DateTime.MinValue;
                decimal pricePerNight = 0;

                using (var cmd = new MySqlCommand(@"
                    SELECT rm.RoomNumber, r.BedNumber, IFNULL(f.FloorNumber, 0), r.CheckInDate, r.CheckOutDate, 
                           IFNULL(rt.BasePrice, 0), c.Email, CONCAT(c.FirstName, ' ', c.LastName)
                    FROM RESERVATIONS r 
                    JOIN ROOMS rm ON r.RoomID = rm.RoomID 
                    JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                    LEFT JOIN FLOORS f ON rm.FloorID = f.FloorID
                    LEFT JOIN ROOM_TYPES rt ON rm.RoomTypeID = rt.RoomTypeID
                    WHERE r.ReservationID = @id", conn, tr))
                {
                    cmd.Parameters.AddWithValue("@id", reservationId);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        roomNumber = reader.GetString(0);
                        bedNumber = reader.GetInt32(1);
                        floorNumber = reader.GetInt32(2);
                        checkIn = reader.GetDateTime(3);
                        checkOut = reader.GetDateTime(4);
                        pricePerNight = reader.GetDecimal(5);
                        customerEmail = reader.IsDBNull(6) ? "" : reader.GetString(6);
                        customerName = reader.GetString(7);
                    }
                }

                // 2) Update status to Reserved (Not CheckedIn yet)
                using (var cmd = new MySqlCommand("UPDATE RESERVATIONS SET Status = 'Reserved' WHERE ReservationID = @id", conn, tr))
                {
                    cmd.Parameters.AddWithValue("@id", reservationId);
                    cmd.ExecuteNonQuery();
                }

                // 3) Update room occupancy because it's now Reserved
                if (!string.IsNullOrEmpty(roomNumber)) {
                    UpdateRoomOccupancyInternal(conn, tr, roomNumber);
                }

                tr.Commit();
                LogActivity("Onay", $"{roomNumber} nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).");

                // 4) Send Email
                if (!string.IsNullOrEmpty(customerEmail)) {
                    string subject = "Rezervasyonunuz Onaylandı - SOM-PMS";
                    string body = MailHelper.GetConfirmationTemplate(customerName, roomNumber, bedNumber, floorNumber, checkIn.ToString("dd.MM.yyyy"), checkOut.ToString("dd.MM.yyyy"), pricePerNight);
                    _ = Task.Run(() => MailHelper.SendEmailAsync(customerEmail, subject, body));
                }
            } catch { tr.Rollback(); throw; }
        }

        public static void PerformCheckIn(int reservationId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // 1) Get details for welcome email
                string roomNumber = "", customerEmail = "", customerName = "";
                int bedNumber = 0, floorNumber = 0;
                DateTime checkIn = DateTime.MinValue, checkOut = DateTime.MinValue;

                using (var cmd = new MySqlCommand(@"
                    SELECT rm.RoomNumber, r.BedNumber, IFNULL(f.FloorNumber, 0), r.CheckInDate, r.CheckOutDate, 
                           c.Email, CONCAT(c.FirstName, ' ', c.LastName)
                    FROM RESERVATIONS r 
                    JOIN ROOMS rm ON r.RoomID = rm.RoomID 
                    JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                    LEFT JOIN FLOORS f ON rm.FloorID = f.FloorID
                    WHERE r.ReservationID = @id", conn, tr))
                {
                    cmd.Parameters.AddWithValue("@id", reservationId);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        roomNumber = reader.GetString(0);
                        bedNumber = reader.GetInt32(1);
                        floorNumber = reader.GetInt32(2);
                        checkIn = reader.GetDateTime(3);
                        checkOut = reader.GetDateTime(4);
                        customerEmail = reader.IsDBNull(5) ? "" : reader.GetString(5);
                        customerName = reader.GetString(6);
                    }
                }

                // 2) Update status to CheckedIn
                using (var cmd = new MySqlCommand("UPDATE RESERVATIONS SET Status = 'CheckedIn' WHERE ReservationID = @id", conn, tr))
                {
                    cmd.Parameters.AddWithValue("@id", reservationId);
                    cmd.ExecuteNonQuery();
                }

                // 3) NOW update room occupancy
                if (!string.IsNullOrEmpty(roomNumber)) {
                    UpdateRoomOccupancyInternal(conn, tr, roomNumber);
                }

                tr.Commit();
                LogActivity("Check-In", $"{roomNumber} nolu odaya müşteri girişi yapıldı.");

                // 4) Send Welcome Email
                if (!string.IsNullOrEmpty(customerEmail)) {
                    string subject = "Hoş Geldiniz! - SOM-PMS Pansiyon";
                    string body = MailHelper.GetWelcomeTemplate(customerName, roomNumber, bedNumber, floorNumber, checkIn.ToString("dd.MM.yyyy"), checkOut.ToString("dd.MM.yyyy"));
                    _ = Task.Run(() => MailHelper.SendEmailAsync(customerEmail, subject, body));
                }
            } catch { tr.Rollback(); throw; }
        }

        public static DataTable GetTodaysArrivals()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT r.ReservationID, CONCAT(c.FirstName, ' ', c.LastName) AS Musteri, rm.RoomNumber 
                FROM RESERVATIONS r
                JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                JOIN ROOMS rm ON r.RoomID = rm.RoomID
                WHERE r.Status = 'Reserved' AND DATE(r.CheckInDate) = CURRENT_DATE", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static List<int> GetOccupiedBedNumbers(string roomNumber, DateTime checkIn, DateTime checkOut)
        {
            var occupiedBeds = new List<int>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT r.BedNumber FROM RESERVATIONS r
                JOIN ROOMS rm ON r.RoomID = rm.RoomID
                WHERE rm.RoomNumber = @rn 
                AND r.Status IN ('CheckedIn', 'Reserved')
                AND (r.CheckInDate < @co AND r.CheckOutDate > @ci)", conn);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            cmd.Parameters.AddWithValue("@ci", checkIn.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@co", checkOut.ToString("yyyy-MM-dd"));
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                occupiedBeds.Add(reader.GetInt32(0));
            }
            return occupiedBeds;
        }

        public static DataTable GetReservations(string statusFilter = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            
            string whereClause = "1=1";
            if (!string.IsNullOrEmpty(statusFilter)) {
                if (statusFilter == "Active") {
                    whereClause += " AND r.Status IN ('CheckedIn', 'CheckedOut')";
                } else if (statusFilter == "Future") {
                    whereClause += " AND r.Status = 'Reserved'";
                } else {
                    whereClause += " AND r.Status = @st";
                }
            }
            if (startDate.HasValue) {
                whereClause += " AND DATE(r.CheckInDate) >= @sd";
            }
            if (endDate.HasValue) {
                whereClause += " AND DATE(r.CheckInDate) <= @ed";
            }

            using var cmd = new MySqlCommand($@"SELECT r.ReservationID, 
                c.IdentityNumber AS IdentityNumber,
                c.Phone AS Phone,
                CONCAT(c.FirstName,' ',c.LastName) AS Musteri, 
                rm.RoomNumber AS Oda, 
                f.FloorNumber,
                r.BedNumber AS Yatak,
                IFNULL(rt.TypeName,'Standart') AS OdaTipi,
                IFNULL(rt.BasePrice,0) AS Fiyat,
                r.CheckInDate AS Giris, 
                r.CheckOutDate AS Cikis,
                r.Status,
                r.TotalAmount AS ToplamTutar,
                IFNULL(r.PaidAmount, 0) AS OdenenMiktar,
                r.Notes AS Notlar
                FROM RESERVATIONS r 
                JOIN CUSTOMERS c ON r.CustomerID=c.CustomerID 
                JOIN ROOMS rm ON r.RoomID=rm.RoomID 
                LEFT JOIN FLOORS f ON rm.FloorID = f.FloorID
                LEFT JOIN ROOM_TYPES rt ON rm.RoomTypeID=rt.RoomTypeID
                WHERE {whereClause}
                ORDER BY r.CheckInDate DESC", conn);
            
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Active" && statusFilter != "Future")
                cmd.Parameters.AddWithValue("@st", statusFilter);
            if (startDate.HasValue)
                cmd.Parameters.AddWithValue("@sd", startDate.Value.ToString("yyyy-MM-dd"));
            if (endDate.HasValue)
                cmd.Parameters.AddWithValue("@ed", endDate.Value.ToString("yyyy-MM-dd"));

            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetActiveReservations()
        {
            return GetReservations("Active");
        }

        public static DataTable GetFutureReservations(DateTime startDate, DateTime endDate)
        {
            return GetReservations("Future", startDate, endDate);
        }

        public static string GetRoomConflictDetails(string roomNumber, DateTime ci, DateTime co)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT CONCAT(c.FirstName, ' ', c.LastName) as Musteri, r.CheckInDate, r.CheckOutDate
                FROM RESERVATIONS r
                JOIN ROOMS rm ON r.RoomID = rm.RoomID
                JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                WHERE rm.RoomNumber = @rn 
                AND r.Status IN ('CheckedIn', 'Reserved')
                AND (r.CheckInDate < @co AND r.CheckOutDate > @ci)
                LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            cmd.Parameters.AddWithValue("@ci", ci.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@co", co.ToString("yyyy-MM-dd"));
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) {
                string name = reader.GetString(0);
                DateTime exCi = reader.GetDateTime(1);
                DateTime exCo = reader.GetDateTime(2);
                return $"Bu oda ({roomNumber}) seçilen tarihlerde dolu!\n\nMevcut Kayıt: {name}\nTarih Aralığı: {exCi:dd.MM.yyyy} - {exCo:dd.MM.yyyy}\n\nLütfen mevcut misafirin çıkış tarihinden sonrasını seçiniz.";
            }
            return null;
        }

        public static async Task<DataTable> GetReservationsAsync() 
        {
            return await Task.Run(() => GetReservations());
        }

        public static DataRow? GetReservationDetailsForCheckout(int resId)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT 
                    r.ReservationID,
                    CONCAT(c.FirstName, ' ', c.LastName) as Musteri,
                    rm.RoomNumber as Oda,
                    r.BedNumber as Yatak,
                    r.TotalAmount as ToplamTutar,
                    IFNULL(r.PaidAmount, 0) as OdenenMiktar,
                    r.CheckInDate as Giris,
                    r.CheckOutDate as Cikis
                FROM RESERVATIONS r
                JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                JOIN ROOMS rm ON r.RoomID = rm.RoomID
                WHERE r.ReservationID = @id", conn);
            cmd.Parameters.AddWithValue("@id", resId);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static DataTable GetAllRoomsDetailed()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT r.RoomID, r.RoomNumber, f.FloorNumber AS Kat,
                    IFNULL(rt.TypeName,'Standart') AS OdaTipi,
                    r.Capacity AS Kapasite, r.OccupiedBeds AS DoluYatak, r.Status AS Durum,
                    IFNULL(
                        (SELECT Price FROM ROOM_PRICES rp WHERE rp.RoomID = r.RoomID ORDER BY rp.StartDate DESC LIMIT 1),
                        IFNULL(rt.BasePrice, 0)
                    ) AS GuncelFiyat
                FROM ROOMS r
                JOIN FLOORS f ON r.FloorID = f.FloorID
                LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID = rt.RoomTypeID
                ORDER BY r.RoomNumber", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetAvailableRoomsWithPrice()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT r.RoomNumber, f.FloorNumber,
                    IFNULL(rt.TypeName,'Standart') AS TypeName,
                    IFNULL(
                        (SELECT Price FROM ROOM_PRICES rp WHERE rp.RoomID = r.RoomID ORDER BY rp.StartDate DESC LIMIT 1),
                        IFNULL(rt.BasePrice, 0)
                    ) AS CurrentPrice
                FROM ROOMS r
                JOIN FLOORS f ON r.FloorID = f.FloorID
                LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE r.Status != 'Occupied'
                ORDER BY r.RoomNumber", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataRow? GetRoomInfo(string roomNumber)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT r.RoomID, r.RoomNumber, f.FloorNumber,
                    IFNULL(rt.TypeName,'Standart') AS TypeName,
                    IFNULL(
                        (SELECT Price FROM ROOM_PRICES rp WHERE rp.RoomID = r.RoomID ORDER BY rp.StartDate DESC LIMIT 1),
                        IFNULL(rt.BasePrice, 0)
                    ) AS CurrentPrice,
                    r.Capacity
                FROM ROOMS r
                JOIN FLOORS f ON r.FloorID = f.FloorID
                LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE r.RoomNumber = @rn", conn);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static void SetRoomPrice(string roomNumber, decimal price)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // Get RoomID directly
            using var cmdId = new MySqlCommand("SELECT RoomID FROM ROOMS WHERE RoomNumber=@r", conn);
            cmdId.Parameters.AddWithValue("@r", roomNumber);
            var roomIdObj = cmdId.ExecuteScalar();
            if (roomIdObj == null || roomIdObj == DBNull.Value) return;
            int roomId = Convert.ToInt32(roomIdObj);

            // Insert new price entry linked to this specific room (old prices kept for history)
            using var cmd = new MySqlCommand(@"INSERT INTO ROOM_PRICES (RoomTypeID, RoomID, RoomNumber, StartDate, Price) 
                VALUES (NULL, @rid, @rn, @dt, @p)", conn);
            cmd.Parameters.AddWithValue("@rid", roomId);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            cmd.Parameters.AddWithValue("@dt", DateTime.Today.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@p", price);
            cmd.ExecuteNonQuery();
        }

        public static DataTable GetRoomPriceHistory(string roomNumber)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT rp.StartDate AS Tarih, rp.Price AS Fiyat
                FROM ROOM_PRICES rp
                JOIN ROOMS r ON rp.RoomID = r.RoomID
                WHERE r.RoomNumber = @rn
                ORDER BY rp.StartDate ASC", conn);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void AddRoom(string roomNumber, int floorNumber, string typeName, int capacity)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmdFloor = new MySqlCommand("SELECT FloorID FROM FLOORS WHERE FloorNumber=@f", conn);
            cmdFloor.Parameters.AddWithValue("@f", floorNumber);
            var floorId = cmdFloor.ExecuteScalar();
            if (floorId == null) {
                using var insFloor = new MySqlCommand("INSERT INTO FLOORS (FloorNumber, Description) VALUES (@f, @d); SELECT LAST_INSERT_ID();", conn);
                insFloor.Parameters.AddWithValue("@f", floorNumber);
                insFloor.Parameters.AddWithValue("@d", $"Kat {floorNumber}");
                floorId = insFloor.ExecuteScalar();
            }
            using var cmdType = new MySqlCommand("SELECT RoomTypeID FROM ROOM_TYPES WHERE TypeName=@t", conn);
            cmdType.Parameters.AddWithValue("@t", typeName);
            var typeId = cmdType.ExecuteScalar();
            if (typeId == null) {
                using var insType = new MySqlCommand("INSERT INTO ROOM_TYPES (TypeName, BasePrice) VALUES (@t, 0); SELECT LAST_INSERT_ID();", conn);
                insType.Parameters.AddWithValue("@t", typeName);
                typeId = insType.ExecuteScalar();
            }
            using var cmd = new MySqlCommand(@"INSERT INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES (@rn, @fid, @tid, @cap, 0, 'Available')", conn);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            cmd.Parameters.AddWithValue("@fid", floorId);
            cmd.Parameters.AddWithValue("@tid", typeId);
            cmd.Parameters.AddWithValue("@cap", capacity);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteRoom(string roomNumber)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM ROOMS WHERE RoomNumber=@rn", conn);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            cmd.ExecuteNonQuery();
        }

        // ═══════ YENİ STOK MİMARİSİ (PRODUCTS, DEPOT, MARKET) ═══════

        // DEPO STOK GİRİŞ LOGU — Her giriş ayrı satır olarak gösterilir (kronolojik sıra)
        public static DataTable GetStorageEntryLog()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                @"SELECT 
                    p.ProductID,
                    p.Barcode,
                    p.ItemName,
                    p.ManufacturerName,
                    t.ToLocation,
                    t.Quantity,
                    t.PurchasePrice,
                    t.EmployeeName,
                    t.SupplierName,
                    t.TransferDate
                  FROM STOCK_TRANSFERS t
                  JOIN PRODUCTS p ON t.ProductID = p.ProductID
                  WHERE t.ToLocation IN ('DEPO', 'LOKANTA')
                  ORDER BY t.TransferDate DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        // TÜM MEVCUT STOKLAR (Özet) - Depo ve Lokanta bir arada
        public static DataTable GetCombinedStockStatus()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                @"SELECT 
                    p.ProductID, 
                    p.Barcode AS Barkod, 
                    p.ItemName AS 'Ürün Adı', 
                    p.ManufacturerName AS 'Üretici',
                    p.Category AS 'Kategori',
                    p.Unit AS 'Birim',
                    IFNULL(s.Quantity, 0) AS 'Depo Stok',
                    IFNULL(m.Quantity, 0) AS 'Lokanta Stok',
                    IFNULL(p.SuggestedSalePrice, 0) AS 'Satış Fiyatı'
                  FROM PRODUCTS p
                  LEFT JOIN STORAGE_STOCKS s ON p.ProductID = s.ProductID AND s.Location = 'DEPO'
                  LEFT JOIN MARKET_STOCKS m ON p.ProductID = m.ProductID AND m.StoreID = 'LOKANTA'
                  ORDER BY p.ItemName ASC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetTodayStorageLog()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                @"SELECT 
                    p.ItemName AS 'Ürün Adı',
                    t.Quantity AS 'Adet',
                    t.PurchasePrice AS 'Alış Fiyatı',
                    IFNULL(p.SuggestedSalePrice, 0) AS 'Satış Fiyatı',
                    IFNULL(t.SupplierName, '-') AS 'Tedarikçi',
                    IFNULL(t.EmployeeName, '-') AS 'Teslim Alan',
                    t.ToLocation AS 'Hedef',
                    IFNULL(t.InvoiceNumber, '') AS 'Fatura No',
                    IFNULL(t.PaymentMethod, '') AS 'Ödeme',
                    TIME(t.TransferDate) AS 'Saat'
                  FROM STOCK_TRANSFERS t
                  JOIN PRODUCTS p ON t.ProductID = p.ProductID
                  WHERE DATE(t.TransferDate) = CURDATE()
                    AND t.ToLocation IN ('DEPO', 'LOKANTA')
                  ORDER BY t.TransferDate DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetStorageEntryLogFiltered(DateTime? startDate = null, DateTime? endDate = null, string productFilter = "", string supplierFilter = "", string employeeFilter = "")
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            var sql = new System.Text.StringBuilder();
            sql.Append(@"SELECT 
                    t.TransferID AS ID,
                    p.Barcode AS 'Barkod',
                    p.ItemName AS 'Ürün Adı',
                    p.ManufacturerName AS 'Üretici',
                    t.ToLocation AS 'Hedef',
                    t.Quantity AS 'Adet',
                    t.PurchasePrice AS 'Alış Fiyatı',
                    IFNULL(p.SuggestedSalePrice, 0) AS 'Satış Fiyatı',
                    IFNULL(t.EmployeeName, '-') AS 'Teslim Alan',
                    IFNULL(t.SupplierName, '-') AS 'Tedarikçi',
                    IFNULL(t.InvoiceNumber, '') AS 'Fatura No',
                    IFNULL(t.PaymentMethod, '') AS 'Ödeme',
                    t.TransferDate AS 'Geliş Tarihi'
                  FROM STOCK_TRANSFERS t
                  JOIN PRODUCTS p ON t.ProductID = p.ProductID
                  WHERE t.ToLocation IN ('DEPO', 'LOKANTA')");
            if (startDate.HasValue) sql.Append(" AND DATE(t.TransferDate) >= @sd");
            if (endDate.HasValue)   sql.Append(" AND DATE(t.TransferDate) <= @ed");
            if (!string.IsNullOrEmpty(productFilter))  sql.Append(" AND p.ItemName LIKE @pf");
            if (!string.IsNullOrEmpty(supplierFilter)) sql.Append(" AND t.SupplierName LIKE @sf");
            if (!string.IsNullOrEmpty(employeeFilter)) sql.Append(" AND t.EmployeeName LIKE @ef");
            sql.Append(" ORDER BY t.TransferDate DESC");
            using var cmd = new MySqlCommand(sql.ToString(), conn);
            if (startDate.HasValue) cmd.Parameters.AddWithValue("@sd", startDate.Value.ToString("yyyy-MM-dd"));
            if (endDate.HasValue)   cmd.Parameters.AddWithValue("@ed", endDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(productFilter))  cmd.Parameters.AddWithValue("@pf", "%" + productFilter + "%");
            if (!string.IsNullOrEmpty(supplierFilter)) cmd.Parameters.AddWithValue("@sf", "%" + supplierFilter + "%");
            if (!string.IsNullOrEmpty(employeeFilter)) cmd.Parameters.AddWithValue("@ef", "%" + employeeFilter + "%");
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void AddOrUpdateStorageItemByID(int productId, string barcode, string itemName, string manufacturer, string category, string unit, decimal price, decimal suggestedSalePrice, string location, int qtyToAdd, DateTime arrivalDate, decimal purchasePrice = 0, string employeeName = "", string supplierName = "", string invoiceNumber = "", string paymentMethod = "")
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // 1) Ürün var mı kontrol et
                bool productExists = false;
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM PRODUCTS WHERE ProductID = @id", conn, tr)) {
                    cmd.Parameters.AddWithValue("@id", productId);
                    productExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }

                if (!productExists) {
                    using var cmdIns = new MySqlCommand("INSERT INTO PRODUCTS (ProductID, Barcode, ItemName, ManufacturerName, Category, Unit, Price, SuggestedSalePrice) VALUES (@id, @b, @n, @m, @c, @u, @p, @sp)", conn, tr);
                    cmdIns.Parameters.AddWithValue("@id", productId);
                    cmdIns.Parameters.AddWithValue("@b", string.IsNullOrEmpty(barcode) ? "PRD-" + productId : barcode);
                    cmdIns.Parameters.AddWithValue("@n", itemName);
                    cmdIns.Parameters.AddWithValue("@m", manufacturer);
                    cmdIns.Parameters.AddWithValue("@c", category);
                    cmdIns.Parameters.AddWithValue("@u", unit);
                    cmdIns.Parameters.AddWithValue("@p", price);
                    cmdIns.Parameters.AddWithValue("@sp", suggestedSalePrice);
                    cmdIns.ExecuteNonQuery();
                } else {
                    using var cmdUpd = new MySqlCommand("UPDATE PRODUCTS SET Barcode=@b, ItemName=@n, ManufacturerName=@m, Category=@c, Unit=@u, Price=@p, SuggestedSalePrice=@sp WHERE ProductID=@id", conn, tr);
                    cmdUpd.Parameters.AddWithValue("@b", string.IsNullOrEmpty(barcode) ? "PRD-" + productId : barcode);
                    cmdUpd.Parameters.AddWithValue("@n", itemName);
                    cmdUpd.Parameters.AddWithValue("@m", manufacturer);
                    cmdUpd.Parameters.AddWithValue("@c", category);
                    cmdUpd.Parameters.AddWithValue("@u", unit);
                    cmdUpd.Parameters.AddWithValue("@p", price);
                    cmdUpd.Parameters.AddWithValue("@sp", suggestedSalePrice);
                    cmdUpd.Parameters.AddWithValue("@id", productId);
                    cmdUpd.ExecuteNonQuery();
                }

                if (location == "DEPO") {
                    // 2) Depo Stoguna (STORAGE_STOCKS) Ekle / Guncelle
                    using (var chk = new MySqlCommand("SELECT COUNT(*) FROM STORAGE_STOCKS WHERE ProductID=@id", conn, tr)) {
                        chk.Parameters.AddWithValue("@id", productId);
                        if (Convert.ToInt32(chk.ExecuteScalar()) > 0) {
                            using var upd = new MySqlCommand("UPDATE STORAGE_STOCKS SET Quantity = Quantity + @q, Location = @loc, ArrivalDate = IF(@q > 0, @dt, ArrivalDate) WHERE ProductID=@id", conn, tr);
                            upd.Parameters.AddWithValue("@q", qtyToAdd);
                            upd.Parameters.AddWithValue("@loc", location);
                            upd.Parameters.AddWithValue("@id", productId);
                            upd.Parameters.AddWithValue("@dt", arrivalDate);
                            upd.ExecuteNonQuery();
                        } else {
                            using var ins = new MySqlCommand("INSERT INTO STORAGE_STOCKS (ProductID, Quantity, Location, ArrivalDate) VALUES (@id, @q, @loc, @dt)", conn, tr);
                            ins.Parameters.AddWithValue("@id", productId);
                            ins.Parameters.AddWithValue("@q", qtyToAdd);
                            ins.Parameters.AddWithValue("@loc", location);
                            ins.Parameters.AddWithValue("@dt", arrivalDate);
                            ins.ExecuteNonQuery();
                        }
                    }
                    // KRITIK: Depoya giris olsa bile, eger urun Lokanta'da (MARKET_STOCKS) tanimliysa fiyatini guncelle
                    if (suggestedSalePrice > 0) {
                        using var updMktPrice = new MySqlCommand("UPDATE MARKET_STOCKS SET Price = @sp WHERE ProductID=@id AND StoreID='LOKANTA'", conn, tr);
                        updMktPrice.Parameters.AddWithValue("@sp", suggestedSalePrice);
                        updMktPrice.Parameters.AddWithValue("@id", productId);
                        updMktPrice.ExecuteNonQuery();
                    }
                } else if (location == "LOKANTA") {
                    // 2) Lokanta Stoguna (MARKET_STOCKS) Ekle / Guncelle
                    using (var chk = new MySqlCommand("SELECT COUNT(*) FROM MARKET_STOCKS WHERE ProductID=@id AND StoreID='LOKANTA'", conn, tr)) {
                        chk.Parameters.AddWithValue("@id", productId);
                        if (Convert.ToInt32(chk.ExecuteScalar()) > 0) {
                            // Fiyatı da güncelliyoruz (User talebi: son fiyat otomatik degissin)
                            using var upd = new MySqlCommand("UPDATE MARKET_STOCKS SET Quantity = Quantity + @q, Price = @p WHERE ProductID=@id AND StoreID='LOKANTA'", conn, tr);
                            upd.Parameters.AddWithValue("@q", qtyToAdd);
                            upd.Parameters.AddWithValue("@p", suggestedSalePrice); // suggestedSalePrice'ı kullanıyoruz
                            upd.Parameters.AddWithValue("@id", productId);
                            upd.ExecuteNonQuery();
                        } else {
                            using var ins = new MySqlCommand("INSERT INTO MARKET_STOCKS (ProductID, StoreID, Quantity, Price) VALUES (@id, 'LOKANTA', @q, @p)", conn, tr);
                            ins.Parameters.AddWithValue("@id", productId);
                            ins.Parameters.AddWithValue("@q", qtyToAdd);
                            ins.Parameters.AddWithValue("@p", suggestedSalePrice);
                            ins.ExecuteNonQuery();
                        }
                    }
                }

                // 3) GİRİŞ LOGU (STOCK_TRANSFERS tablosu)
                using (var log = new MySqlCommand("INSERT INTO STOCK_TRANSFERS (ProductID, FromLocation, ToLocation, Quantity, PurchasePrice, EmployeeName, SupplierName, InvoiceNumber, PaymentMethod, Notes, TransferDate) VALUES (@id, 'TEDARIKCI', @loc, @q, @pp, @emp, @sup, @inv, @pay, @n, @dt)", conn, tr)) {
                    log.Parameters.AddWithValue("@id", productId);
                    log.Parameters.AddWithValue("@loc", location ?? "DEPO");
                    log.Parameters.AddWithValue("@q", qtyToAdd);
                    log.Parameters.AddWithValue("@pp", purchasePrice);
                    log.Parameters.AddWithValue("@emp", employeeName);
                    log.Parameters.AddWithValue("@sup", supplierName);
                    log.Parameters.AddWithValue("@inv", invoiceNumber);
                    log.Parameters.AddWithValue("@pay", paymentMethod);
                    log.Parameters.AddWithValue("@n", "Stok Girişi (Mal Kabul)");
                    log.Parameters.AddWithValue("@dt", arrivalDate);
                    log.ExecuteNonQuery();
                }

                tr.Commit();
            }
            catch { tr.Rollback(); throw; }
        }

        public static DataRow? GetProductByID(int productId)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM PRODUCTS WHERE ProductID=@id", conn);
            cmd.Parameters.AddWithValue("@id", productId);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static DataRow? GetProductByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return null;
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM PRODUCTS WHERE Barcode=@b", conn);
            cmd.Parameters.AddWithValue("@b", barcode);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static void RegisterProduct(int productId, string barcode, string itemName, string category, string manufacturer, string unit, decimal price, decimal suggestedSalePrice)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("INSERT INTO PRODUCTS (ProductID, Barcode, ItemName, Category, ManufacturerName, Unit, Price, SuggestedSalePrice) VALUES (@id, @b, @n, @c, @m, @u, @p, @sp) ON DUPLICATE KEY UPDATE Barcode=@b, ItemName=@n, Category=@c, ManufacturerName=@m, Unit=@u, Price=@p, SuggestedSalePrice=@sp", conn);
            cmd.Parameters.AddWithValue("@id", productId);
            cmd.Parameters.AddWithValue("@b", string.IsNullOrEmpty(barcode) ? "PRD-" + productId : barcode);
            cmd.Parameters.AddWithValue("@n", itemName);
            cmd.Parameters.AddWithValue("@c", category);
            cmd.Parameters.AddWithValue("@m", manufacturer);
            cmd.Parameters.AddWithValue("@u", unit);
            cmd.Parameters.AddWithValue("@p", price);
            cmd.Parameters.AddWithValue("@sp", suggestedSalePrice);
            cmd.ExecuteNonQuery();
        }

        public static DataTable GetAllProducts()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT ProductID, Barcode, ItemName, Category, ManufacturerName, Unit, Price, SuggestedSalePrice FROM PRODUCTS ORDER BY ItemName ASC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        // ═══════ MANUFACTURERS ═══════
        public static void AddManufacturer(string name)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("INSERT IGNORE INTO MANUFACTURERS (Name) VALUES (@n)", conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.ExecuteNonQuery();
        }

        public static DataTable GetAllManufacturers()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM MANUFACTURERS ORDER BY Name ASC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void DeleteManufacturer(string name)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM MANUFACTURERS WHERE Name=@n", conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.ExecuteNonQuery();
        }

        // ═══════ SUPPLIERS ═══════
        public static void AddSupplier(string name)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("INSERT IGNORE INTO SUPPLIERS (Name) VALUES (@n)", conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.ExecuteNonQuery();
        }

        public static DataTable GetAllSuppliers()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM SUPPLIERS ORDER BY Name ASC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void DeleteSupplier(string name)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM SUPPLIERS WHERE Name=@n", conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.ExecuteNonQuery();
        }

        // MARKET (MAĞAZA) STOK LİSTESİ
        public static DataTable GetAllMarketStocks(string storeId = "MARKET_1")
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                @"SELECT 
                    p.ProductID, 
                    p.Barcode, 
                    p.ItemName, 
                    p.ManufacturerName,
                    p.Category,
                    COALESCE(NULLIF(m.Price, 0), p.SuggestedSalePrice, p.Price, 0) AS Price, 
                    m.StoreID, 
                    IFNULL(m.Quantity, 0) AS MarketQuantity
                  FROM MARKET_STOCKS m
                  INNER JOIN PRODUCTS p ON p.ProductID = m.ProductID
                  WHERE m.StoreID = @storeId
                  ORDER BY p.ItemName ASC", conn);
            cmd.Parameters.AddWithValue("@storeId", storeId);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        // TRANFER: DEPODAN MARKETE GÖNDER (Fiyat ile)
        public static void TransferToMarketWithPrice(int productId, string storeId, int qtyToTransfer, decimal salesPrice, string notes = "", DateTime? transferDate = null)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // 1) Depo stogunu kontrol et
                int storageQty = 0;
                using (var cmd = new MySqlCommand("SELECT Quantity FROM STORAGE_STOCKS WHERE ProductID=@id", conn, tr)) {
                    cmd.Parameters.AddWithValue("@id", productId);
                    var res = cmd.ExecuteScalar();
                    if (res != null) storageQty = Convert.ToInt32(res);
                }
                if (storageQty < qtyToTransfer) throw new Exception("Depoda yeterli ürün yok! Mevcut: " + storageQty);

                // 2) Depodan Düş (--)
                using (var updDepo = new MySqlCommand("UPDATE STORAGE_STOCKS SET Quantity = Quantity - @q WHERE ProductID=@id", conn, tr)) {
                    updDepo.Parameters.AddWithValue("@q", qtyToTransfer);
                    updDepo.Parameters.AddWithValue("@id", productId);
                    updDepo.ExecuteNonQuery();
                }

                // 3) Markete Ekle (++)
                using (var chkMarket = new MySqlCommand("SELECT COUNT(*) FROM MARKET_STOCKS WHERE ProductID=@id AND StoreID=@s", conn, tr)) {
                    chkMarket.Parameters.AddWithValue("@id", productId);
                    chkMarket.Parameters.AddWithValue("@s", storeId);
                    
                    // Eğer salesPrice belirtilmemişse (0 ise), ürünün tablodaki varsayılan fiyatını al
                    if (salesPrice <= 0) {
                        using (var cmdPrice = new MySqlCommand("SELECT Price FROM PRODUCTS WHERE ProductID=@id", conn, tr)) {
                            cmdPrice.Parameters.AddWithValue("@id", productId);
                            salesPrice = Convert.ToDecimal(cmdPrice.ExecuteScalar() ?? 0);
                        }
                    }

                    if (Convert.ToInt32(chkMarket.ExecuteScalar()) > 0) {
                        using var updMkt = new MySqlCommand("UPDATE MARKET_STOCKS SET Quantity = Quantity + @q, Price = @p WHERE ProductID=@id AND StoreID=@s", conn, tr);
                        updMkt.Parameters.AddWithValue("@q", qtyToTransfer);
                        updMkt.Parameters.AddWithValue("@p", salesPrice);
                        updMkt.Parameters.AddWithValue("@id", productId);
                        updMkt.Parameters.AddWithValue("@s", storeId);
                        updMkt.ExecuteNonQuery();
                    } else {
                        using var insMkt = new MySqlCommand("INSERT INTO MARKET_STOCKS (ProductID, StoreID, Quantity, Price) VALUES (@id, @s, @q, @p)", conn, tr);
                        insMkt.Parameters.AddWithValue("@id", productId);
                        insMkt.Parameters.AddWithValue("@s", storeId);
                        insMkt.Parameters.AddWithValue("@q", qtyToTransfer);
                        insMkt.Parameters.AddWithValue("@p", salesPrice);
                        insMkt.ExecuteNonQuery();
                    }
                }

                // 4) Transfer Logu (STOCK_TRANSFERS)
                using (var log = new MySqlCommand("INSERT INTO STOCK_TRANSFERS (ProductID, FromLocation, ToLocation, Quantity, Notes, TransferDate) VALUES (@id, 'DEPO', @s, @q, @n, @dt)", conn, tr)) {
                    log.Parameters.AddWithValue("@id", productId);
                    log.Parameters.AddWithValue("@s", storeId);
                    log.Parameters.AddWithValue("@q", qtyToTransfer);
                    log.Parameters.AddWithValue("@n", notes);
                    log.Parameters.AddWithValue("@dt", transferDate ?? DateTime.Now);
                    log.ExecuteNonQuery();
                }
                tr.Commit();
            }
            catch { tr.Rollback(); throw; }
        }

        public static void TransferToMarket(int productId, string storeId, int qtyToTransfer, string notes = "")
        {
            TransferToMarketWithPrice(productId, storeId, qtyToTransfer, 0, notes);
        }

        public static void TransferToMarket(int productId, string storeId, int qtyToTransfer, string notes, DateTime transferDate)
        {
            TransferToMarketWithPrice(productId, storeId, qtyToTransfer, 0, notes, transferDate);
        }

        // SATIŞ YAP (Brakod ile Satış - Market Stoğu Düşer)
        public static void SellFromMarket(string barcode, string storeId, int qtyToSell, string roomInfo = "")
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // Barkoddan productId al
                int productId = 0;
                using (var cmd = new MySqlCommand("SELECT ProductID FROM PRODUCTS WHERE Barcode=@b", conn, tr)) {
                    cmd.Parameters.AddWithValue("@b", barcode);
                    var res = cmd.ExecuteScalar();
                    if (res == null) throw new Exception("Bu barkoda ait ürün bulunamadı!");
                    productId = Convert.ToInt32(res);
                }

                int marketQty = 0;
                using (var chk = new MySqlCommand("SELECT Quantity FROM MARKET_STOCKS WHERE ProductID=@id AND StoreID=@s", conn, tr)) {
                    chk.Parameters.AddWithValue("@id", productId);
                    chk.Parameters.AddWithValue("@s", storeId);
                    var res = chk.ExecuteScalar();
                    if (res == null) throw new Exception("Mağazada bu ürün bulunmamaktadır!");
                    marketQty = Convert.ToInt32(res);
                }

                if (marketQty < qtyToSell) throw new Exception($"Mağazada yeterli stok yok! (Mevcut: {marketQty})");

                // Market stoğundan düş (--)
                using (var upd = new MySqlCommand("UPDATE MARKET_STOCKS SET Quantity = Quantity - @q WHERE ProductID=@id AND StoreID=@s", conn, tr)) {
                    upd.Parameters.AddWithValue("@q", qtyToSell);
                    upd.Parameters.AddWithValue("@id", productId);
                    upd.Parameters.AddWithValue("@s", storeId);
                    upd.ExecuteNonQuery();
                }

                // 2) Satiş Fiyatini al
                decimal unitPrice = 0;
                using (var cmdP = new MySqlCommand("SELECT Price FROM MARKET_STOCKS WHERE ProductID=@id AND StoreID=@s", conn, tr)) {
                    cmdP.Parameters.AddWithValue("@id", productId);
                    cmdP.Parameters.AddWithValue("@s", storeId);
                    unitPrice = Convert.ToDecimal(cmdP.ExecuteScalar() ?? 0);
                }

                // 3) Satış Logu (SALES_LOG)
                using (var log = new MySqlCommand("INSERT INTO SALES_LOG (ProductID, StoreID, Quantity, UnitPrice, TotalPrice, RoomInfo, Status) VALUES (@id, @s, @q, @up, @tp, @ri, 'Pending')", conn, tr)) {
                    log.Parameters.AddWithValue("@id", productId);
                    log.Parameters.AddWithValue("@s", storeId);
                    log.Parameters.AddWithValue("@q", qtyToSell);
                    log.Parameters.AddWithValue("@up", unitPrice);
                    log.Parameters.AddWithValue("@tp", unitPrice * qtyToSell);
                    log.Parameters.AddWithValue("@ri", roomInfo ?? "");
                    log.ExecuteNonQuery();
                }

                // Opsiyonel: Eğer bir odaya yazılmışsa, rezervasyonun toplam borcuna da eklenebilir.
                // (Müşterinin talebine göre buraya ek mantık konulabilir)

                tr.Commit();
            }
            catch { tr.Rollback(); throw; }
        }

        public static DataTable GetAllSalesLogs()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                @"SELECT 
                    sl.SaleID, 
                    p.ItemName AS 'Ürün Adı', 
                    sl.Quantity AS 'Adet', 
                    sl.UnitPrice AS 'Birim Fiyat', 
                    sl.TotalPrice AS 'Toplam Tutar', 
                    sl.RoomInfo AS 'Oda / Müşteri Bilgisi',
                    sl.SaleDate AS 'Satış Tarihi'
                  FROM SALES_LOG sl
                  JOIN PRODUCTS p ON sl.ProductID = p.ProductID
                  ORDER BY sl.SaleDate DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static decimal GetTotalMarketRevenue()
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalPrice),0) FROM SALES_LOG", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static DataTable GetLokantaSalesForGuest(string roomInfo)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // RoomInfo tam eşleşme ve IsPaid = 0 kontrolü
            var cmd = new MySqlCommand(@"SELECT sl.SaleID, p.ItemName, sl.Quantity, sl.UnitPrice, sl.TotalPrice, sl.SaleDate 
                                         FROM SALES_LOG sl JOIN PRODUCTS p ON sl.ProductID = p.ProductID 
                                         WHERE sl.RoomInfo = @ri AND (sl.IsPaid = 0 OR sl.IsPaid IS NULL) 
                                         ORDER BY sl.SaleDate DESC", conn);
            cmd.Parameters.AddWithValue("@ri", roomInfo);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static decimal GetLokantaTotalForGuest(string roomInfo)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalPrice), 0) FROM SALES_LOG WHERE RoomInfo = @ri AND (IsPaid = 0 OR IsPaid IS NULL)", conn);
            cmd.Parameters.AddWithValue("@ri", roomInfo);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static void MarkLokantaSalesAsPaid(string roomInfo)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE SALES_LOG SET IsPaid = 1 WHERE RoomInfo = @ri AND (IsPaid = 0 OR IsPaid IS NULL)", conn);
            cmd.Parameters.AddWithValue("@ri", roomInfo);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateMarketPrice(string barcode, string storeId, decimal newPrice)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // 1) Get ProductID from barcode
            int productId = 0;
            using (var cmd = new MySqlCommand("SELECT ProductID FROM PRODUCTS WHERE Barcode=@b", conn)) {
                cmd.Parameters.AddWithValue("@b", barcode);
                productId = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }
            if (productId == 0) return;

            // 2) Update price in MARKET_STOCKS
            using var upd = new MySqlCommand("UPDATE MARKET_STOCKS SET Price=@p WHERE ProductID=@id AND StoreID=@s", conn);
            upd.Parameters.AddWithValue("@p", newPrice);
            upd.Parameters.AddWithValue("@id", productId);
            upd.Parameters.AddWithValue("@s", storeId);
            upd.ExecuteNonQuery();
        }

        public static DataTable GetStockMovements(string toLocation = null)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"
                SELECT 
                    t.TransferID, 
                    p.ItemName AS 'Ürün', 
                    t.FromLocation AS 'Nereden', 
                    t.ToLocation AS 'Nereye', 
                    t.Quantity AS 'Adet', 
                    t.TransferDate AS 'Tarih', 
                    t.Notes AS 'Notlar'
                FROM STOCK_TRANSFERS t
                JOIN PRODUCTS p ON t.ProductID = p.ProductID";
            
            if (!string.IsNullOrEmpty(toLocation)) sql += " WHERE t.ToLocation = @loc";
            sql += " ORDER BY t.TransferDate DESC";

            using var cmd = new MySqlCommand(sql, conn);
            if (!string.IsNullOrEmpty(toLocation)) cmd.Parameters.AddWithValue("@loc", toLocation);
            
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void TruncateStorage() {
             using var conn = DatabaseHelper.GetConnection(); conn.Open();
             using var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS=0; DELETE FROM STOCK_TRANSFERS; DELETE FROM MARKET_STOCKS; DELETE FROM STORAGE_STOCKS; DELETE FROM PRODUCTS; SET FOREIGN_KEY_CHECKS=1;", conn);
             cmd.ExecuteNonQuery();
        }

        public static void TruncateMarket() {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM MARKET_STOCKS", conn); // Assuming MARKET_STOCKS is the new market table
            cmd.ExecuteNonQuery();
        }

        // ═══════ REPORTS ═══════
        public static decimal GetTotalRevenue() {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalAmount),0) FROM RESERVATIONS", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static DataTable GetTopSellingProducts() {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            // Basitlik için STOCK_TRANSFERS içinden 'MARKET_1'e gidenleri de sayabiliriz veya satış logu gerekiyordu.
            // Şimdilik TRANSFER üzerinden bir özet gösterelim.
            using var cmd = new MySqlCommand(@"SELECT p.ItemName, SUM(t.Quantity) as Total 
                FROM STOCK_TRANSFERS t JOIN PRODUCTS p ON t.ProductID=p.ProductID 
                GROUP BY p.ProductID ORDER BY Total DESC LIMIT 5", conn);
            using var da = new MySqlDataAdapter(cmd); da.Fill(dt); return dt;
        }

        public static DataTable GetLowStocks() {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"SELECT ItemName, Quantity FROM STORAGE_STOCKS s 
                JOIN PRODUCTS p ON s.ProductID=p.ProductID WHERE Quantity < 10", conn);
            using var da = new MySqlDataAdapter(cmd); da.Fill(dt); return dt;
        }

        public static DataTable GetMonthlyRevenue(int months = 6) {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"SELECT DATE_FORMAT(CreatedAt, '%Y-%m') as Month, SUM(TotalAmount) as Revenue 
                FROM RESERVATIONS WHERE Status IN ('CheckedIn', 'Completed') 
                AND CreatedAt >= DATE_SUB(CURDATE(), INTERVAL @m MONTH) 
                GROUP BY Month ORDER BY Month ASC", conn);
            cmd.Parameters.AddWithValue("@m", months);
            using var da = new MySqlDataAdapter(cmd); da.Fill(dt); return dt;
        }

        public static DataTable GetRoomDistribution() {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"SELECT Status, COUNT(*) as Count FROM ROOMS GROUP BY Status", conn);
            using var da = new MySqlDataAdapter(cmd); da.Fill(dt); return dt;
        }

        public static DataTable GetOccupancyTrends(int days = 30) {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"SELECT DATE(CreatedAt) as Day, COUNT(*) as Count 
                FROM RESERVATIONS WHERE CreatedAt >= DATE_SUB(CURDATE(), INTERVAL @d DAY) 
                GROUP BY Day ORDER BY Day ASC", conn);
            cmd.Parameters.AddWithValue("@d", days);
            using var da = new MySqlDataAdapter(cmd); da.Fill(dt); return dt;
        }
        public static decimal GetPaidTotalForReservation(int reservationId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalAmount), 0) FROM PAYMENTS WHERE ReservationID = @rid", conn);
            cmd.Parameters.AddWithValue("@rid", reservationId);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static void RecordPayment(int reservationId, decimal totalPaid, string method)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // 1) Record in PAYMENTS table
                using (var cmd = new MySqlCommand("INSERT INTO PAYMENTS (ReservationID, TotalAmount, PaymentMethod) VALUES (@rid, @ta, @pm)", conn, tr)) {
                    cmd.Parameters.AddWithValue("@rid", reservationId);
                    cmd.Parameters.AddWithValue("@ta", totalPaid);
                    cmd.Parameters.AddWithValue("@pm", method);
                    cmd.ExecuteNonQuery();
                }

                // 2) Update PaidAmount in RESERVATIONS table
                using (var cmdUpd = new MySqlCommand("UPDATE RESERVATIONS SET PaidAmount = PaidAmount + @pa WHERE ReservationID = @rid", conn, tr)) {
                    cmdUpd.Parameters.AddWithValue("@pa", totalPaid);
                    cmdUpd.Parameters.AddWithValue("@rid", reservationId);
                    cmdUpd.ExecuteNonQuery();
                }

                tr.Commit();
                LogActivity("Ödeme", $"{reservationId} nolu rezervasyon için {totalPaid:C2} tutarında ödeme alındı ({method}).");
            } catch { tr.Rollback(); throw; }
        }

        public static void LogActivity(string type, string description)
        {
            try {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new MySqlCommand("INSERT INTO ACTIVITY_LOG (ActivityType, Description) VALUES (@t, @d)", conn);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.Parameters.AddWithValue("@d", description);
                cmd.ExecuteNonQuery();
            } catch { } // Silently fail to not block main logic
        }

        public static DataTable GetRecentActivities(int limit = 50)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT CreatedAt AS Tarih, ActivityType AS Tip, Description AS Detay FROM ACTIVITY_LOG ORDER BY CreatedAt DESC LIMIT @l", conn);
            cmd.Parameters.AddWithValue("@l", limit);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetCheckedOutArchive()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT 
                    r.RoomNumber AS 'Oda',
                    CONCAT(c.FirstName, ' ', c.LastName) AS 'Müşteri',
                    res.CheckInDate AS 'Giriş',
                    res.CheckOutDate AS 'Çıkış',
                    DATEDIFF(res.CheckOutDate, res.CheckInDate) AS 'Gün',
                    res.TotalAmount AS 'Borç',
                    (SELECT IFNULL(SUM(TotalAmount), 0) FROM PAYMENTS p WHERE p.ReservationID = res.ReservationID) AS 'Ödenen'
                FROM RESERVATIONS res
                JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                JOIN ROOMS r ON res.RoomID = r.RoomID
                WHERE res.Status = 'CheckedOut' OR res.Status = 'Completed'
                ORDER BY res.CheckOutDate DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetTodayOperations()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                (SELECT '💰 Ödeme' as Tip, CONCAT(pm.PaymentMethod, ' - Oda ', r.RoomNumber) as Detay, pm.TotalAmount as Tutar, pm.PaymentDate as Tarih 
                 FROM PAYMENTS pm 
                 JOIN RESERVATIONS res ON pm.ReservationID = res.ReservationID 
                 JOIN ROOMS r ON res.RoomID = r.RoomID
                 WHERE DATE(pm.PaymentDate) = CURDATE())
                UNION ALL
                (SELECT '🛒 Market' as Tip, sl.RoomInfo as Detay, sl.TotalPrice as Tutar, sl.SaleDate as Tarih 
                 FROM SALES_LOG sl
                 WHERE DATE(sl.SaleDate) = CURDATE())
                ORDER BY Tarih DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static decimal GetTotalMarketRevenueToday()
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalPrice),0) FROM SALES_LOG WHERE DATE(SaleDate) = CURDATE()", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static decimal GetTotalRevenueToday()
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalAmount),0) FROM PAYMENTS WHERE DATE(PaymentDate) = CURDATE()", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        // ═══════ WEEKLY REPORT DATA ═══════

        public static DataTable GetWeeklyPayments(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT 
                    pm.PaymentDate AS 'Tarih',
                    r.RoomNumber AS 'Oda',
                    CONCAT(c.FirstName, ' ', c.LastName) AS 'Müşteri',
                    pm.PaymentMethod AS 'Yöntem',
                    pm.TotalAmount AS 'Tutar'
                FROM PAYMENTS pm
                JOIN RESERVATIONS res ON pm.ReservationID = res.ReservationID
                JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                JOIN ROOMS r ON res.RoomID = r.RoomID
                WHERE DATE(pm.PaymentDate) BETWEEN @s AND @e
                ORDER BY pm.PaymentDate DESC", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static decimal GetWeeklyAccommodationIncome(DateTime start, DateTime end)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalAmount),0) FROM PAYMENTS WHERE DATE(PaymentDate) BETWEEN @s AND @e", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static decimal GetWeeklyRestaurantIncome(DateTime start, DateTime end)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalPrice),0) FROM SALES_LOG WHERE DATE(SaleDate) BETWEEN @s AND @e", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        // --- NEW PREMIUM REPORTING METHODS ---

        public static decimal GetRevenueRange(DateTime start, DateTime end)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalAmount),0) FROM PAYMENTS WHERE DATE(PaymentDate) BETWEEN @s AND @e", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static decimal GetMarketRevenueRange(DateTime start, DateTime end)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            // Only count sales that ARE NOT linked to a room (immediate cash/walk-in sales)
            // Guest restaurant sales are counted when they pay their room bill (PAYMENTS table)
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(TotalPrice),0) FROM SALES_LOG WHERE (RoomInfo = '' OR RoomInfo IS NULL) AND DATE(SaleDate) BETWEEN @s AND @e", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static double GetOccupancyRate(DateTime start, DateTime end)
        {
            // Simple calculation: (Total Full Days / (Total Rooms * Total Days)) * 100
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            
            // Get total room count
            using var cmdTotal = new MySqlCommand("SELECT COUNT(*) FROM ROOMS", conn);
            int totalRooms = Convert.ToInt32(cmdTotal.ExecuteScalar());
            if (totalRooms == 0) return 0;

            int totalReportDays = Math.Max(1, (int)(end - start).TotalDays + 1);

            // Get count of occupied dates in range
            using var cmdOcc = new MySqlCommand(@"
                SELECT COUNT(*) FROM (
                    SELECT res.RoomID, d.date
                    FROM RESERVATIONS res
                    JOIN (
                        SELECT @row := @row + 1 as r, DATE_ADD(@s, INTERVAL @row DAY) as date
                        FROM (SELECT 0 UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION SELECT 6) t1,
                             (SELECT 0 UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION SELECT 6) t2,
                             (SELECT @row:=-1, @s:=@start_date) r_vars
                        LIMIT 100
                    ) d ON d.date >= res.CheckInDate AND d.date < res.CheckOutDate
                    WHERE d.date BETWEEN @start_date AND @end_date
                ) AS occupied_days", conn);
            
            cmdOcc.Parameters.AddWithValue("@start_date", start.ToString("yyyy-MM-dd"));
            cmdOcc.Parameters.AddWithValue("@end_date", end.ToString("yyyy-MM-dd"));
            
            double occupiedCount = Convert.ToDouble(cmdOcc.ExecuteScalar());
            return Math.Min(100, (occupiedCount / (totalRooms * totalReportDays)) * 100);
        }

        public static decimal GetAverageDailyRate(DateTime start, DateTime end)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT IFNULL(AVG(TotalAmount / NULLIF(DATEDIFF(CheckOutDate, CheckInDate), 0)), 0) 
                FROM RESERVATIONS 
                WHERE CheckOutDate BETWEEN @s AND @e", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static DataTable GetPaymentMethodDistribution(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT PaymentMethod as 'Yöntem', SUM(TotalAmount) as 'Toplam'
                FROM PAYMENTS 
                WHERE DATE(PaymentDate) BETWEEN @s AND @e
                GROUP BY PaymentMethod", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetTopSellingProducts(DateTime start, DateTime end, int count = 5)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT p.ItemName as 'Ürün', SUM(sl.Quantity) as 'Adet', SUM(sl.TotalPrice) as 'Ciro'
                FROM SALES_LOG sl
                JOIN PRODUCTS p ON sl.ProductID = p.ProductID
                WHERE DATE(sl.SaleDate) BETWEEN @s AND @e
                GROUP BY p.ItemName
                ORDER BY Adet DESC
                LIMIT @c", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@c", count);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetWeeklyCheckIns(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT 
                    r.RoomNumber AS 'Oda',
                    CONCAT(c.FirstName, ' ', c.LastName) AS 'Müşteri',
                    res.CheckInDate AS 'Giriş',
                    res.CheckOutDate AS 'Çıkış',
                    res.TotalAmount AS 'Tutar'
                FROM RESERVATIONS res
                JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                JOIN ROOMS r ON res.RoomID = r.RoomID
                WHERE DATE(res.CheckInDate) BETWEEN @s AND @e
                ORDER BY res.CheckInDate ASC", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetWeeklyCheckOuts(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT 
                    r.RoomNumber AS 'Oda',
                    CONCAT(c.FirstName, ' ', c.LastName) AS 'Müşteri',
                    res.CheckInDate AS 'Giriş',
                    res.CheckOutDate AS 'Çıkış',
                    res.TotalAmount AS 'Tutar'
                FROM RESERVATIONS res
                JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                JOIN ROOMS r ON res.RoomID = r.RoomID
                WHERE DATE(res.CheckOutDate) BETWEEN @s AND @e
                AND res.Status IN ('CheckedOut', 'Completed')
                ORDER BY res.CheckOutDate ASC", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetWeeklyTopSales(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT 
                    p.ItemName AS 'Ürün',
                    SUM(sl.Quantity) AS 'Adet',
                    SUM(sl.TotalPrice) AS 'Toplam'
                FROM SALES_LOG sl
                JOIN PRODUCTS p ON sl.ProductID = p.ProductID
                WHERE DATE(sl.SaleDate) BETWEEN @s AND @e
                GROUP BY p.ProductID, p.ItemName
                ORDER BY Toplam DESC
                LIMIT 10", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetRooms()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM ROOMS ORDER BY RoomNumber", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetRecentSales(int count)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT 
                    sl.SaleDate AS 'Tarih',
                    sl.RoomInfo AS 'Oda/Misafir',
                    p.ItemName AS 'Ürün',
                    sl.Quantity AS 'Adet',
                    sl.TotalPrice AS 'Tutar'
                FROM SALES_LOG sl
                JOIN PRODUCTS p ON sl.ProductID = p.ProductID
                ORDER BY sl.SaleDate DESC
                LIMIT @c", conn);
            cmd.Parameters.AddWithValue("@c", count);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        // --- EXPENSE MANAGEMENT ---

        public static void AddExpense(string title, string category, decimal amount, string desc, string paidBy)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("INSERT INTO EXPENSES (Title, Category, Amount, Description, PaidBy) VALUES (@t, @c, @a, @d, @pb)", conn);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@c", category);
            cmd.Parameters.AddWithValue("@a", amount);
            cmd.Parameters.AddWithValue("@d", desc);
            cmd.Parameters.AddWithValue("@pb", paidBy);
            cmd.ExecuteNonQuery();
        }

        public static DataTable GetExpenses(DateTime s, DateTime e)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM EXPENSES WHERE DATE(ExpenseDate) BETWEEN @s AND @e ORDER BY ExpenseDate DESC", conn);
            cmd.Parameters.AddWithValue("@s", s.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", e.ToString("yyyy-MM-dd"));
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static decimal GetTotalExpensesRange(DateTime start, DateTime end)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(Amount),0) FROM EXPENSES WHERE DATE(ExpenseDate) BETWEEN @s AND @e", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        // --- KBS (IDENTITY) REPORTING ---

        public static DataTable GetKbsIdentityReport()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            // Currently checked-in guests
            using var cmd = new MySqlCommand(@"
                SELECT 
                    c.IdentityNumber AS 'T.C. No',
                    c.FirstName AS 'Ad',
                    c.LastName AS 'Soyad',
                    r.RoomNumber AS 'Oda',
                    res.CheckInDate AS 'Giriş'
                FROM RESERVATIONS res
                JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                JOIN ROOMS r ON res.RoomID = r.RoomID
                WHERE res.Status = 'CheckedIn'
                ORDER BY r.RoomNumber ASC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        // --- MAINTENANCE LOGS ---

        public static void AddMaintenanceLog(int roomId, string desc, string tech, decimal cost)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("INSERT INTO MAINTENANCE_LOGS (RoomID, FaultDescription, TechnicianName, Cost, Status) VALUES (@rid, @d, @t, @c, 'Pending')", conn);
            cmd.Parameters.AddWithValue("@rid", roomId);
            cmd.Parameters.AddWithValue("@d", desc);
            cmd.Parameters.AddWithValue("@t", tech);
            cmd.Parameters.AddWithValue("@c", cost);
            cmd.ExecuteNonQuery();

            // Update room status
            using var cmdRoom = new MySqlCommand("UPDATE ROOMS SET Status='Maintenance' WHERE RoomID=@rid", conn);
            cmdRoom.Parameters.AddWithValue("@rid", roomId);
            cmdRoom.ExecuteNonQuery();
        }

        public static DataTable GetActiveEmployees()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM EMPLOYEES WHERE IsActive=1 ORDER BY FirstName, LastName", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetMaintenanceLogs()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT ml.*, r.RoomNumber 
                FROM MAINTENANCE_LOGS ml 
                JOIN ROOMS r ON ml.RoomID = r.RoomID 
                ORDER BY ml.ReportedDate DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        // --- CORPORATE & COMPANIES ---

        public static DataTable GetCompanies()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM COMPANIES ORDER BY CompanyName", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void AddCompany(string name, string taxNo, string taxOff, string addr, string phone)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("INSERT INTO COMPANIES (CompanyName, TaxNumber, TaxOffice, Address, Phone) VALUES (@n, @tn, @to, @a, @p)", conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@tn", taxNo);
            cmd.Parameters.AddWithValue("@to", taxOff);
            cmd.Parameters.AddWithValue("@a", addr);
            cmd.Parameters.AddWithValue("@p", phone);
            cmd.ExecuteNonQuery();
        }

        // --- EXTRA SERVICES (Laundry, Transfer etc) ---

        public static void AddServiceToReservation(int resId, string name, decimal cost, string desc)
        {
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("INSERT INTO SERVICES (ReservationID, ServiceName, Cost, Description) VALUES (@rid, @n, @c, @d)", conn);
            cmd.Parameters.AddWithValue("@rid", resId);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@c", cost);
            cmd.Parameters.AddWithValue("@d", desc ?? "");
            cmd.ExecuteNonQuery();
        }

        public static DataTable GetReservationServices(int resId)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT ServiceName as 'Hizmet', Cost as 'Tutar', ServiceDate as 'Tarih' FROM SERVICES WHERE ReservationID = @rid", conn);
            cmd.Parameters.AddWithValue("@rid", resId);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetChannelDistribution(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT ChannelName AS 'Kanal', COUNT(*) AS 'Rezervasyon', SUM(TotalAmount) AS 'Ciro', SUM(CommissionAmount) AS 'Komisyon'
                FROM RESERVATIONS
                WHERE DATE(CreatedAt) BETWEEN @s AND @e
                GROUP BY ChannelName
                ORDER BY Ciro DESC", conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }
        public static DataTable GetAgencyStats()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT ChannelName as Acente, 
                       COUNT(*) as RezervasyonSayisi, 
                       SUM(TotalAmount) as ToplamCiro, 
                       SUM(CommissionAmount) as ToplamKomisyon
                FROM RESERVATIONS
                WHERE Status != 'Cancelled'
                GROUP BY ChannelName
                ORDER BY ToplamCiro ASC
            ", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetRestaurantTables()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM RESTAURANT_TABLES ORDER BY TableID", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void UpdateTableStatus(int tableId, string status, int? reservationId = null)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("UPDATE RESTAURANT_TABLES SET Status=@s, CurrentReservationID=@r WHERE TableID=@id", conn);
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@r", reservationId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id", tableId);
            cmd.ExecuteNonQuery();
        }

        // ═══════ KITCHEN (KDS) ═══════
        public static DataTable GetKitchenOrders()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // Get orders that are not yet served
            using var cmd = new MySqlCommand(@"
                SELECT 
                    sl.SaleID, 
                    p.ItemName, 
                    sl.Quantity, 
                    sl.RoomInfo, 
                    sl.Status, 
                    sl.SaleDate
                FROM SALES_LOG sl
                JOIN PRODUCTS p ON sl.ProductID = p.ProductID
                WHERE sl.Status != 'Served'
                ORDER BY sl.SaleDate DESC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static void UpdateSaleStatus(int saleId, string status)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("UPDATE SALES_LOG SET Status=@s WHERE SaleID=@id", conn);
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@id", saleId);
            cmd.ExecuteNonQuery();
        }

        // ═══════ REPORTING (MODERN) ═══════
        public static DataTable GetRoomIncomeReport(DateTime start, DateTime end, string roomNumber = null)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            string sql = @"
                SELECT 
                    rm.RoomNumber AS 'Oda No',
                    CONCAT(c.FirstName, ' ', c.LastName) AS 'Müşteri',
                    res.CheckInDate AS 'Giriş Tarihi',
                    res.CheckOutDate AS 'Çıkış Tarihi',
                    res.TotalAmount AS 'Ücret'
                FROM RESERVATIONS res
                JOIN ROOMS rm ON res.RoomID = rm.RoomID
                JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                WHERE DATE(res.CheckInDate) BETWEEN @s AND @e";
            if (!string.IsNullOrEmpty(roomNumber) && roomNumber != "Tüm Odalar")
                sql += " AND rm.RoomNumber = @rn";
            sql += " ORDER BY res.CheckInDate DESC";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(roomNumber) && roomNumber != "Tüm Odalar")
                cmd.Parameters.AddWithValue("@rn", roomNumber);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetRestaurantIncomeReport(DateTime start, DateTime end, string roomNumber = null)
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            string sql = @"
                SELECT 
                    sl.SaleDate AS 'Satış Tarihi',
                    p.ItemName AS 'Ürün Adı',
                    sl.Quantity AS 'Adet',
                    sl.UnitPrice AS 'Birim Fiyat',
                    sl.TotalPrice AS 'Toplam Fiyat',
                    sl.RoomInfo AS 'Oda/Masa'
                FROM SALES_LOG sl
                JOIN PRODUCTS p ON sl.ProductID = p.ProductID
                WHERE DATE(sl.SaleDate) BETWEEN @s AND @e";
            if (!string.IsNullOrEmpty(roomNumber) && roomNumber != "Tüm Odalar")
                sql += " AND sl.RoomInfo LIKE @rn";
            sql += " ORDER BY sl.SaleDate DESC";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", end.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(roomNumber) && roomNumber != "Tüm Odalar")
                cmd.Parameters.AddWithValue("@rn", "%Oda " + roomNumber + "%");
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }
        public static DataTable GetRoomTypes()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM ROOM_TYPES ORDER BY TypeName", conn);
            using var da = new MySqlDataAdapter(cmd); da.Fill(dt); return dt;
        }

        public static DataTable GetFloors()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection(); conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM FLOORS ORDER BY FloorNumber", conn);
            using var da = new MySqlDataAdapter(cmd); da.Fill(dt); return dt;
        }

        // ═══════ CRM SCHEMA MIGRATION ═══════
        /// <summary>
        /// Ensures CRM columns (Preferences, VipStatus, Allergies) exist on the CUSTOMERS table.
        /// Call once at application startup.
        /// </summary>
        public static void EnsureCrmColumns()
        {
            try {
                using var conn = DatabaseHelper.GetConnection(); conn.Open();
                var cols = new[] {
                    ("Preferences",  "TEXT"),
                    ("VipStatus",    "VARCHAR(50) DEFAULT 'Normal'"),
                    ("Allergies",    "TEXT")
                };
                foreach (var (col, def) in cols) {
                    using var chk = new MySqlCommand(
                        $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='CUSTOMERS' AND COLUMN_NAME='{col}'", conn);
                    if (Convert.ToInt32(chk.ExecuteScalar()) == 0) {
                        using var alter = new MySqlCommand($"ALTER TABLE CUSTOMERS ADD COLUMN {col} {def}", conn);
                        alter.ExecuteNonQuery();
                    }
                }
            } catch { /* Non-fatal: fallback in UpdateGuestCrmProfile already handles missing cols */ }
        }
    }
}
