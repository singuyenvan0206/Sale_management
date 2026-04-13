using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;
using Dapper;

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
                MigrateAccountsTable(connection);
                MigrateCategoriesTable(connection);
                MigrateSuppliersTable(connection);
                MigrateProductsTable(connection);
                MigrateCustomersTable(connection);
                MigrateVouchersTable(connection);
                MigrateInvoicesTable(connection);
                MigrateInvoiceItemsTable(connection);
                CreateStockMovementsTable(connection);
                CreateCustomerVoucherUsageTable(connection);
                CreatePromotionsTable(connection);
                MigratePromotionsTable(connection);
                SeedSampleData(connection);
                BackfillBarcodes(connection);

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
                // Disable safe updates for this session to allow mass update
                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 0;");

                // Generate barcodes for any products that have empty or null Code
                // We use Id > 0 to satisfy some safe-update configurations even with safe updates off
                // Pattern: 893 + 7-digit padded ID (Total 10 digits)
                string sql = @"
                    UPDATE Products 
                    SET Code = CONCAT('893', LPAD(Id, 7, '0')) 
                    WHERE (Code IS NULL OR TRIM(Code) = '') AND Id > 0;";
                
                int affected = ExecuteNonQuery(connection, sql);
                if (affected > 0)
                {
                    Debug.WriteLine($"Successfully generated barcodes for {affected} products.");
                }

                // Re-enable safe updates
                ExecuteNonQuery(connection, "SET SQL_SAFE_UPDATES = 1;");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error backfilling barcodes: {ex.Message}");
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
                    ExecuteNonQuery(connection, $"INSERT IGNORE INTO Categories (Name, TaxPercent) VALUES ('{cat}', 10);");
                }

                // 2. Seed Suppliers
                ExecuteNonQuery(connection, "INSERT IGNORE INTO Suppliers (Name, ContactName, Phone, Address) VALUES ('Công ty May Mặc Á Châu', 'Nguyễn Văn A', '0901234567', '123 Đường ABC, HCM');");

                // Get some IDs
                int catId = connection.ExecuteScalar<int>("SELECT Id FROM Categories WHERE Name='Áo Sơ Mi' LIMIT 1");
                int supId = connection.ExecuteScalar<int>("SELECT Id FROM Suppliers WHERE Name='Công ty May Mặc Á Châu' LIMIT 1");

                if (catId > 0)
                {
                    string[][] products = new[]
                    {
                        new[] { "Áo Sơ Mi Công Sở Nam", "8930001", "350000", "200000", "Chất liệu cotton cao cấp" },
                        new[] { "Quần Jean Skinny Nữ", "8930002", "450000", "250000", "Co giãn 4 chiều" },
                        new[] { "Giày Sneaker Classic", "8930003", "600000", "350000", "Phong cách trẻ trung" },
                        new[] { "Thắt Lưng Da", "8930004", "150000", "80000", "Da bò thật 100%" }
                    };

                    foreach (var p in products)
                    {
                        // Use INSERT IGNORE combined with the unique index on 'Code'
                        // This ensures we ONLY add these products if their barcode doesn't exist
                        string sql = "INSERT IGNORE INTO Products (Name, Code, CategoryId, SalePrice, PurchasePrice, PurchaseUnit, ImportQuantity, StockQuantity, Description, SupplierId) " +
                                     $"VALUES ('{p[0]}', '{p[1]}', {catId}, {p[2]}, {p[3]}, 'Cái', 100, 50, '{p[4]}', {supId});";
                        ExecuteNonQuery(connection, sql);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error seeding sample data: {ex.Message}");
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

                // Cleanup: Remove redundant columns if they exist (Barcode, ImageUrl, Weight, Dimensions)
                string[] unusedColumns = { "Barcode", "ImageUrl", "Weight", "Dimensions" };
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

        private static bool ColumnExists(MySqlConnection connection, string tableName, string columnName)
        {
            try
            {
                string checkCmd = $"SHOW COLUMNS FROM {tableName} LIKE '{columnName}';";
                var result = connection.ExecuteScalar<string>(checkCmd);
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        private static int ExecuteNonQuery(MySqlConnection connection, string sql)
        {
            return connection.Execute(sql);
        }
    }
}
