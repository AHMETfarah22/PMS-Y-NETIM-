using MySql.Data.MySqlClient;

namespace PmsSystem.Database
{
    public static class DatabaseHelper
    {
        private static string Server = "127.0.0.1";
        private static string DbName = "pms_system";
        private static string User = "root";
        private static string Password = "";
        private static string Port = "3306";

        public static string ConnectionString =>
            $"Server={Server};Port={Port};Database={DbName};Uid={User};Pwd={Password};Allow User Variables=true;CharSet=utf8mb4;Connection Timeout=10;";

        public static string ConnectionStringNoDb =>
            $"Server={Server};Port={Port};Uid={User};Pwd={Password};Allow User Variables=true;Connection Timeout=10;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public static void InitializeDatabase()
        {
            // Önce veritabanını oluştur
            using (var conn = new MySqlConnection(ConnectionStringNoDb))
            {
                conn.Open();
                var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{DbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", conn);
                cmd.ExecuteNonQuery();
            }

            // Tabloları oluştur
            using (var conn = GetConnection())
            {
                conn.Open();

                string[] tables = {
                    @"CREATE TABLE IF NOT EXISTS FLOORS (
                        FloorID INT AUTO_INCREMENT PRIMARY KEY,
                        FloorNumber INT NOT NULL UNIQUE,
                        Description TEXT
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS ROOM_TYPES (
                        RoomTypeID INT AUTO_INCREMENT PRIMARY KEY,
                        TypeName VARCHAR(50) NOT NULL,
                        Description TEXT,
                        MaxOccupancy INT DEFAULT 2,
                        BasePrice DECIMAL(10,2) DEFAULT 0
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS USERS (
                        UserID INT AUTO_INCREMENT PRIMARY KEY,
                        Username VARCHAR(50) NOT NULL UNIQUE,
                        FullName VARCHAR(100) NOT NULL,
                        Email VARCHAR(100),
                        PasswordHash VARCHAR(255) NOT NULL,
                        Role VARCHAR(20) DEFAULT 'Kasiyer',
                        PhoneNumber VARCHAR(20),
                        IsActive TINYINT(1) DEFAULT 1,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"SET FOREIGN_KEY_CHECKS = 0;",
                    @"DROP TABLE IF EXISTS MARKET;",
                    @"DROP TABLE IF EXISTS STORAGE;",
                    @"SET FOREIGN_KEY_CHECKS = 1;",

                    @"CREATE TABLE IF NOT EXISTS PRODUCTS (
                        ProductID INT AUTO_INCREMENT PRIMARY KEY,
                        Barcode VARCHAR(50) UNIQUE NOT NULL,
                        ItemName VARCHAR(100) NOT NULL,
                        Price DECIMAL(10,2) DEFAULT 0,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS STORAGE_STOCKS (
                        ProductID INT PRIMARY KEY,
                        Quantity INT DEFAULT 0,
                        Location VARCHAR(100),
                        LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                        FOREIGN KEY (ProductID) REFERENCES PRODUCTS(ProductID) ON DELETE CASCADE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS MARKET_STOCKS (
                        MarketStockID INT AUTO_INCREMENT PRIMARY KEY,
                        ProductID INT NOT NULL,
                        StoreID VARCHAR(50) NOT NULL,
                        Quantity INT DEFAULT 0,
                        LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                        UNIQUE KEY (ProductID, StoreID),
                        FOREIGN KEY (ProductID) REFERENCES PRODUCTS(ProductID) ON DELETE CASCADE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS STOCK_TRANSFERS (
                        TransferID INT AUTO_INCREMENT PRIMARY KEY,
                        ProductID INT NOT NULL,
                        FromLocation VARCHAR(50),
                        ToLocation VARCHAR(50),
                        Quantity INT,
                        TransferDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        Notes VARCHAR(200),
                        FOREIGN KEY (ProductID) REFERENCES PRODUCTS(ProductID) ON DELETE CASCADE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS BEDS (
                        BedID INT AUTO_INCREMENT PRIMARY KEY,
                        RoomTypeID INT,
                        BedType VARCHAR(50),
                        Capacity INT,
                        FOREIGN KEY (RoomTypeID) REFERENCES ROOM_TYPES(RoomTypeID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS ROOMS (
                        RoomID INT AUTO_INCREMENT PRIMARY KEY,
                        RoomNumber VARCHAR(10) NOT NULL UNIQUE,
                        FloorID INT,
                        RoomTypeID INT,
                        Capacity INT DEFAULT 2,
                        OccupiedBeds INT DEFAULT 0,
                        Status VARCHAR(20) DEFAULT 'Available',
                        Description TEXT,
                        FOREIGN KEY (FloorID) REFERENCES FLOORS(FloorID),
                        FOREIGN KEY (RoomTypeID) REFERENCES ROOM_TYPES(RoomTypeID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS ROOM_PRICES (
                        PriceID INT AUTO_INCREMENT PRIMARY KEY,
                        RoomTypeID INT,
                        StartDate DATE,
                        EndDate DATE,
                        Price DECIMAL(10,2),
                        DayOfWeek VARCHAR(20),
                        FOREIGN KEY (RoomTypeID) REFERENCES ROOM_TYPES(RoomTypeID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS CUSTOMERS (
                        CustomerID INT AUTO_INCREMENT PRIMARY KEY,
                        IdentityNumber VARCHAR(11) UNIQUE,
                        UserID INT NULL,
                        FirstName VARCHAR(50) NOT NULL,
                        LastName VARCHAR(50) NOT NULL,
                        Email VARCHAR(100),
                        Phone VARCHAR(20),
                        Address TEXT,
                        RoomNumber VARCHAR(10),
                        BedNumber INT DEFAULT 1,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (UserID) REFERENCES USERS(UserID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS RESERVATIONS (
                        ReservationID INT AUTO_INCREMENT PRIMARY KEY,
                        CustomerID INT,
                        RoomID INT,
                        BedNumber INT DEFAULT 1,
                        CheckInDate DATE NOT NULL,
                        CheckOutDate DATE NOT NULL,
                        Status VARCHAR(20) DEFAULT 'CheckedIn',
                        TotalAmount DECIMAL(10,2),
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (CustomerID) REFERENCES CUSTOMERS(CustomerID),
                        FOREIGN KEY (RoomID) REFERENCES ROOMS(RoomID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS PAYMENTS (
                        PaymentID INT AUTO_INCREMENT PRIMARY KEY,
                        ReservationID INT,
                        PaymentDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        Amount DECIMAL(10,2) NOT NULL,
                        PaymentMethod VARCHAR(50),
                        Status VARCHAR(20),
                        FOREIGN KEY (ReservationID) REFERENCES RESERVATIONS(ReservationID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS SERVICES (
                        ServiceID INT AUTO_INCREMENT PRIMARY KEY,
                        ReservationID INT,
                        ServiceName VARCHAR(100) NOT NULL,
                        ServiceDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        Cost DECIMAL(10,2) NOT NULL,
                        Description TEXT,
                        FOREIGN KEY (ReservationID) REFERENCES RESERVATIONS(ReservationID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
                };

                foreach (var sql in tables)
                {
                    using (var cmd = new MySqlCommand(sql, conn))
                        cmd.ExecuteNonQuery();
                }

                // Add IdentityNumber Column if not exists
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='CUSTOMERS' AND COLUMN_NAME='IdentityNumber' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE CUSTOMERS ADD COLUMN IdentityNumber VARCHAR(11) UNIQUE AFTER CustomerID", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add RoomID column to ROOM_PRICES for per-room pricing (if not exists)
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='ROOM_PRICES' AND COLUMN_NAME='RoomID' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE ROOM_PRICES ADD COLUMN RoomID INT NULL AFTER RoomTypeID, ADD INDEX idx_room_price_roomid (RoomID)", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Migrate ROOM_PRICES (Zaten var, çalışır)

                // Admin
                using (var cmd = new MySqlCommand(@"INSERT IGNORE INTO USERS (Username, FullName, Email, PasswordHash, Role)
                    VALUES ('admin', 'Sistem Yoneticisi', 'admin@pms.com', 'admin123', 'Admin')", conn))
                    cmd.ExecuteNonQuery();

                // Seed Floors
                using (var cmd = new MySqlCommand("INSERT IGNORE INTO FLOORS (FloorNumber, Description) VALUES (1,'Kat 1'),(2,'Kat 2'),(3,'Kat 3')", conn))
                    cmd.ExecuteNonQuery();

                // Seed Room Types
                using (var cmd = new MySqlCommand(@"INSERT IGNORE INTO ROOM_TYPES (RoomTypeID, TypeName, Description, MaxOccupancy, BasePrice) VALUES 
                    (1, 'Deniz Manzarali', 'Deniz manzarali oda', 4, 1500.00),
                    (2, 'Standart', 'Standart oda', 4, 800.00)", conn))
                    cmd.ExecuteNonQuery();

                // Seed Rooms - 101,102,201,202,301,302 = Deniz Manzarali (TypeID=1), digerleri Standart (TypeID=2)
                string[] roomSeeds = {
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('101',(SELECT FloorID FROM FLOORS WHERE FloorNumber=1),1,2,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('102',(SELECT FloorID FROM FLOORS WHERE FloorNumber=1),1,3,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('103',(SELECT FloorID FROM FLOORS WHERE FloorNumber=1),2,1,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('104',(SELECT FloorID FROM FLOORS WHERE FloorNumber=1),2,4,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('105',(SELECT FloorID FROM FLOORS WHERE FloorNumber=1),2,2,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('106',(SELECT FloorID FROM FLOORS WHERE FloorNumber=1),2,3,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('201',(SELECT FloorID FROM FLOORS WHERE FloorNumber=2),1,2,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('202',(SELECT FloorID FROM FLOORS WHERE FloorNumber=2),1,1,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('203',(SELECT FloorID FROM FLOORS WHERE FloorNumber=2),2,3,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('204',(SELECT FloorID FROM FLOORS WHERE FloorNumber=2),2,4,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('205',(SELECT FloorID FROM FLOORS WHERE FloorNumber=2),2,2,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('206',(SELECT FloorID FROM FLOORS WHERE FloorNumber=2),2,3,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('301',(SELECT FloorID FROM FLOORS WHERE FloorNumber=3),1,1,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('302',(SELECT FloorID FROM FLOORS WHERE FloorNumber=3),1,2,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('303',(SELECT FloorID FROM FLOORS WHERE FloorNumber=3),2,3,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('304',(SELECT FloorID FROM FLOORS WHERE FloorNumber=3),2,4,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('305',(SELECT FloorID FROM FLOORS WHERE FloorNumber=3),2,2,0,'Available')",
                    "INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) VALUES ('306',(SELECT FloorID FROM FLOORS WHERE FloorNumber=3),2,1,0,'Available')"
                };
                foreach (var rs in roomSeeds)
                    using (var cmd = new MySqlCommand(rs, conn)) cmd.ExecuteNonQuery();
            }
        }
    }
}
