using Dapper;
using FashionStore.Core.Settings;
using MySql.Data.MySqlClient;

namespace FashionStore.Data
{
    public static class DatabaseHelper
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static void InitializeDatabase()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            connection.Execute(@"CREATE TABLE IF NOT EXISTS Accounts (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR(255) NOT NULL UNIQUE,
                Password VARCHAR(255) NOT NULL,
                Role VARCHAR(20) NOT NULL DEFAULT 'Cashier',
                EmployeeName VARCHAR(255)
            );");

            // Ensure EmployeeName column exists
            try
            {
                var checkEmpCol = connection.ExecuteScalar<string>("SHOW COLUMNS FROM Accounts LIKE 'EmployeeName';");
                if (checkEmpCol == null)
                    connection.Execute("ALTER TABLE Accounts ADD COLUMN EmployeeName VARCHAR(255);");
            }
            catch { }

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

            UpdateProductsTable(connection);
            FixExistingProductData(connection);
            DatabaseMigration.RunAllMigrations(connection);

            EnsureAdminAccount(connection);
        }

        private static void EnsureAdminAccount(MySqlConnection connection)
        {
            long adminCount = connection.ExecuteScalar<long>("SELECT COUNT(*) FROM Accounts WHERE Username='admin';");
            if (adminCount == 0)
            {
                string hashedPassword = PasswordHelper.HashPassword("admin");
                connection.Execute("INSERT INTO Accounts (Username, Password, Role) VALUES ('admin', @password, 'Admin');", new { password = hashedPassword });
            }
        }

        private static void UpdateProductsTable(MySqlConnection connection)
        {
            try
            {
                string[] columns = { "Code", "Description", "CreatedDate", "UpdatedDate", "PurchasePrice", "PromoDiscountPercent", "PromoStartDate", "PromoEndDate", "PurchaseUnit", "ImportQuantity", "SupplierId" };
                foreach (var col in columns)
                {
                    var check = connection.ExecuteScalar<string>($"SHOW COLUMNS FROM Products LIKE '{col}';");
                    if (check == null)
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
                            connection.Execute(alterSql);
                        }
                    }
                }
            }
            catch { }
        }

        private static void FixExistingProductData(MySqlConnection connection)
        {
            try { connection.Execute("UPDATE Products SET Code = CONCAT('PROD', LPAD(Id, 4, '0')) WHERE Code IS NULL OR Code = '';"); } catch { }
        }
    }
}
