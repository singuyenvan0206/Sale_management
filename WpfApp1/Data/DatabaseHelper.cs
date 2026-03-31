using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FashionStore.Data
{
    using FashionStore.Services;
    using FashionStore.Core;
    public static class DatabaseHelper
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static void InitializeDatabase()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            string tableCmd = @"CREATE TABLE IF NOT EXISTS Accounts (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR(255) NOT NULL UNIQUE,
                Password VARCHAR(255) NOT NULL,
                Role VARCHAR(20) NOT NULL DEFAULT 'Cashier',
                EmployeeName VARCHAR(255)
            );";
            using var cmd = new MySqlCommand(tableCmd, connection);
            cmd.ExecuteNonQuery();

            // Ensure EmployeeName column exists
            try
            {
                using var checkEmpCol = new MySqlCommand("SHOW COLUMNS FROM Accounts LIKE 'EmployeeName';", connection);
                if (checkEmpCol.ExecuteScalar() == null)
                {
                    using var addEmpCol = new MySqlCommand("ALTER TABLE Accounts ADD COLUMN EmployeeName VARCHAR(255);", connection);
                    addEmpCol.ExecuteNonQuery();
                }
            }
            catch { }

            string categoryCmd = @"CREATE TABLE IF NOT EXISTS Categories (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL UNIQUE,
                TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0
            );";
            using var catCmd = new MySqlCommand(categoryCmd, connection);
            catCmd.ExecuteNonQuery();

            string productCmd = @"CREATE TABLE IF NOT EXISTS Products (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Code VARCHAR(50) UNIQUE,
                CategoryId INT,
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
            );";
            using var prodCmd = new MySqlCommand(productCmd, connection);
            prodCmd.ExecuteNonQuery();

            string customerCmd = @"CREATE TABLE IF NOT EXISTS Customers (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Phone VARCHAR(20),
                Email VARCHAR(255),
                Address TEXT,
                CustomerType VARCHAR(50) DEFAULT 'Regular',
                Points INT NOT NULL DEFAULT 0,
                UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
            );";
            using var custCmd = new MySqlCommand(customerCmd, connection);
            custCmd.ExecuteNonQuery();

            string invoicesCmd = @"CREATE TABLE IF NOT EXISTS Invoices (
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
            );";
            using var invCmd = new MySqlCommand(invoicesCmd, connection);
            invCmd.ExecuteNonQuery();

            string invoiceItemsCmd = @"CREATE TABLE IF NOT EXISTS InvoiceItems (
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
            );";
            using var invItemsCmd = new MySqlCommand(invoiceItemsCmd, connection);
            invItemsCmd.ExecuteNonQuery();

            string suppliersCmd = @"CREATE TABLE IF NOT EXISTS Suppliers (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                ContactName VARCHAR(255),
                Phone VARCHAR(50),
                Email VARCHAR(255),
                Address TEXT
            );";
            using var supCmd = new MySqlCommand(suppliersCmd, connection);
            supCmd.ExecuteNonQuery();

            string vouchersCmd = @"CREATE TABLE IF NOT EXISTS Vouchers (
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
            );";
            using var vouchCmd = new MySqlCommand(vouchersCmd, connection);
            vouchCmd.ExecuteNonQuery();

            UpdateProductsTable(connection);
            FixExistingProductData(connection);
            DatabaseMigration.RunAllMigrations(connection);

            EnsureAdminAccount(connection);

            // Migrate unhashed passwords
            UserService.MigratePasswordsToHashed(connection);
        }

        private static void EnsureAdminAccount(MySqlConnection connection)
        {
            string checkAdminCmd = "SELECT COUNT(*) FROM Accounts WHERE Username='admin';";
            using var checkCmd = new MySqlCommand(checkAdminCmd, connection);
            if (Convert.ToInt64(checkCmd.ExecuteScalar()) == 0)
            {
                string hashedPassword = PasswordHelper.HashPassword("admin");
                string insertAdminCmd = "INSERT INTO Accounts (Username, Password, Role) VALUES ('admin', @password, 'Admin');";
                using var insertCmd = new MySqlCommand(insertAdminCmd, connection);
                insertCmd.Parameters.AddWithValue("@password", hashedPassword);
                insertCmd.ExecuteNonQuery();
            }
        }

        private static void UpdateProductsTable(MySqlConnection connection)
        {
            try
            {
                // Core column checks and migrations
                string[] columns = { "Code", "Description", "CreatedDate", "UpdatedDate", "PurchasePrice", "PromoDiscountPercent", "PromoStartDate", "PromoEndDate", "PurchaseUnit", "ImportQuantity", "SupplierId" };
                foreach (var col in columns)
                {
                    using var check = new MySqlCommand($"SHOW COLUMNS FROM Products LIKE '{col}';", connection);
                    if (check.ExecuteScalar() == null)
                    {
                        string alterSql = col switch
                        {
                            "Code" => "ALTER TABLE Products ADD COLUMN Code VARCHAR(50);",
                            "Description" => "ALTER TABLE Products ADD COLUMN Description TEXT;",
                            "CreatedDate" => "ALTER TABLE Products ADD COLUMN CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP;",
                            "UpdatedDate" => "ALTER TABLE Products ADD COLUMN UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;",
                            "PurchasePrice" => "ALTER TABLE Products ADD COLUMN PurchasePrice DECIMAL(10,2) DEFAULT 0 AFTER SalePrice;",
                            "PromoDiscountPercent" => "ALTER TABLE Products ADD COLUMN PromoDiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0 AFTER SalePrice;",
                            "PromoStartDate" => "ALTER TABLE Products ADD COLUMN PromoStartDate DATETIME NULL AFTER PromoDiscountPercent;",
                            "PromoEndDate" => "ALTER TABLE Products ADD COLUMN PromoEndDate DATETIME NULL AFTER PromoStartDate;",
                            "PurchaseUnit" => "ALTER TABLE Products ADD COLUMN PurchaseUnit VARCHAR(50) DEFAULT 'VND' AFTER PurchasePrice;",
                            "ImportQuantity" => "ALTER TABLE Products ADD COLUMN ImportQuantity INT DEFAULT 0 AFTER PurchaseUnit;",
                            "SupplierId" => "ALTER TABLE Products ADD COLUMN SupplierId INT DEFAULT 0;",
                            _ => ""
                        };
                        if (!string.IsNullOrEmpty(alterSql))
                        {
                            using var alterCmd = new MySqlCommand(alterSql, connection);
                            alterCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch { }
        }

        private static void FixExistingProductData(MySqlConnection connection)
        {
            try
            {
                string fixCodesCmd = "UPDATE Products SET Code = CONCAT('PROD', LPAD(Id, 4, '0')) WHERE Code IS NULL OR Code = '';";
                using var fixCodes = new MySqlCommand(fixCodesCmd, connection);
                fixCodes.ExecuteNonQuery();
            }
            catch { }
        }
    }
}

