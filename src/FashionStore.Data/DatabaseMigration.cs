using Dapper;
using FashionStore.Core.Models;
using FashionStore.Core.Settings;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace FashionStore.Data
{
    /// <summary>
    /// Database migration helper for updating schema to match new comprehensive design
    /// </summary>
    public static class DatabaseMigration
    {
        /// <summary>
        /// Run all migrations to update database schema
        /// </summary>
        public static void RunAllMigrations(MySqlConnection connection)
        {
            try
            {
                EnsureDatabaseSchema(connection);
                EnsureAdminAccount(connection);
                
                MigrateAccountsTable(connection);
                MigrateCategoriesTable(connection);
                MigrateSuppliersTable(connection);
                MigrateProductsTable(connection);
                MigrateCustomersTable(connection);
                MigrateVouchersTable(connection);
                MigrateInvoicesTable(connection);
                MigrateInvoiceItemsTable(connection);
                CreateProductVariantsTable(connection);
                CreateStockMovementsTable(connection);
                CreateCustomerVoucherUsageTable(connection);
                CreatePromotionsTable(connection);
                MigratePromotionsTable(connection);
                CreateErpTables(connection);
                
                SeedSampleData(connection);
                FixIncorrectProductCategories(connection);
                BackfillBarcodes(connection);
                SyncCustomerPointsAndTotalSpent(connection);
                MigrateTransactionHistoryView(connection);

                Debug.WriteLine("All database migrations completed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during database migration: {ex.Message}");
                throw;
            }
        }

        private static void BackfillBarcodes(MySqlConnection connection)
        {
            try
            {
                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 0;");
                string sql = @"
                    UPDATE Products 
                    SET Code = CONCAT('893', LPAD(Id, 7, '0')) 
                    WHERE (Code IS NULL OR TRIM(Code) = '') AND Id > 0;";
                ExecuteNonQuery(connection, sql);
                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 1;");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error backfilling barcodes: {ex.Message}");
            }
        }

        private static void RandomizeAllBarcodes(MySqlConnection connection)
        {
            try
            {
                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 0;");
                
                // 1. Randomize Products.Code
                string sqlProducts = @"
                    UPDATE Products 
                    SET Code = CONCAT('893', LPAD(FLOOR(RAND() * 10000000), 7, '0')) 
                    WHERE Id > 0;";
                ExecuteNonQuery(connection, sqlProducts);

                // 2. Randomize ProductVariants.Barcode
                string sqlVariants = @"
                    UPDATE ProductVariants 
                    SET Barcode = CONCAT('888', LPAD(FLOOR(RAND() * 10000000), 7, '0'))
                    WHERE Id > 0;";
                ExecuteNonQuery(connection, sqlVariants);

                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 1;");
                Debug.WriteLine("Successfully randomized all barcodes in database.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error randomizing barcodes: {ex.Message}");
            }
        }

        private static void SyncCustomerPointsAndTotalSpent(MySqlConnection connection)
        {
            try
            {
                ExecuteNonQuery(connection, "ALTER TABLE Customers MODIFY COLUMN CustomerType VARCHAR(20) DEFAULT 'Regular';");
                ExecuteNonQuery(connection, "UPDATE Customers SET CustomerType = 'VIP' WHERE CustomerType = 'Platinum' OR CustomerType = '';");

                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 0;");
                string sqlSpent = @"
                    UPDATE Customers c
                    SET TotalSpent = (
                        SELECT IFNULL(SUM(Total), 0)
                        FROM Invoices
                        WHERE CustomerId = c.Id
                    );";
                ExecuteNonQuery(connection, sqlSpent);

                decimal spendPerPoint = TierSettingsManager.Load().SpendPerPoint;
                if (spendPerPoint <= 0) spendPerPoint = 100000;
                string sqlPoints = @"
                    UPDATE Customers c
                    SET Points = FLOOR(TotalSpent / @SpendPerPoint)
                    WHERE TotalSpent > 0;";
                ExecuteNonQuery(connection, sqlPoints, new { SpendPerPoint = spendPerPoint });
                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 1;");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error syncing customer points: {ex.Message}");
            }
        }

        private static void SeedSampleData(MySqlConnection connection)
        {
            try
            {
                // 1. Seed Categories (using INSERT IGNORE)
                string[] categories = { "Áo Sơ Mi", "Quần Jean", "Giày Sneaker", "Phụ Kiện" };
                foreach (var cat in categories)
                {
                    ExecuteNonQuery(connection, "INSERT IGNORE INTO Categories (Name, TaxPercent) VALUES (@Name, @Tax);", new { Name = cat, Tax = 10 });
                }

                // 2. Seed Suppliers
                ExecuteNonQuery(connection, "INSERT IGNORE INTO Suppliers (Name, ContactName, Phone, Address) VALUES ('Công ty May Mặc Á Châu', 'Nguyễn Văn A', '0901234567', '123 Đường ABC, HCM');");

                // Get some IDs
                int catId = connection.ExecuteScalar<int>("SELECT Id FROM Categories WHERE Name=@Name LIMIT 1", new { Name = "Áo Sơ Mi" });
                int supId = connection.ExecuteScalar<int>("SELECT Id FROM Suppliers WHERE Name=@Name LIMIT 1", new { Name = "Công ty May Mặc Á Châu" });

                string[][] products = new[]
                {
                    new[] { "Áo Sơ Mi Công Sở Nam", "8930001", "350000", "200000", "Chất liệu cotton cao cấp", "Áo Sơ Mi" },
                    new[] { "Quần Jean Skinny Nữ", "8930002", "450000", "250000", "Co giãn 4 chiều", "Quần Jean" },
                    new[] { "Giày Sneaker Classic", "8930003", "600000", "350000", "Phong cách trẻ trung", "Giày Sneaker" },
                    new[] { "Thắt Lưng Da", "8930004", "150000", "80000", "Da bò thật 100%", "Phụ Kiện" }
                };

                foreach (var p in products)
                {
                    int specificCatId = connection.ExecuteScalar<int>("SELECT Id FROM Categories WHERE Name=@Name LIMIT 1", new { Name = p[5] });
                    if (specificCatId == 0) specificCatId = catId; 

                    string sql = "INSERT IGNORE INTO Products (Name, Code, CategoryId, SalePrice, PurchasePrice, PurchaseUnit, ImportQuantity, StockQuantity, Description, SupplierId) " +
                                 "VALUES (@Name, @Code, @CategoryId, @SalePrice, @PurchasePrice, @PurchaseUnit, 100, 50, @Description, @SupplierId);";
                    ExecuteNonQuery(connection, sql, new { 
                        Name = p[0], 
                        Code = p[1], 
                        CategoryId = specificCatId, 
                        SalePrice = p[2], 
                        PurchasePrice = p[3], 
                        PurchaseUnit = "Cái",
                        Description = p[4],
                        SupplierId = supId
                    });
                }

                // 3. Seed Default Customer (Walk-in)
                ExecuteNonQuery(connection, "INSERT IGNORE INTO Customers (Id, Name, Phone) VALUES (1, 'Khách lẻ', '0000000000');");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error seeding sample data: {ex.Message}");
            }
        }

        private static void FixIncorrectProductCategories(MySqlConnection connection)
        {
            try
            {
                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 0;");
                
                // Ensure categories exist
                string[] categories = { "Áo Sơ Mi", "Quần Jean", "Giày Sneaker", "Phụ Kiện" };
                foreach (var cat in categories)
                {
                    ExecuteNonQuery(connection, "INSERT IGNORE INTO Categories (Name, TaxPercent) VALUES (@Name, @Tax);", new { Name = cat, Tax = 10 });
                }

                string[] categoryMappings = {
                    // PHÂN LOẠIƯU TIÊN (Keywords cụ thể nhất)
                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Váy' LIMIT 1) WHERE Name LIKE '%Váy%' OR Name LIKE '%Đầm%' OR Name LIKE '%Skirt%' OR Name LIKE '%Dress%';",
                    
                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Đồ ngủ' LIMIT 1) WHERE Name LIKE '%Đồ ngủ%' OR Name LIKE '%Pijama%' OR Name LIKE '%Đồ bộ%';",
                    
                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Áo khoác' LIMIT 1) WHERE Name LIKE '%Khoác%' OR Name LIKE '%Jacket%' OR Name LIKE '%Hoodie%' OR Name LIKE '%Cardigan%';",

                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Áo thun' LIMIT 1) WHERE Name LIKE '%Thun%' OR Name LIKE '%Phông%' OR Name LIKE '%T-shirt%' OR Name LIKE '%Polo%';",

                    // QUẦN
                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Quần jeans' LIMIT 1) WHERE Name LIKE '%Jean%' OR Name LIKE '%Denim%' OR Name LIKE '%Quần bò%';",
                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Quần short' LIMIT 1) WHERE Name LIKE '%Short%' OR Name LIKE '%Quần đùi%' OR Name LIKE '%Quần lửng%';",
                    
                    // GIÀY (Chạy sau Váy để tránh Sandal quai váy bị nhầm)
                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Giày Sneaker' LIMIT 1) WHERE Name LIKE '%Sneaker%' OR Name LIKE '%Bitis%';",
                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Giày dép' LIMIT 1) WHERE (Name LIKE '%Giày%' OR Name LIKE '%Dép%' OR Name LIKE '%Sandal%') AND Name NOT LIKE '%Váy%' AND Name NOT LIKE '%Đầm%';",
                    
                    // PHỤ KIỆN
                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Phụ kiện' LIMIT 1) WHERE Name LIKE '%Thắt Lưng%' OR Name LIKE '%Dây Nịt%' OR Name LIKE '%Ví%' OR Name LIKE '%Nón%' OR Name LIKE '%Mũ%' OR Name LIKE '%Tất%' OR Name LIKE '%Vớ%' OR Name LIKE '%Cà Vạt%' OR Name LIKE '%Kính%';",
                    
                    // ÁO (Sau cùng để làm default cho các loại ÁO khác)
                    "UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name='Áo sơ mi' LIMIT 1) WHERE (Name LIKE '%Sơ Mi%' OR Name LIKE '%Áo%') AND CategoryId IS NULL;"
                };

                foreach (var sql in categoryMappings)
                {
                    // Enhanced safety check: Don't run the update if the target category doesn't exist
                    // Extract category name from SQL: Name='...'
                    var startIndex = sql.IndexOf("Name='") + 6;
                    var endIndex = sql.IndexOf("'", startIndex);
                    if (startIndex > 5 && endIndex > startIndex)
                    {
                        var catName = sql.Substring(startIndex, endIndex - startIndex);
                        var exists = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Categories WHERE Name=@Name", new { Name = catName }) > 0;
                        if (!exists) 
                        { 
                            continue; 
                        }
                    }
                    ExecuteNonQuery(connection, sql); 
                }

                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 1;");
                Debug.WriteLine("Successfully reorganized all product categories based on keywords.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fixing product categories: {ex.Message}");
            }
        }

        private static void EnsureDatabaseSchema(MySqlConnection connection)
        {
            try
            {
                connection.Execute(@"CREATE TABLE IF NOT EXISTS Accounts (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(255) NOT NULL UNIQUE,
                    Password VARCHAR(255) NOT NULL,
                    Role VARCHAR(20) NOT NULL DEFAULT 'Cashier',
                    EmployeeName VARCHAR(255)
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS Categories (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL UNIQUE,
                    TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS Products (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Code VARCHAR(50) UNIQUE,
                    CategoryId INT,
                    ImageUrl VARCHAR(500),
                    SalePrice DECIMAL(10,2) NOT NULL,
                    PromoDiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
                    PromoStartDate DATETIME NULL,
                    PromoEndDate DATETIME NULL,
                    PurchasePrice DECIMAL(10,2) DEFAULT 0,
                    PurchaseUnit VARCHAR(50) DEFAULT 'VND',
                    ImportQuantity INT DEFAULT 0,
                    StockQuantity INT NOT NULL DEFAULT 0,
                    Description TEXT,
                    SupplierId INT DEFAULT 0,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS Customers (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Phone VARCHAR(20),
                    Email VARCHAR(255),
                    Address TEXT,
                    CustomerType VARCHAR(50) DEFAULT 'Regular',
                    Points INT NOT NULL DEFAULT 0,
                    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS Invoices (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    CustomerId INT NOT NULL,
                    EmployeeId INT NOT NULL,
                    Subtotal DECIMAL(12,2) NOT NULL,
                    TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
                    TaxAmount DECIMAL(12,2) NOT NULL DEFAULT 0,
                    Discount DECIMAL(12,2) NOT NULL DEFAULT 0,
                    Total DECIMAL(12,2) NOT NULL,
                    Paid DECIMAL(12,2) NOT NULL DEFAULT 0,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    VoucherId INT NULL,
                    Status VARCHAR(50) DEFAULT 'Completed',
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
                    FOREIGN KEY (EmployeeId) REFERENCES Accounts(Id)
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS InvoiceItems (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    InvoiceId INT NOT NULL,
                    ProductId INT NOT NULL,
                    EmployeeId INT NOT NULL,
                    UnitPrice DECIMAL(12,2) NOT NULL,
                    Quantity INT NOT NULL,
                    LineTotal DECIMAL(12,2) NOT NULL,
                    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id),
                    FOREIGN KEY (EmployeeId) REFERENCES Accounts(Id)
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS Suppliers (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    ContactName VARCHAR(255),
                    Phone VARCHAR(50),
                    Email VARCHAR(255),
                    Address TEXT
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS Vouchers (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Code VARCHAR(50) NOT NULL UNIQUE,
                    DiscountType VARCHAR(20) NOT NULL,
                    DiscountValue DECIMAL(12,2) NOT NULL,
                    MinInvoiceAmount DECIMAL(12,2) NOT NULL DEFAULT 0,
                    StartDate DATETIME NOT NULL,
                    EndDate DATETIME NOT NULL,
                    UsageLimit INT NOT NULL DEFAULT 0,
                    UsedCount INT NOT NULL DEFAULT 0,
                    IsActive BOOLEAN NOT NULL DEFAULT 1
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS SystemSettings (
                    SettingKey VARCHAR(255) PRIMARY KEY,
                    SettingValue TEXT,
                    Description TEXT,
                    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                );");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ensuring database schema: {ex.Message}");
                throw;
            }
        }

        private static void EnsureAdminAccount(MySqlConnection connection)
        {
            try
            {
                long adminCount = connection.ExecuteScalar<long>("SELECT COUNT(*) FROM Accounts WHERE Username='admin';");
                if (adminCount == 0)
                {
                    string hashedPassword = PasswordHelper.HashPassword("admin"); 
                    connection.Execute("INSERT INTO Accounts (Username, Password, Role) VALUES ('admin', @password, 'Admin');", new { password = hashedPassword });
                    Debug.WriteLine("Default admin account created.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ensuring admin account: {ex.Message}");
            }
        }

        private static void MigrateTransactionHistoryView(MySqlConnection connection)
        {
            try
            {
                string sql = @"
                    CREATE OR REPLACE VIEW TransactionHistory AS
                    SELECT 
                        i.Id AS InvoiceId,
                        i.InvoiceDate,
                        i.TotalAmount,
                        i.PaymentMethod,
                        i.CustomerId,
                        c.Name AS CustomerName,
                        a.Username AS EmployeeName
                    FROM Invoices i
                    LEFT JOIN Customers c ON i.CustomerId = c.Id
                    LEFT JOIN Accounts a ON i.EmployeeId = a.Id;
                ";
                ExecuteNonQuery(connection, sql);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating TransactionHistory view: {ex.Message}");
            }
        }


        private static void MigrateAccountsTable(MySqlConnection connection)
        {
            try
            {
                // Add EmployeeName column
                if (!ColumnExists(connection, "Accounts", "EmployeeName"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Accounts ADD COLUMN EmployeeName VARCHAR(255) NULL AFTER Username;");
                }

                // Add LastLoginDate column
                if (!ColumnExists(connection, "Accounts", "LastLoginDate"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Accounts ADD COLUMN LastLoginDate DATETIME NULL;");
                }

                // Add IsActive column
                if (!ColumnExists(connection, "Accounts", "IsActive"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Accounts ADD COLUMN IsActive BOOLEAN DEFAULT TRUE;");
                }

                // Add CreatedDate column
                if (!ColumnExists(connection, "Accounts", "CreatedDate"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Accounts ADD COLUMN CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP;");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating Accounts table: {ex.Message}");
            }
        }

        private static void MigrateCategoriesTable(MySqlConnection connection)
        {
            try
            {
                // Add Description column
                if (!ColumnExists(connection, "Categories", "Description"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Categories ADD COLUMN Description TEXT NULL;");
                }

                // Add IsActive column
                if (!ColumnExists(connection, "Categories", "IsActive"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Categories ADD COLUMN IsActive BOOLEAN DEFAULT TRUE;");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating Categories table: {ex.Message}");
            }
        }

        private static void MigrateSuppliersTable(MySqlConnection connection)
        {
            try
            {
                // Add TaxCode column
                if (!ColumnExists(connection, "Suppliers", "TaxCode"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Suppliers ADD COLUMN TaxCode VARCHAR(50) NULL;");
                }

                // Add Website column
                if (!ColumnExists(connection, "Suppliers", "Website"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Suppliers ADD COLUMN Website VARCHAR(255) NULL;");
                }

                // Add Notes column
                if (!ColumnExists(connection, "Suppliers", "Notes"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Suppliers ADD COLUMN Notes TEXT NULL;");
                }

                // Add IsActive column
                if (!ColumnExists(connection, "Suppliers", "IsActive"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Suppliers ADD COLUMN IsActive BOOLEAN DEFAULT TRUE;");
                }

                // Add CreatedDate column
                if (!ColumnExists(connection, "Suppliers", "CreatedDate"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Suppliers ADD COLUMN CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP;");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating Suppliers table: {ex.Message}");
            }
        }

        private static void MigrateProductsTable(MySqlConnection connection)
        {
            try
            {
                // Add SupplierId column if not exists
                if (!ColumnExists(connection, "Products", "SupplierId"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Products ADD COLUMN SupplierId INT NULL AFTER CategoryId;");
                    // Add foreign key
                    try
                    {
                        ExecuteNonQuery(connection, "ALTER TABLE Products ADD FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE SET NULL;");
                    }
                    catch { /* FK might already exist */ }
                }

                // Add MinStockLevel column
                if (!ColumnExists(connection, "Products", "MinStockLevel"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Products ADD COLUMN MinStockLevel INT DEFAULT 10;");
                }

                // Add MaxStockLevel column
                if (!ColumnExists(connection, "Products", "MaxStockLevel"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Products ADD COLUMN MaxStockLevel INT DEFAULT 1000;");
                }

                // Add ImageUrl column if missing
                if (!ColumnExists(connection, "Products", "ImageUrl"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Products ADD COLUMN ImageUrl VARCHAR(500) NULL AFTER CategoryId;");
                }

                // Cleanup: Remove redundant columns if they exist (Barcode, Weight, Dimensions)
                string[] unusedColumns = { "Barcode", "Weight", "Dimensions" };
                foreach (var col in unusedColumns)
                {
                    if (ColumnExists(connection, "Products", col))
                    {
                        try { ExecuteNonQuery(connection, $"ALTER TABLE Products DROP COLUMN {col};"); }
                        catch (Exception ex) { Debug.WriteLine($"Error dropping column {col}: {ex.Message}"); }
                    }
                }

                // Add IsActive column
                if (!ColumnExists(connection, "Products", "IsActive"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Products ADD COLUMN IsActive BOOLEAN DEFAULT TRUE;");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating Products table: {ex.Message}");
            }
        }

        private static void MigrateCustomersTable(MySqlConnection connection)
        {
            try
            {
                // Add TotalSpent column
                if (!ColumnExists(connection, "Customers", "TotalSpent"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Customers ADD COLUMN TotalSpent DECIMAL(15,2) DEFAULT 0;");
                }

                // Add DateOfBirth column
                if (!ColumnExists(connection, "Customers", "DateOfBirth"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Customers ADD COLUMN DateOfBirth DATE NULL;");
                }

                // Add Gender column
                if (!ColumnExists(connection, "Customers", "Gender"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Customers ADD COLUMN Gender VARCHAR(10) NULL;");
                }

                // Add LastPurchaseDate column
                if (!ColumnExists(connection, "Customers", "LastPurchaseDate"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Customers ADD COLUMN LastPurchaseDate DATETIME NULL;");
                }

                // Add IsActive column
                if (!ColumnExists(connection, "Customers", "IsActive"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Customers ADD COLUMN IsActive BOOLEAN DEFAULT TRUE;");
                }

                // Add CreatedDate column
                if (!ColumnExists(connection, "Customers", "CreatedDate"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Customers ADD COLUMN CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP;");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating Customers table: {ex.Message}");
            }
        }

        private static void MigrateVouchersTable(MySqlConnection connection)
        {
            try
            {
                // Add Name column
                if (!ColumnExists(connection, "Vouchers", "Name"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Vouchers ADD COLUMN Name VARCHAR(255) NULL AFTER Code;");
                }

                // Add MaxDiscountAmount column
                if (!ColumnExists(connection, "Vouchers", "MaxDiscountAmount"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Vouchers ADD COLUMN MaxDiscountAmount DECIMAL(12,2) NULL;");
                }

                // Add UsageLimitPerCustomer column
                if (!ColumnExists(connection, "Vouchers", "UsageLimitPerCustomer"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Vouchers ADD COLUMN UsageLimitPerCustomer INT DEFAULT 0;");
                }

                // Add ApplicableCategories column
                if (!ColumnExists(connection, "Vouchers", "ApplicableCategories"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Vouchers ADD COLUMN ApplicableCategories TEXT NULL;");
                }

                // Add ApplicableProducts column
                if (!ColumnExists(connection, "Vouchers", "ApplicableProducts"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Vouchers ADD COLUMN ApplicableProducts TEXT NULL;");
                }

                // Add ApplicableCustomerTypes column
                if (!ColumnExists(connection, "Vouchers", "ApplicableCustomerTypes"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Vouchers ADD COLUMN ApplicableCustomerTypes VARCHAR(255) NULL;");
                }

                // Add CreatedBy column
                if (!ColumnExists(connection, "Vouchers", "CreatedBy"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Vouchers ADD COLUMN CreatedBy INT NULL;");
                }

                // Add CreatedDate column
                if (!ColumnExists(connection, "Vouchers", "CreatedDate"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Vouchers ADD COLUMN CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP;");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating Vouchers table: {ex.Message}");
            }
        }

        private static void MigrateInvoicesTable(MySqlConnection connection)
        {
            try
            {
                // Add InvoiceNumber column
                if (!ColumnExists(connection, "Invoices", "InvoiceNumber"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN InvoiceNumber VARCHAR(50) NULL AFTER Id;");
                    // Generate invoice numbers for existing records
                    ExecuteNonQuery(connection, "UPDATE Invoices SET InvoiceNumber = CONCAT('INV-', YEAR(CreatedDate), '-', LPAD(Id, 4, '0')) WHERE InvoiceNumber IS NULL;");
                    // Add unique constraint
                    try
                    {
                        ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD UNIQUE (InvoiceNumber);");
                    }
                    catch { /* Unique constraint might already exist */ }
                }

                // Add VoucherId column
                if (!ColumnExists(connection, "Invoices", "VoucherId"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN VoucherId INT NULL;");
                    try
                    {
                        ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE SET NULL;");
                    }
                    catch { /* FK might already exist */ }
                }

                // Rename Discount to DiscountAmount if needed
                if (ColumnExists(connection, "Invoices", "Discount") && !ColumnExists(connection, "Invoices", "DiscountAmount"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices CHANGE COLUMN Discount DiscountAmount DECIMAL(15,2) NOT NULL DEFAULT 0;");
                }
                else if (!ColumnExists(connection, "Invoices", "DiscountAmount"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN DiscountAmount DECIMAL(15,2) DEFAULT 0;");
                }

                // Add VoucherDiscount column
                if (!ColumnExists(connection, "Invoices", "VoucherDiscount"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN VoucherDiscount DECIMAL(15,2) DEFAULT 0;");
                }

                // Add TierDiscount column
                if (!ColumnExists(connection, "Invoices", "TierDiscount"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN TierDiscount DECIMAL(15,2) DEFAULT 0;");
                }

                // Add PaymentMethod column
                if (!ColumnExists(connection, "Invoices", "PaymentMethod"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN PaymentMethod VARCHAR(50) DEFAULT 'Cash';");
                }

                // Add PaymentStatus column
                if (!ColumnExists(connection, "Invoices", "PaymentStatus"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN PaymentStatus VARCHAR(20) DEFAULT 'Paid';");
                }

                // Add ChangeAmount column
                if (!ColumnExists(connection, "Invoices", "ChangeAmount"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN ChangeAmount DECIMAL(15,2) DEFAULT 0;");
                }

                // Add Notes column
                if (!ColumnExists(connection, "Invoices", "Notes"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN Notes TEXT NULL;");
                }

                // Add Status column
                if (!ColumnExists(connection, "Invoices", "Status"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN Status VARCHAR(20) DEFAULT 'Completed';");
                }

                // Add UpdatedDate column
                if (!ColumnExists(connection, "Invoices", "UpdatedDate"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;");
                }

                // Add CompletedDate column
                if (!ColumnExists(connection, "Invoices", "CompletedDate"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Invoices ADD COLUMN CompletedDate DATETIME NULL;");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating Invoices table: {ex.Message}");
            }
        }

        private static void MigrateInvoiceItemsTable(MySqlConnection connection)
        {
            try
            {
                // Add ProductName column (snapshot)
                if (!ColumnExists(connection, "InvoiceItems", "ProductName"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE InvoiceItems ADD COLUMN ProductName VARCHAR(255) NULL AFTER ProductId;");
                }

                // Add ProductCode column (snapshot)
                if (!ColumnExists(connection, "InvoiceItems", "ProductCode"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE InvoiceItems ADD COLUMN ProductCode VARCHAR(50) NULL AFTER ProductName;");
                }

                // Add DiscountPercent column
                if (!ColumnExists(connection, "InvoiceItems", "DiscountPercent"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE InvoiceItems ADD COLUMN DiscountPercent DECIMAL(5,2) DEFAULT 0;");
                }

                // Add DiscountAmount column
                if (!ColumnExists(connection, "InvoiceItems", "DiscountAmount"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE InvoiceItems ADD COLUMN DiscountAmount DECIMAL(12,2) DEFAULT 0;");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating InvoiceItems table: {ex.Message}");
            }
        }

        private static void CreateStockMovementsTable(MySqlConnection connection)
        {
            try
            {
                string createTableCmd = @"CREATE TABLE IF NOT EXISTS StockMovements (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    ProductId INT NOT NULL,
                    MovementType VARCHAR(20) NOT NULL COMMENT 'Import, Sale, Adjustment, Return, Transfer',
                    Quantity INT NOT NULL COMMENT 'Positive for increase, negative for decrease',
                    PreviousStock INT NOT NULL,
                    NewStock INT NOT NULL,
                    ReferenceType VARCHAR(50) NULL COMMENT 'Invoice, PurchaseOrder, Adjustment, etc.',
                    ReferenceId INT NULL,
                    Notes TEXT NULL,
                    EmployeeId INT NULL,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
                    FOREIGN KEY (EmployeeId) REFERENCES Accounts(Id) ON DELETE SET NULL,
                    INDEX idx_product (ProductId),
                    INDEX idx_created_date (CreatedDate),
                    INDEX idx_movement_type (MovementType)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

                ExecuteNonQuery(connection, createTableCmd);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating StockMovements table: {ex.Message}");
            }
        }

        private static void CreateCustomerVoucherUsageTable(MySqlConnection connection)
        {
            try
            {
                string createTableCmd = @"CREATE TABLE IF NOT EXISTS CustomerVoucherUsage (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    CustomerId INT NOT NULL,
                    VoucherId INT NOT NULL,
                    InvoiceId INT NULL,
                    DiscountAmount DECIMAL(12,2) NOT NULL,
                    UsedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
                    FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE CASCADE,
                    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE SET NULL,
                    INDEX idx_customer (CustomerId),
                    INDEX idx_voucher (VoucherId),
                    INDEX idx_used_date (UsedDate)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

                ExecuteNonQuery(connection, createTableCmd);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating CustomerVoucherUsage table: {ex.Message}");
            }
        }

        private static void CreatePromotionsTable(MySqlConnection connection)
        {
            try
            {
                string createTableCmd = @"CREATE TABLE IF NOT EXISTS Promotions (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Type VARCHAR(50) NOT NULL COMMENT 'FlashSale, BOGO, Combo',
                    StartDate DATETIME NOT NULL,
                    EndDate DATETIME NOT NULL,
                    
                    DiscountPercent DECIMAL(5,2) DEFAULT 0,
                    DiscountAmount DECIMAL(12,2) DEFAULT 0,
                    
                    RequiredQuantity INT DEFAULT 0,
                    RewardProductId INT NULL,
                    RewardQuantity INT DEFAULT 0,
                    TargetCategoryId INT NULL,
                    
                    IsActive BOOLEAN DEFAULT TRUE,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    
                    FOREIGN KEY (RequiredProductId) REFERENCES Products(Id) ON DELETE SET NULL,
                    FOREIGN KEY (RewardProductId) REFERENCES Products(Id) ON DELETE SET NULL,
                    FOREIGN KEY (TargetCategoryId) REFERENCES Categories(Id) ON DELETE SET NULL
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

                ExecuteNonQuery(connection, createTableCmd);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating Promotions table: {ex.Message}");
            }
        }


        private static void MigratePromotionsTable(MySqlConnection connection)
        {
            try
            {
                if (!ColumnExists(connection, "Promotions", "TargetCategoryId"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Promotions ADD COLUMN TargetCategoryId INT NULL AFTER RewardQuantity;");
                    try
                    {
                        ExecuteNonQuery(connection, "ALTER TABLE Promotions ADD FOREIGN KEY (TargetCategoryId) REFERENCES Categories(Id) ON DELETE SET NULL;");
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error migrating Promotions table: {ex.Message}");
            }
        }

        private static void CreateProductVariantsTable(MySqlConnection connection)
        {
            try
            {
                string createTableCmd = @"CREATE TABLE IF NOT EXISTS ProductVariants (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    ProductId INT NOT NULL,
                    Size VARCHAR(50) NULL,
                    Color VARCHAR(50) NULL,
                    Sku VARCHAR(100) NULL,
                    Barcode VARCHAR(100) NULL,
                    StockQuantity INT DEFAULT 0,
                    PriceAdjustment DECIMAL(15,2) DEFAULT 0,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
                    UNIQUE KEY idx_barcode (Barcode),
                    INDEX idx_product (ProductId)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

                ExecuteNonQuery(connection, createTableCmd);

                // Migrate existing stock to variants if variants table was empty
                var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM ProductVariants");
                if (count == 0)
                {
                    string migrateSql = @"
                        INSERT INTO ProductVariants (ProductId, Size, Color, Sku, Barcode, StockQuantity)
                        SELECT Id, 'Default', 'Default', Code, Code, StockQuantity
                        FROM Products
                        WHERE StockQuantity > 0 OR (Code IS NOT NULL AND Code <> '');";
                    int migrated = ExecuteNonQuery(connection, migrateSql);
                    if (migrated > 0)
                    {
                        Debug.WriteLine($"Migrated {migrated} products to default variants.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating ProductVariants table: {ex.Message}");
            }
        }

        private static void CreateErpTables(MySqlConnection connection)
        {
            try
            {
                connection.Execute(@"CREATE TABLE IF NOT EXISTS PurchaseOrders (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    SupplierId INT NOT NULL,
                    EmployeeId INT NOT NULL,
                    TotalAmount DECIMAL(15,2) NOT NULL,
                    PaidAmount DECIMAL(15,2) NOT NULL DEFAULT 0,
                    Status VARCHAR(50) DEFAULT 'Draft',
                    Notes TEXT,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    ReceivedDate DATETIME NULL,
                    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
                    FOREIGN KEY (EmployeeId) REFERENCES Accounts(Id)
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS PurchaseOrderItems (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PurchaseOrderId INT NOT NULL,
                    ProductId INT NOT NULL,
                    Quantity INT NOT NULL,
                    UnitPrice DECIMAL(15,2) NOT NULL,
                    LineTotal DECIMAL(15,2) NOT NULL,
                    FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrders(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                );");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS Expenses (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Category VARCHAR(100) NOT NULL,
                    Amount DECIMAL(15,2) NOT NULL,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Description TEXT,
                    EmployeeId INT NOT NULL,
                    FOREIGN KEY (EmployeeId) REFERENCES Accounts(Id)
                );");

                // Ensure CreatedDate exists if the table was created with 'Date' before
                if (TableExists(connection, "Expenses") && ColumnExists(connection, "Expenses", "Date") && !ColumnExists(connection, "Expenses", "CreatedDate"))
                {
                    connection.Execute("ALTER TABLE Expenses CHANGE COLUMN Date CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP;");
                }

                connection.Execute(@"CREATE TABLE IF NOT EXISTS EmployeeShifts (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    EmployeeId INT NOT NULL,
                    ClockIn DATETIME NOT NULL,
                    ClockOut DATETIME NULL,
                    OpeningBalance DECIMAL(15,2) NOT NULL DEFAULT 0,
                    ClosingBalance DECIMAL(15,2) NULL,
                    Notes TEXT,
                    FOREIGN KEY (EmployeeId) REFERENCES Accounts(Id)
                );");

                // Ensure columns match if created with different names before
                if (TableExists(connection, "EmployeeShifts"))
                {
                    if (ColumnExists(connection, "EmployeeShifts", "StartTime"))
                        connection.Execute("ALTER TABLE EmployeeShifts CHANGE COLUMN StartTime ClockIn DATETIME NOT NULL;");
                    if (ColumnExists(connection, "EmployeeShifts", "EndTime"))
                        connection.Execute("ALTER TABLE EmployeeShifts CHANGE COLUMN EndTime ClockOut DATETIME NULL;");
                    if (ColumnExists(connection, "EmployeeShifts", "StartCash"))
                        connection.Execute("ALTER TABLE EmployeeShifts CHANGE COLUMN StartCash OpeningBalance DECIMAL(15,2) NOT NULL DEFAULT 0;");
                    if (ColumnExists(connection, "EmployeeShifts", "EndCash"))
                        connection.Execute("ALTER TABLE EmployeeShifts CHANGE COLUMN EndCash ClosingBalance DECIMAL(15,2) NULL;");
                }

                Debug.WriteLine("ERP tables checked/created.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating ERP tables: {ex.Message}");
            }
        }

        private static bool TableExists(MySqlConnection connection, string tableName)
        {
            string sql = "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = @TableName AND table_schema = DATABASE();";
            return connection.ExecuteScalar<int>(sql, new { TableName = tableName }) > 0;
        }

        private static bool ColumnExists(MySqlConnection connection, string tableName, string columnName)
        {
            string sql = "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = @TableName AND column_name = @ColumnName AND table_schema = DATABASE();";
            return connection.ExecuteScalar<int>(sql, new { TableName = tableName, ColumnName = columnName }) > 0;
        }

        private static int ExecuteNonQuery(MySqlConnection connection, string sql, object? param = null)
        {
            return connection.Execute(sql, param);
        }
    }
}
