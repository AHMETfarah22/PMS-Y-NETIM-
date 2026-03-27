using MySql.Data.MySqlClient;
using System.Data;

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
                IFNULL(rt.TypeName,'Standart') AS OdaTipi, IFNULL(rt.BasePrice,0) AS Fiyat
                FROM ROOMS r 
                LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID=rt.RoomTypeID 
                ORDER BY r.RoomNumber", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetAvailableRooms()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT r.RoomNumber, f.FloorNumber, IFNULL(rt.TypeName,'Standart') as TypeName, IFNULL(rt.BasePrice,0) as Price
                FROM ROOMS r 
                JOIN FLOORS f ON r.FloorID = f.FloorID
                LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE r.Status != 'Occupied' 
                ORDER BY r.RoomNumber", conn);
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

        // ═══════ CUSTOMERS ═══════
        public static int AddCustomer(string firstName, string lastName, string phone, string email, string roomNumber, int bedNumber, string address = "", string idNo = "")
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            if (!string.IsNullOrWhiteSpace(idNo)) {
                using var chk = new MySqlCommand("SELECT CustomerID FROM CUSTOMERS WHERE IdentityNumber=@idn", conn);
                chk.Parameters.AddWithValue("@idn", idNo);
                var existingId = chk.ExecuteScalar();
                if (existingId != null) {
                    using var upd = new MySqlCommand(@"UPDATE CUSTOMERS SET FirstName=@fn, LastName=@ln, Phone=@ph, Email=@em, RoomNumber=@rn, BedNumber=@bn, Address=@ad WHERE CustomerID=@cid", conn);
                    upd.Parameters.AddWithValue("@fn", firstName);
                    upd.Parameters.AddWithValue("@ln", lastName);
                    upd.Parameters.AddWithValue("@ph", phone);
                    upd.Parameters.AddWithValue("@em", email);
                    upd.Parameters.AddWithValue("@rn", roomNumber);
                    upd.Parameters.AddWithValue("@bn", bedNumber);
                    upd.Parameters.AddWithValue("@ad", address);
                    upd.Parameters.AddWithValue("@cid", existingId);
                    upd.ExecuteNonQuery();
                    return Convert.ToInt32(existingId);
                }
            }

            using var cmd = new MySqlCommand(@"INSERT INTO CUSTOMERS (FirstName, LastName, Phone, Email, RoomNumber, BedNumber, Address, IdentityNumber) 
                VALUES (@fn, @ln, @ph, @em, @rn, @bn, @ad, @idn); SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@fn", firstName);
            cmd.Parameters.AddWithValue("@ln", lastName);
            cmd.Parameters.AddWithValue("@ph", phone);
            cmd.Parameters.AddWithValue("@em", email);
            cmd.Parameters.AddWithValue("@rn", roomNumber);
            cmd.Parameters.AddWithValue("@bn", bedNumber);
            cmd.Parameters.AddWithValue("@ad", address);
            cmd.Parameters.AddWithValue("@idn", idNo);
            return Convert.ToInt32(cmd.ExecuteScalar());
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

        public static bool IsCustomerStaying(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber)) return false;
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM RESERVATIONS r
                JOIN CUSTOMERS c ON r.CustomerID = c.CustomerID
                WHERE c.IdentityNumber = @idn AND r.Status = 'CheckedIn'
                AND @today BETWEEN r.CheckInDate AND r.CheckOutDate", conn);
            cmd.Parameters.AddWithValue("@idn", identityNumber);
            cmd.Parameters.AddWithValue("@today", DateTime.Today.ToString("yyyy-MM-dd"));
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public static DataTable GetAllCustomers()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT CustomerID, IdentityNumber, FirstName, LastName, Phone, Email, RoomNumber, BedNumber, CreatedAt FROM CUSTOMERS ORDER BY CustomerID ASC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
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

        public static void AddReservation(int customerId, string roomNumber, int bedNumber, DateTime checkIn, DateTime checkOut)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            // Get RoomID and Price
            using var cmdRoom = new MySqlCommand(@"SELECT r.RoomID, IFNULL(rt.BasePrice,0) AS Price 
                FROM ROOMS r LEFT JOIN ROOM_TYPES rt ON r.RoomTypeID=rt.RoomTypeID 
                WHERE r.RoomNumber=@r", conn);
            cmdRoom.Parameters.AddWithValue("@r", roomNumber);
            using var reader = cmdRoom.ExecuteReader();
            int roomId = 0; decimal price = 0;
            if (reader.Read()) { roomId = reader.GetInt32(0); price = reader.GetDecimal(1); }
            reader.Close();

            using var cmd = new MySqlCommand(@"INSERT INTO RESERVATIONS (CustomerID, RoomID, BedNumber, CheckInDate, CheckOutDate, Status, TotalAmount) 
                VALUES (@cid, @rid, @bn, @ci, @co, 'CheckedIn', @amt)", conn);
            cmd.Parameters.AddWithValue("@cid", customerId);
            cmd.Parameters.AddWithValue("@rid", roomId);
            cmd.Parameters.AddWithValue("@bn", bedNumber);
            cmd.Parameters.AddWithValue("@ci", checkIn.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@co", checkOut.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@amt", price);
            cmd.ExecuteNonQuery();

            // Update room occupancy
            using var cmdOcc = new MySqlCommand("SELECT OccupiedBeds FROM ROOMS WHERE RoomNumber=@r", conn);
            cmdOcc.Parameters.AddWithValue("@r", roomNumber);
            int current = Convert.ToInt32(cmdOcc.ExecuteScalar());
            UpdateRoomOccupancy(roomNumber, current + 1);
        }

        public static DataTable GetReservations()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"SELECT r.ReservationID, 
                CONCAT(c.FirstName,' ',c.LastName) AS Musteri, 
                rm.RoomNumber AS Oda, 
                r.BedNumber AS Yatak,
                IFNULL(rt.TypeName,'Standart') AS OdaTipi,
                IFNULL(rt.BasePrice,0) AS Fiyat,
                r.CheckInDate AS Giris, 
                r.CheckOutDate AS Cikis,
                r.Status 
                FROM RESERVATIONS r 
                JOIN CUSTOMERS c ON r.CustomerID=c.CustomerID 
                JOIN ROOMS rm ON r.RoomID=rm.RoomID 
                LEFT JOIN ROOM_TYPES rt ON rm.RoomTypeID=rt.RoomTypeID
                ORDER BY r.CreatedAt ASC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
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

        // DEPO (STORAGE) SAYFASI İÇİN LİSTE (Tüm Ürünler ve Depo Stokları)
        public static DataTable GetAllStorageStocks()
        {
            var dt = new DataTable();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(
                @"SELECT 
                    p.ProductID, 
                    p.Barcode, 
                    p.ItemName, 
                    p.Price, 
                    IFNULL(s.Location, '-') AS Location, 
                    IFNULL(s.Quantity, 0) AS StorageQuantity
                  FROM PRODUCTS p
                  LEFT JOIN STORAGE_STOCKS s ON p.ProductID = s.ProductID
                  ORDER BY p.ProductID ASC", conn);
            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        // DEPOYA ÜRÜN GİRİŞİ (Yoksa Ürün Yaratır, Varsa Sadece Depo Stoğunu Arttırır)
        public static void AddOrUpdateStorageItem(string barcode, string itemName, decimal price, string location, int qtyToAdd)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tr = conn.BeginTransaction();
            try {
                // 1) Barkoda göre ürünü bul veya yarat
                int productId = 0;
                using (var cmd = new MySqlCommand("SELECT ProductID FROM PRODUCTS WHERE Barcode = @b", conn, tr)) {
                    cmd.Parameters.AddWithValue("@b", barcode);
                    var res = cmd.ExecuteScalar();
                    if (res != null) productId = Convert.ToInt32(res);
                }

                if (productId == 0) {
                    using var cmdIns = new MySqlCommand("INSERT INTO PRODUCTS (Barcode, ItemName, Price) VALUES (@b, @n, @p); SELECT LAST_INSERT_ID();", conn, tr);
                    cmdIns.Parameters.AddWithValue("@b", barcode);
                    cmdIns.Parameters.AddWithValue("@n", itemName);
                    cmdIns.Parameters.AddWithValue("@p", price);
                    productId = Convert.ToInt32(cmdIns.ExecuteScalar());
                } else {
                    // Update main product details if changed
                    using var cmdUpd = new MySqlCommand("UPDATE PRODUCTS SET ItemName=@n, Price=@p WHERE ProductID=@id", conn, tr);
                    cmdUpd.Parameters.AddWithValue("@n", itemName);
                    cmdUpd.Parameters.AddWithValue("@p", price);
                    cmdUpd.Parameters.AddWithValue("@id", productId);
                    cmdUpd.ExecuteNonQuery();
                }

                // 2) Depo Stoguna (STORAGE_STOCKS) Miktarı Ekle (Quantity + qtyToAdd)
                using (var chk = new MySqlCommand("SELECT COUNT(*) FROM STORAGE_STOCKS WHERE ProductID=@id", conn, tr)) {
                    chk.Parameters.AddWithValue("@id", productId);
                    if (Convert.ToInt32(chk.ExecuteScalar()) > 0) {
                        using var upd = new MySqlCommand("UPDATE STORAGE_STOCKS SET Quantity = Quantity + @q, Location = @loc WHERE ProductID=@id", conn, tr);
                        upd.Parameters.AddWithValue("@q", qtyToAdd);
                        upd.Parameters.AddWithValue("@loc", location ?? "");
                        upd.Parameters.AddWithValue("@id", productId);
                        upd.ExecuteNonQuery();
                    } else {
                        using var ins = new MySqlCommand("INSERT INTO STORAGE_STOCKS (ProductID, Quantity, Location) VALUES (@id, @q, @loc)", conn, tr);
                        ins.Parameters.AddWithValue("@id", productId);
                        ins.Parameters.AddWithValue("@q", qtyToAdd);
                        ins.Parameters.AddWithValue("@loc", location ?? "");
                        ins.ExecuteNonQuery();
                    }
                }
                tr.Commit();
            }
            catch { tr.Rollback(); throw; }
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
                    p.Price, 
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

        // TRANFER: DEPODAN MARKETE GÖNDER (Depo Miktarı Düşer, Market Miktarı Artar)
        public static void TransferToMarket(int productId, string storeId, int qtyToTransfer, string notes = "")
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
                    if (Convert.ToInt32(chkMarket.ExecuteScalar()) > 0) {
                        using var updMkt = new MySqlCommand("UPDATE MARKET_STOCKS SET Quantity = Quantity + @q WHERE ProductID=@id AND StoreID=@s", conn, tr);
                        updMkt.Parameters.AddWithValue("@q", qtyToTransfer);
                        updMkt.Parameters.AddWithValue("@id", productId);
                        updMkt.Parameters.AddWithValue("@s", storeId);
                        updMkt.ExecuteNonQuery();
                    } else {
                        using var insMkt = new MySqlCommand("INSERT INTO MARKET_STOCKS (ProductID, StoreID, Quantity) VALUES (@id, @s, @q)", conn, tr);
                        insMkt.Parameters.AddWithValue("@id", productId);
                        insMkt.Parameters.AddWithValue("@s", storeId);
                        insMkt.Parameters.AddWithValue("@q", qtyToTransfer);
                        insMkt.ExecuteNonQuery();
                    }
                }

                // 4) Transfer Logu (STOCK_TRANSFERS)
                using (var log = new MySqlCommand("INSERT INTO STOCK_TRANSFERS (ProductID, FromLocation, ToLocation, Quantity, Notes) VALUES (@id, 'DEPO', @s, @q, @n)", conn, tr)) {
                    log.Parameters.AddWithValue("@id", productId);
                    log.Parameters.AddWithValue("@s", storeId);
                    log.Parameters.AddWithValue("@q", qtyToTransfer);
                    log.Parameters.AddWithValue("@n", notes);
                    log.ExecuteNonQuery();
                }
                tr.Commit();
            }
            catch { tr.Rollback(); throw; }
        }

        // SATIŞ YAP (Brakod ile Satış - Market Stoğu Düşer)
        public static void SellFromMarket(string barcode, string storeId, int qtyToSell)
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

                // (Opsiyonel) Satış log tablosuna da insert yapılabilir...

                tr.Commit();
            }
            catch { tr.Rollback(); throw; }
        }

        public static void TruncateStorage() {
             using var conn = DatabaseHelper.GetConnection(); conn.Open();
             using var cmd = new MySqlCommand("DELETE FROM STOCK_TRANSFERS; DELETE FROM MARKET_STOCKS; DELETE FROM STORAGE_STOCKS; DELETE FROM PRODUCTS;", conn);
             cmd.ExecuteNonQuery();
        }

        public static void TruncateMarket() {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM MARKET_STOCKS", conn); // Assuming MARKET_STOCKS is the new market table
            cmd.ExecuteNonQuery();
        }
    }
}
