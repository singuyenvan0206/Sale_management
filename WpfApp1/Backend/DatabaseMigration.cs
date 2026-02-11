using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;

namespace WpfApp1
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
                
                Debug.WriteLine("All database migrations completed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during database migration: {ex.Message}");
                throw;
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

                // Add Barcode column
                if (!ColumnExists(connection, "Products", "Barcode"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Products ADD COLUMN Barcode VARCHAR(100) NULL;");
                }

                // Add ImageUrl column
                if (!ColumnExists(connection, "Products", "ImageUrl"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Products ADD COLUMN ImageUrl VARCHAR(500) NULL;");
                }

                // Add Weight column
                if (!ColumnExists(connection, "Products", "Weight"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Products ADD COLUMN Weight DECIMAL(10,2) NULL;");
                }

                // Add Dimensions column
                if (!ColumnExists(connection, "Products", "Dimensions"))
                {
                    ExecuteNonQuery(connection, "ALTER TABLE Products ADD COLUMN Dimensions VARCHAR(100) NULL;");
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

        private static bool ColumnExists(MySqlConnection connection, string tableName, string columnName)
        {
            try
            {
                string checkCmd = $"SHOW COLUMNS FROM {tableName} LIKE '{columnName}';";
                using var cmd = new MySqlCommand(checkCmd, connection);
                var result = cmd.ExecuteScalar();
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        private static void ExecuteNonQuery(MySqlConnection connection, string sql)
        {
            using var cmd = new MySqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }
    }
}
