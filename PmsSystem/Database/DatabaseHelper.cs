using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PmsSystem.Database
{
    public static class DatabaseHelper
    {
        private static IConfiguration _configuration;
        private static string Server;
        private static string DbName;
        private static string User;
        private static string Password;
        private static string Port;

        static DatabaseHelper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();

            Server   = _configuration["DatabaseConfig:Server"] ?? "127.0.0.1";
            DbName   = _configuration["DatabaseConfig:Database"] ?? "pms_system";
            User     = _configuration["DatabaseConfig:User"] ?? "root";
            Password = _configuration["DatabaseConfig:Password"] ?? "";
            Port     = _configuration["DatabaseConfig:Port"] ?? "3306";
        }

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

                    @"CREATE TABLE IF NOT EXISTS PRODUCTS (
                        ProductID INT PRIMARY KEY,
                        Barcode VARCHAR(50) UNIQUE NOT NULL,
                        ItemName VARCHAR(100) NOT NULL,
                        ManufacturerName VARCHAR(100),
                        Price DECIMAL(10,2) DEFAULT 0,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS STORAGE_STOCKS (
                        StorageID INT AUTO_INCREMENT PRIMARY KEY,
                        ProductID INT NOT NULL,
                        Quantity INT DEFAULT 0,
                        Location VARCHAR(100),
                        ArrivalDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                        FOREIGN KEY (ProductID) REFERENCES PRODUCTS(ProductID) ON DELETE CASCADE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS MARKET_STOCKS (
                        MarketStockID INT AUTO_INCREMENT PRIMARY KEY,
                        ProductID INT NOT NULL,
                        StoreID VARCHAR(50) NOT NULL,
                        Quantity INT DEFAULT 0,
                        Price DECIMAL(10,2) DEFAULT 0,
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
                        PurchasePrice DECIMAL(10,2) DEFAULT 0,
                        EmployeeName VARCHAR(100),
                        SupplierName VARCHAR(100),
                        TransferDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        Notes VARCHAR(200),
                        FOREIGN KEY (ProductID) REFERENCES PRODUCTS(ProductID) ON DELETE CASCADE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS MANUFACTURERS (
                        ManufacturerID INT AUTO_INCREMENT PRIMARY KEY,
                        Name VARCHAR(100) NOT NULL UNIQUE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS SUPPLIERS (
                        SupplierID INT AUTO_INCREMENT PRIMARY KEY,
                        Name VARCHAR(100) NOT NULL UNIQUE,
                        ContactPhone VARCHAR(20),
                        Address TEXT
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
                        FatherName VARCHAR(50),
                        MotherName VARCHAR(50),
                        BirthPlace VARCHAR(50),
                        BirthDate DATE,
                        Gender VARCHAR(10),
                        Email VARCHAR(100),
                        Phone VARCHAR(20),
                        Address TEXT,
                        RoomNumber VARCHAR(10),
                        BedNumber INT DEFAULT 1,
                        Nationality VARCHAR(50) DEFAULT 'Türkiye',
                        Notes TEXT,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (UserID) REFERENCES USERS(UserID),
                        INDEX (IdentityNumber),
                        INDEX (FirstName, LastName)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    // NOT: COMPANIES tablosu RESERVATIONS tarafından referans alındığı için
                    // yeni kurulumda FK hatası olmaması adına RESERVATIONS'dan önce oluşturulmalıdır.
                    @"CREATE TABLE IF NOT EXISTS COMPANIES (
                        CompanyID INT AUTO_INCREMENT PRIMARY KEY,
                        CompanyName VARCHAR(100) UNIQUE,
                        TaxNumber VARCHAR(20),
                        TaxOffice VARCHAR(50),
                        Address TEXT,
                        Phone VARCHAR(20)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS RESERVATIONS (
                        ReservationID INT AUTO_INCREMENT PRIMARY KEY,
                        CustomerID INT,
                        RoomID INT,
                        BedNumber INT DEFAULT 1,
                        CheckInDate DATE NOT NULL,
                        CheckOutDate DATE NOT NULL,
                        Status VARCHAR(20) DEFAULT 'Pending',
                        TotalAmount DECIMAL(10,2),
                        PaidAmount DECIMAL(10,2) DEFAULT 0,
                        ChannelName VARCHAR(50) DEFAULT 'Direkt',
                        CommissionAmount DECIMAL(10,2) DEFAULT 0,
                        CompanyID INT NULL,
                        Notes TEXT,
                        ExtraAmount DECIMAL(10,2) DEFAULT 0,
                        IsOnline TINYINT(1) DEFAULT 0,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (CustomerID) REFERENCES CUSTOMERS(CustomerID),
                        FOREIGN KEY (RoomID) REFERENCES ROOMS(RoomID),
                        FOREIGN KEY (CompanyID) REFERENCES COMPANIES(CompanyID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS PAYMENTS (
                        PaymentID INT AUTO_INCREMENT PRIMARY KEY,
                        ReservationID INT,
                        RoomAmount DECIMAL(10,2) DEFAULT 0,
                        LokantaAmount DECIMAL(10,2) DEFAULT 0,
                        TotalAmount DECIMAL(10,2) DEFAULT 0,
                        PaymentMethod VARCHAR(50),
                        PaymentDate DATETIME DEFAULT CURRENT_TIMESTAMP,
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
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS SALES_LOG (
                        SaleID INT AUTO_INCREMENT PRIMARY KEY,
                        ProductID INT NOT NULL,
                        StoreID VARCHAR(50) NOT NULL,
                        Quantity INT NOT NULL,
                        UnitPrice DECIMAL(10,2) NOT NULL,
                        TotalPrice DECIMAL(10,2) NOT NULL,
                        RoomInfo VARCHAR(255) DEFAULT '',
                        IsPaid TINYINT DEFAULT 0,
                        SaleDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (ProductID) REFERENCES PRODUCTS(ProductID) ON DELETE CASCADE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS ACTIVITY_LOG (
                        ActivityID INT AUTO_INCREMENT PRIMARY KEY,
                        ActivityType VARCHAR(50),
                        Description TEXT,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS EXPENSES (
                        ExpenseID INT AUTO_INCREMENT PRIMARY KEY,
                        Title VARCHAR(100),
                        Category VARCHAR(50),
                        Amount DECIMAL(18,2),
                        ExpenseDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        Description TEXT,
                        PaidBy VARCHAR(50)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS MAINTENANCE_LOGS (
                        LogID INT AUTO_INCREMENT PRIMARY KEY,
                        RoomID INT,
                        FaultDescription TEXT,
                        ReportedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        ResolvedDate DATETIME,
                        TechnicianName VARCHAR(100),
                        Cost DECIMAL(10,2) DEFAULT 0,
                        Status VARCHAR(20) DEFAULT 'Pending',
                        FOREIGN KEY (RoomID) REFERENCES ROOMS(RoomID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS HOUSEKEEPING_TASKS (
                        TaskID INT AUTO_INCREMENT PRIMARY KEY,
                        RoomID INT,
                        AssignedTo VARCHAR(100),
                        TaskStatus VARCHAR(20) DEFAULT 'Pending',
                        TaskType VARCHAR(50) DEFAULT 'Cleaning',
                        Notes TEXT,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        CompletedAt DATETIME,
                        FOREIGN KEY (RoomID) REFERENCES ROOMS(RoomID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS EMPLOYEES (
                        EmployeeID INT AUTO_INCREMENT PRIMARY KEY,
                        FirstName VARCHAR(50),
                        LastName VARCHAR(50),
                        Role VARCHAR(50),
                        Phone VARCHAR(20),
                        Salary DECIMAL(10,2),
                        IsActive TINYINT(1) DEFAULT 1,
                        HireDate DATE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS SHIFTS (
                        ShiftID INT AUTO_INCREMENT PRIMARY KEY,
                        EmployeeID INT,
                        ShiftDate DATE,
                        StartTime TIME,
                        EndTime TIME,
                        Status VARCHAR(20) DEFAULT 'Scheduled',
                        FOREIGN KEY (EmployeeID) REFERENCES EMPLOYEES(EmployeeID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS END_OF_DAY_REPORTS (
                        ReportID INT AUTO_INCREMENT PRIMARY KEY,
                        ReportDate DATE UNIQUE,
                        TotalCash DECIMAL(12,2) DEFAULT 0,
                        TotalCreditCard DECIMAL(12,2) DEFAULT 0,
                        TotalExpenses DECIMAL(12,2) DEFAULT 0,
                        TotalRevenue DECIMAL(12,2) DEFAULT 0,
                        CompletedBy VARCHAR(100),
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS RESTAURANT_TABLES (
                        TableID INT AUTO_INCREMENT PRIMARY KEY,
                        TableName VARCHAR(50) NOT NULL,
                        Status VARCHAR(20) DEFAULT 'Available',
                        CurrentReservationID INT NULL,
                        FOREIGN KEY (CurrentReservationID) REFERENCES RESERVATIONS(ReservationID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

                    @"CREATE TABLE IF NOT EXISTS CUSTOMER_MESSAGES (
                        MessageID INT AUTO_INCREMENT PRIMARY KEY,
                        CustomerID INT NOT NULL,
                        MessageText TEXT NOT NULL,
                        Direction VARCHAR(20) DEFAULT 'Incoming',
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (CustomerID) REFERENCES CUSTOMERS(CustomerID) ON DELETE CASCADE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
                };

                // KRİTİK: Veri kaybı riski
                // Eski şemadan kalan MARKET/STORAGE tablolarını otomatik DROP etmek, production ortamında
                // yanlışlıkla veri kaybına sebep olabilir. Bu nedenle sadece açıkça izin verildiğinde çalıştırılır.
                // appsettings.json örnek:  "DatabaseConfig": { ..., "AllowLegacyDrops": "true" }
                var allowLegacyDrops = (_configuration["DatabaseConfig:AllowLegacyDrops"] ?? _configuration["DatabaseConfig:AllowLegacyDrop"] ?? "false")
                    .Equals("true", StringComparison.OrdinalIgnoreCase);
                if (allowLegacyDrops)
                {
                    string[] legacyDrops = {
                        "SET FOREIGN_KEY_CHECKS = 0;",
                        "DROP TABLE IF EXISTS MARKET;",
                        "DROP TABLE IF EXISTS STORAGE;",
                        "SET FOREIGN_KEY_CHECKS = 1;"
                    };
                    foreach (var sql in legacyDrops)
                    {
                        using var cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                    }
                }

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

                // Add ToLocation and FromLocation to STOCK_TRANSFERS if not exists
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='STOCK_TRANSFERS' AND COLUMN_NAME='ToLocation' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter1 = new MySqlCommand("ALTER TABLE STOCK_TRANSFERS ADD COLUMN FromLocation VARCHAR(50) AFTER ProductID", conn); alter1.ExecuteNonQuery();
                        using var alter2 = new MySqlCommand("ALTER TABLE STOCK_TRANSFERS ADD COLUMN ToLocation VARCHAR(50) AFTER FromLocation", conn); alter2.ExecuteNonQuery();
                        // Optional fallback config: set Direction to 'IN' if ToLocation wasn't mapped
                        try { using var updateCmd = new MySqlCommand("UPDATE STOCK_TRANSFERS SET FromLocation='TEDARIKCI', ToLocation='DEPO' WHERE FromLocation IS NULL", conn); updateCmd.ExecuteNonQuery(); } catch { /* Ignore if update fails */ }
                    }
                }

                // Add SupplierName column to STOCK_TRANSFERS if not exists
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='STOCK_TRANSFERS' AND COLUMN_NAME='SupplierName' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE STOCK_TRANSFERS ADD COLUMN SupplierName VARCHAR(100) AFTER EmployeeName", conn);
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

                // Add ManufacturerName, Category, Unit, SuggestedSalePrice to PRODUCTS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='PRODUCTS' AND COLUMN_NAME='ManufacturerName' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE PRODUCTS ADD COLUMN ManufacturerName VARCHAR(100) AFTER ItemName", conn);
                        alter.ExecuteNonQuery();
                    }
                }
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='PRODUCTS' AND COLUMN_NAME='Category' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE PRODUCTS ADD COLUMN Category VARCHAR(50) AFTER ItemName, ADD COLUMN Unit VARCHAR(20) AFTER ManufacturerName, ADD COLUMN SuggestedSalePrice DECIMAL(10,2) AFTER Price", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add InvoiceNumber and PaymentMethod to STOCK_TRANSFERS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='STOCK_TRANSFERS' AND COLUMN_NAME='InvoiceNumber' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE STOCK_TRANSFERS ADD COLUMN InvoiceNumber VARCHAR(50) AFTER TransferDate, ADD COLUMN PaymentMethod VARCHAR(50) AFTER InvoiceNumber", conn);
                        alter.ExecuteNonQuery();
                    }
                }


                // Add ArrivalDate to STORAGE_STOCKS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='STORAGE_STOCKS' AND COLUMN_NAME='ArrivalDate' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE STORAGE_STOCKS ADD COLUMN ArrivalDate DATETIME AFTER Location", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add Price to MARKET_STOCKS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='MARKET_STOCKS' AND COLUMN_NAME='Price' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE MARKET_STOCKS ADD COLUMN Price DECIMAL(10,2) DEFAULT 0 AFTER Quantity", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add RoomInfo column to SALES_LOG if not exists
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='SALES_LOG' AND COLUMN_NAME='RoomInfo' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE SALES_LOG ADD COLUMN RoomInfo VARCHAR(255) DEFAULT '' AFTER TotalPrice", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add Status column to SALES_LOG if not exists
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='SALES_LOG' AND COLUMN_NAME='Status' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE SALES_LOG ADD COLUMN Status VARCHAR(20) DEFAULT 'Pending' AFTER IsPaid", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add IsPaid column to SALES_LOG
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='SALES_LOG' AND COLUMN_NAME='IsPaid' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE SALES_LOG ADD COLUMN IsPaid TINYINT DEFAULT 0 AFTER RoomInfo", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add Channel and Commission to RESERVATIONS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='RESERVATIONS' AND COLUMN_NAME='ChannelName' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE RESERVATIONS ADD COLUMN ChannelName VARCHAR(50) DEFAULT 'Direkt', ADD COLUMN CommissionAmount DECIMAL(10,2) DEFAULT 0", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add CompanyID to RESERVATIONS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='RESERVATIONS' AND COLUMN_NAME='CompanyID' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE RESERVATIONS ADD COLUMN CompanyID INT NULL, ADD FOREIGN KEY (CompanyID) REFERENCES COMPANIES(CompanyID)", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add PaidAmount column to RESERVATIONS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='RESERVATIONS' AND COLUMN_NAME='PaidAmount' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE RESERVATIONS ADD COLUMN PaidAmount DECIMAL(10,2) DEFAULT 0 AFTER TotalAmount", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add Notes and ExtraAmount to RESERVATIONS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='RESERVATIONS' AND COLUMN_NAME='Notes' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE RESERVATIONS ADD COLUMN Notes TEXT", conn);
                        alter.ExecuteNonQuery();
                    }
                }
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='RESERVATIONS' AND COLUMN_NAME='ExtraAmount' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE RESERVATIONS ADD COLUMN ExtraAmount DECIMAL(10,2) DEFAULT 0", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add IsOnline column to RESERVATIONS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='RESERVATIONS' AND COLUMN_NAME='IsOnline' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE RESERVATIONS ADD COLUMN IsOnline TINYINT(1) DEFAULT 0 AFTER ExtraAmount", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Sync PAYMENTS columns
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='PAYMENTS' AND COLUMN_NAME='TotalAmount' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE PAYMENTS ADD COLUMN RoomAmount DECIMAL(10,2) DEFAULT 0, ADD COLUMN LokantaAmount DECIMAL(10,2) DEFAULT 0, ADD COLUMN TotalAmount DECIMAL(10,2) DEFAULT 0", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add RoomNumber column to ROOM_PRICES for per-room pricing (if not exists)
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='ROOM_PRICES' AND COLUMN_NAME='RoomNumber' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE ROOM_PRICES ADD COLUMN RoomNumber VARCHAR(10) NULL AFTER RoomID", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Add PurchasePrice to STOCK_TRANSFERS
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME='STOCK_TRANSFERS' AND COLUMN_NAME='PurchasePrice' AND TABLE_SCHEMA=@db", conn))
                {
                    cmd.Parameters.AddWithValue("@db", DbName);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        using var alter = new MySqlCommand("ALTER TABLE STOCK_TRANSFERS ADD COLUMN PurchasePrice DECIMAL(10,2) DEFAULT 0 AFTER Quantity, ADD COLUMN EmployeeName VARCHAR(100) AFTER PurchasePrice", conn);
                        alter.ExecuteNonQuery();
                    }
                }

                // Seed Manufacturers
                using (var cmd = new MySqlCommand(@"INSERT IGNORE INTO MANUFACTURERS (Name) VALUES 
                    ('Coca-Cola'), ('Pepsi'), ('Fanta'), ('Sprite'), ('Uludağ Gazoz'), 
                    ('Cappy'), ('Dimes'), ('Tropicana'), ('Pınar Meyve Suyu'), 
                    ('Erikli Su'), ('Hayat Su'), ('Saka Su'), ('Damla Su'), 
                    ('Türk Kahvesi'), ('Çaykur'), ('Nescafe')", conn))
                    cmd.ExecuteNonQuery();

                // Migrate ROOM_PRICES (Zaten var, çalışır)

                // Add missing columns if they don't exist (Migration)
                string[] migrations = {
                    "ALTER TABLE CUSTOMERS ADD COLUMN IF NOT EXISTS FatherName VARCHAR(50) AFTER LastName",
                    "ALTER TABLE CUSTOMERS ADD COLUMN IF NOT EXISTS MotherName VARCHAR(50) AFTER FatherName",
                    "ALTER TABLE CUSTOMERS ADD COLUMN IF NOT EXISTS BirthPlace VARCHAR(50) AFTER MotherName",
                    "ALTER TABLE CUSTOMERS ADD COLUMN IF NOT EXISTS BirthDate DATE AFTER BirthPlace",
                    "ALTER TABLE CUSTOMERS ADD COLUMN IF NOT EXISTS Gender VARCHAR(10) AFTER BirthDate",
                    "ALTER TABLE CUSTOMERS ADD COLUMN IF NOT EXISTS Nationality VARCHAR(50) DEFAULT 'Türkiye' AFTER BedNumber",
                    "ALTER TABLE CUSTOMERS ADD COLUMN IF NOT EXISTS Notes TEXT AFTER Nationality"
                };
                foreach (var migration in migrations) {
                    try {
                        using (var cmd = new MySqlCommand(migration, conn))
                            cmd.ExecuteNonQuery();
                    } catch { /* Column might already exist or MariaDB version doesn't support IF NOT EXISTS in ALTER */ }
                }

                // Admin (Sifre: admin123 - SHA256 Hash)
                using (var cmd = new MySqlCommand(@"INSERT IGNORE INTO USERS (Username, FullName, Email, PasswordHash, Role)
                    VALUES ('admin', 'Sistem Yoneticisi', 'admin@pms.com', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Admin')", conn))
                    cmd.ExecuteNonQuery();

                // Fix existing admin password hash if it was seeded with incorrect value
                using (var cmd = new MySqlCommand(@"UPDATE USERS SET PasswordHash='240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9' 
                    WHERE Username='admin' AND PasswordHash != '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9'", conn))
                    cmd.ExecuteNonQuery();

                // Seed Floors
                using (var cmd = new MySqlCommand("INSERT IGNORE INTO FLOORS (FloorNumber, Description) VALUES (1,'Kat 1'),(2,'Kat 2'),(3,'Kat 3')", conn))
                    cmd.ExecuteNonQuery();

                // Seed Room Types
                using (var cmd = new MySqlCommand(@"INSERT IGNORE INTO ROOM_TYPES (RoomTypeID, TypeName, Description, MaxOccupancy, BasePrice) VALUES 
                    (1, 'Deniz Manzarali', 'Deniz manzarali oda', 4, 1500.00),
                    (2, 'Standart', 'Standart oda', 4, 800.00)", conn))
                    cmd.ExecuteNonQuery();

                // Seed Rooms - 101-110, 201-210, 301-310
                for (int f = 1; f <= 3; f++)
                {
                    for (int r = 1; r <= 10; r++)
                    {
                        // 1. kat: 101-110, 2. kat: 201-210 ...
                        string roomNum = (f * 100 + r).ToString();
                        
                        // İlk 2 oda deniz manzaralı olsun (TypeID=1), diğerleri standart (TypeID=2)
                        int typeId = (r <= 2) ? 1 : 2;
                        int capacity = (r % 3 == 0) ? 3 : (r % 2 == 0 ? 4 : 2); // Değişken kapasiteler
                        
                        using (var cmd = new MySqlCommand(@"
                            INSERT IGNORE INTO ROOMS (RoomNumber, FloorID, RoomTypeID, Capacity, OccupiedBeds, Status) 
                            VALUES (@rn, (SELECT FloorID FROM FLOORS WHERE FloorNumber=@fn), @tid, @cap, 0, 'Available')", conn))
                        {
                            cmd.Parameters.AddWithValue("@rn", roomNum);
                            cmd.Parameters.AddWithValue("@fn", f);
                            cmd.Parameters.AddWithValue("@tid", typeId);
                            cmd.Parameters.AddWithValue("@cap", capacity);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Seed Tables if empty
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM RESTAURANT_TABLES", conn))
                {
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        string[] tableNames = { "Masa 1", "Masa 2", "Masa 3", "Masa 4", "Masa 5", "Bahçe 1", "Bahçe 2", "Teras 1", "Teras 2" };
                        foreach (var tname in tableNames)
                        {
                            using (var cmdIns = new MySqlCommand("INSERT INTO RESTAURANT_TABLES (TableName) VALUES (@n)", conn))
                            {
                                cmdIns.Parameters.AddWithValue("@n", tname);
                                cmdIns.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}
