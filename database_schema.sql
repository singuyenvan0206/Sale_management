-- ============================================================================
-- FASHION STORE DATABASE SCHEMA
-- ============================================================================
-- Version: 2.0
-- Date: 2026-02-07
-- Description: Complete database schema for Fashion Store Management System
-- Currency: VND (Vietnamese Dong)
-- Encoding: UTF-8 (utf8mb4)
-- ============================================================================

-- Create database
CREATE DATABASE IF NOT EXISTS main 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE main;

-- ============================================================================
-- TABLE 1: Accounts (User Management & Authentication)
-- ============================================================================
CREATE TABLE IF NOT EXISTS Accounts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(255) NOT NULL UNIQUE COMMENT 'Unique username for login',
    EmployeeName VARCHAR(255) NULL COMMENT 'Full name of employee',
    Password VARCHAR(255) NOT NULL COMMENT 'Hashed password (BCrypt)',
    Role VARCHAR(20) NOT NULL DEFAULT 'Cashier' COMMENT 'User role: Admin, Manager, Cashier',
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Account creation timestamp',
    LastLoginDate DATETIME NULL COMMENT 'Last successful login',
    IsActive BOOLEAN DEFAULT TRUE COMMENT 'Account status',
    
    INDEX idx_username (Username),
    INDEX idx_role (Role),
    INDEX idx_active (IsActive)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='User accounts and authentication';

-- ============================================================================
-- TABLE 2: Categories (Product Classification)
-- ============================================================================
CREATE TABLE IF NOT EXISTS Categories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL UNIQUE COMMENT 'Category name (e.g., Áo, Quần)',
    TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0.00 COMMENT 'Tax percentage for this category',
    Description TEXT NULL COMMENT 'Category description',
    IsActive BOOLEAN DEFAULT TRUE COMMENT 'Category status',
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_name (Name),
    INDEX idx_active (IsActive)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='Product categories with tax rates';

-- ============================================================================
-- TABLE 3: Suppliers (Vendor Management)
-- ============================================================================
CREATE TABLE IF NOT EXISTS Suppliers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL COMMENT 'Supplier company name',
    ContactName VARCHAR(255) NULL COMMENT 'Contact person name',
    Phone VARCHAR(50) NULL COMMENT 'Contact phone number',
    Email VARCHAR(255) NULL COMMENT 'Contact email address',
    Address TEXT NULL COMMENT 'Supplier address',
    TaxCode VARCHAR(50) NULL COMMENT 'Tax identification number',
    Website VARCHAR(255) NULL COMMENT 'Supplier website',
    Notes TEXT NULL COMMENT 'Additional notes',
    IsActive BOOLEAN DEFAULT TRUE COMMENT 'Supplier status',
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_name (Name),
    INDEX idx_phone (Phone),
    INDEX idx_email (Email),
    INDEX idx_active (IsActive)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='Supplier/vendor information';

-- ============================================================================
-- TABLE 4: Products (Inventory Management)
-- ============================================================================
CREATE TABLE IF NOT EXISTS Products (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL COMMENT 'Product name',
    Code VARCHAR(50) UNIQUE COMMENT 'Unique product code/SKU',
    CategoryId INT NULL COMMENT 'Reference to Categories table',
    SupplierId INT NULL DEFAULT 0 COMMENT 'Reference to Suppliers table',
    
    -- Pricing Information (VND)
    SalePrice DECIMAL(12,2) NOT NULL COMMENT 'Selling price in VND',
    PurchasePrice DECIMAL(12,2) DEFAULT 0.00 COMMENT 'Purchase/cost price in VND',
    PurchaseUnit VARCHAR(50) DEFAULT 'VND' COMMENT 'Currency unit (always VND)',
    
    -- Promotion Information
    PromoDiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0.00 COMMENT 'Promotion discount percentage',
    PromoStartDate DATETIME NULL COMMENT 'Promotion start date',
    PromoEndDate DATETIME NULL COMMENT 'Promotion end date',
    
    -- Inventory Information
    StockQuantity INT NOT NULL DEFAULT 0 COMMENT 'Current stock quantity',
    ImportQuantity INT DEFAULT 0 COMMENT 'Last import quantity',
    MinStockLevel INT DEFAULT 0 COMMENT 'Minimum stock alert level',
    MaxStockLevel INT DEFAULT 0 COMMENT 'Maximum stock capacity',
    
    -- Additional Information
    Description TEXT NULL COMMENT 'Product description',
    Barcode VARCHAR(100) NULL COMMENT 'Product barcode',
    ImageUrl VARCHAR(500) NULL COMMENT 'Product image URL/path',
    Weight DECIMAL(10,2) NULL COMMENT 'Product weight (kg)',
    Dimensions VARCHAR(100) NULL COMMENT 'Product dimensions (LxWxH)',
    
    -- Status & Timestamps
    IsActive BOOLEAN DEFAULT TRUE COMMENT 'Product status',
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE SET NULL,
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE SET NULL,
    
    INDEX idx_name (Name),
    INDEX idx_code (Code),
    INDEX idx_category (CategoryId),
    INDEX idx_supplier (SupplierId),
    INDEX idx_barcode (Barcode),
    INDEX idx_active (IsActive),
    INDEX idx_stock (StockQuantity),
    INDEX idx_price (SalePrice)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='Product inventory and details';

-- ============================================================================
-- TABLE 5: Customers (Customer Relationship Management)
-- ============================================================================
CREATE TABLE IF NOT EXISTS Customers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL COMMENT 'Customer full name',
    Phone VARCHAR(20) NULL COMMENT 'Primary phone number',
    Email VARCHAR(255) NULL COMMENT 'Email address',
    Address TEXT NULL COMMENT 'Customer address',
    
    -- Customer Tier System
    CustomerType VARCHAR(50) DEFAULT 'Regular' COMMENT 'Customer tier: Bronze, Silver, Gold, Platinum',
    Points INT NOT NULL DEFAULT 0 COMMENT 'Loyalty points',
    TotalSpent DECIMAL(15,2) DEFAULT 0.00 COMMENT 'Total amount spent (VND)',
    
    -- Additional Information
    DateOfBirth DATE NULL COMMENT 'Date of birth',
    Gender VARCHAR(10) NULL COMMENT 'Gender: Male, Female, Other',
    Notes TEXT NULL COMMENT 'Customer notes',
    
    -- Status & Timestamps
    IsActive BOOLEAN DEFAULT TRUE COMMENT 'Customer status',
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    LastPurchaseDate DATETIME NULL COMMENT 'Last purchase timestamp',
    
    INDEX idx_name (Name),
    INDEX idx_phone (Phone),
    INDEX idx_email (Email),
    INDEX idx_customer_type (CustomerType),
    INDEX idx_points (Points),
    INDEX idx_active (IsActive)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='Customer information and loyalty program';

-- ============================================================================
-- TABLE 6: Vouchers (Discount & Promotion Management)
-- ============================================================================
CREATE TABLE IF NOT EXISTS Vouchers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Code VARCHAR(50) NOT NULL UNIQUE COMMENT 'Unique voucher code',
    Name VARCHAR(255) NOT NULL COMMENT 'Voucher name/description',
    
    -- Discount Configuration
    DiscountType VARCHAR(20) NOT NULL COMMENT 'Type: Percentage, FixedAmount',
    DiscountValue DECIMAL(12,2) NOT NULL COMMENT 'Discount value (% or VND)',
    MaxDiscountAmount DECIMAL(12,2) NULL COMMENT 'Maximum discount cap (VND)',
    MinInvoiceAmount DECIMAL(12,2) NOT NULL DEFAULT 0.00 COMMENT 'Minimum invoice amount (VND)',
    
    -- Usage Limits
    StartDate DATETIME NOT NULL COMMENT 'Voucher valid from',
    EndDate DATETIME NOT NULL COMMENT 'Voucher valid until',
    UsageLimit INT NOT NULL DEFAULT 0 COMMENT 'Maximum total uses (0 = unlimited)',
    UsageLimitPerCustomer INT DEFAULT 1 COMMENT 'Max uses per customer',
    UsedCount INT NOT NULL DEFAULT 0 COMMENT 'Current usage count',
    
    -- Applicability
    ApplicableCategories TEXT NULL COMMENT 'JSON array of category IDs',
    ApplicableProducts TEXT NULL COMMENT 'JSON array of product IDs',
    ApplicableCustomerTypes TEXT NULL COMMENT 'JSON array of customer tiers',
    
    -- Status
    IsActive BOOLEAN NOT NULL DEFAULT TRUE COMMENT 'Voucher status',
    CreatedBy INT NULL COMMENT 'Created by user ID',
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (CreatedBy) REFERENCES Accounts(Id) ON DELETE SET NULL,
    
    INDEX idx_code (Code),
    INDEX idx_active (IsActive),
    INDEX idx_dates (StartDate, EndDate),
    INDEX idx_discount_type (DiscountType)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='Discount vouchers and promotions';

-- ============================================================================
-- TABLE 7: Invoices (Sales Transactions)
-- ============================================================================
CREATE TABLE IF NOT EXISTS Invoices (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InvoiceNumber VARCHAR(50) UNIQUE COMMENT 'Unique invoice number',
    
    -- References
    CustomerId INT NOT NULL COMMENT 'Reference to Customers table',
    EmployeeId INT NOT NULL COMMENT 'Cashier/employee who created invoice',
    VoucherId INT NULL COMMENT 'Applied voucher (if any)',
    
    -- Financial Breakdown (VND)
    Subtotal DECIMAL(15,2) NOT NULL COMMENT 'Subtotal before tax and discount',
    TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0.00 COMMENT 'Tax percentage',
    TaxAmount DECIMAL(15,2) NOT NULL DEFAULT 0.00 COMMENT 'Tax amount in VND',
    DiscountAmount DECIMAL(15,2) NOT NULL DEFAULT 0.00 COMMENT 'Total discount in VND',
    VoucherDiscount DECIMAL(15,2) DEFAULT 0.00 COMMENT 'Voucher discount amount',
    TierDiscount DECIMAL(15,2) DEFAULT 0.00 COMMENT 'Customer tier discount',
    Total DECIMAL(15,2) NOT NULL COMMENT 'Final total amount',
    
    -- Payment Information
    Paid DECIMAL(15,2) NOT NULL DEFAULT 0.00 COMMENT 'Amount paid',
    PaymentMethod VARCHAR(50) DEFAULT 'Cash' COMMENT 'Payment method: Cash, Card, Transfer, QR',
    PaymentStatus VARCHAR(20) DEFAULT 'Paid' COMMENT 'Status: Paid, Partial, Pending',
    ChangeAmount DECIMAL(15,2) DEFAULT 0.00 COMMENT 'Change returned to customer',
    
    -- Additional Information
    Notes TEXT NULL COMMENT 'Invoice notes',
    Status VARCHAR(20) DEFAULT 'Completed' COMMENT 'Invoice status',
    
    -- Timestamps
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Invoice creation time',
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CompletedDate DATETIME NULL COMMENT 'Invoice completion time',
    
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE RESTRICT,
    FOREIGN KEY (EmployeeId) REFERENCES Accounts(Id) ON DELETE RESTRICT,
    FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE SET NULL,
    
    INDEX idx_invoice_number (InvoiceNumber),
    INDEX idx_customer (CustomerId),
    INDEX idx_employee (EmployeeId),
    INDEX idx_voucher (VoucherId),
    INDEX idx_created_date (CreatedDate),
    INDEX idx_status (Status),
    INDEX idx_payment_method (PaymentMethod)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='Sales invoices and transactions';

-- ============================================================================
-- TABLE 8: InvoiceItems (Invoice Line Items)
-- ============================================================================
CREATE TABLE IF NOT EXISTS InvoiceItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InvoiceId INT NOT NULL COMMENT 'Reference to Invoices table',
    ProductId INT NOT NULL COMMENT 'Reference to Products table',
    EmployeeId INT NOT NULL COMMENT 'Employee who added this item',
    
    -- Product Details (snapshot at time of sale)
    ProductName VARCHAR(255) NOT NULL COMMENT 'Product name at time of sale',
    ProductCode VARCHAR(50) NULL COMMENT 'Product code at time of sale',
    
    -- Pricing & Quantity (VND)
    UnitPrice DECIMAL(12,2) NOT NULL COMMENT 'Unit price at time of sale',
    Quantity INT NOT NULL COMMENT 'Quantity sold',
    DiscountPercent DECIMAL(5,2) DEFAULT 0.00 COMMENT 'Item discount percentage',
    DiscountAmount DECIMAL(12,2) DEFAULT 0.00 COMMENT 'Item discount amount',
    LineTotal DECIMAL(15,2) NOT NULL COMMENT 'Line total (after discount)',
    
    -- Timestamps
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE RESTRICT,
    FOREIGN KEY (EmployeeId) REFERENCES Accounts(Id) ON DELETE RESTRICT,
    
    INDEX idx_invoice (InvoiceId),
    INDEX idx_product (ProductId),
    INDEX idx_employee (EmployeeId)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='Invoice line items and product details';

-- ============================================================================
-- ADDITIONAL TABLES (Optional - for future expansion)
-- ============================================================================

-- TABLE 9: StockMovements (Inventory Tracking)
CREATE TABLE IF NOT EXISTS StockMovements (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    MovementType VARCHAR(20) NOT NULL COMMENT 'Type: Import, Sale, Return, Adjustment',
    Quantity INT NOT NULL COMMENT 'Quantity moved (positive or negative)',
    PreviousStock INT NOT NULL COMMENT 'Stock before movement',
    NewStock INT NOT NULL COMMENT 'Stock after movement',
    ReferenceType VARCHAR(50) NULL COMMENT 'Reference type: Invoice, PurchaseOrder',
    ReferenceId INT NULL COMMENT 'Reference ID',
    Notes TEXT NULL,
    EmployeeId INT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (EmployeeId) REFERENCES Accounts(Id) ON DELETE SET NULL,
    
    INDEX idx_product (ProductId),
    INDEX idx_movement_type (MovementType),
    INDEX idx_created_date (CreatedDate)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='Stock movement history';

-- TABLE 10: CustomerVoucherUsage (Voucher Usage Tracking)
CREATE TABLE IF NOT EXISTS CustomerVoucherUsage (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CustomerId INT NOT NULL,
    VoucherId INT NOT NULL,
    InvoiceId INT NOT NULL,
    DiscountAmount DECIMAL(12,2) NOT NULL,
    UsedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id) ON DELETE CASCADE,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    
    INDEX idx_customer (CustomerId),
    INDEX idx_voucher (VoucherId),
    INDEX idx_invoice (InvoiceId),
    INDEX idx_used_date (UsedDate)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci
COMMENT='Customer voucher usage history';

-- ============================================================================
-- DEFAULT DATA INSERTION
-- ============================================================================

-- Insert default admin account
-- Password: admin (will be hashed by application on first run)
INSERT IGNORE INTO Accounts (Username, EmployeeName, Password, Role) 
VALUES ('admin', 'Administrator', 'admin', 'Admin');

-- Insert default categories
INSERT IGNORE INTO Categories (Name, TaxPercent) VALUES
('Áo', 10.00),
('Quần', 10.00),
('Váy', 10.00),
('Phụ kiện', 8.00),
('Giày dép', 10.00),
('Túi xách', 8.00);

-- Insert default customer (Walk-in customer)
INSERT IGNORE INTO Customers (Name, Phone, CustomerType, Points) 
VALUES ('Khách lẻ', '0000000000', 'Regular', 0);

-- ============================================================================
-- VIEWS FOR REPORTING
-- ============================================================================

-- View: Product Sales Summary
CREATE OR REPLACE VIEW vw_ProductSales AS
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.Code AS ProductCode,
    c.Name AS CategoryName,
    COUNT(ii.Id) AS TotalSales,
    SUM(ii.Quantity) AS TotalQuantitySold,
    SUM(ii.LineTotal) AS TotalRevenue,
    p.StockQuantity AS CurrentStock
FROM Products p
LEFT JOIN Categories c ON p.CategoryId = c.Id
LEFT JOIN InvoiceItems ii ON p.Id = ii.ProductId
GROUP BY p.Id, p.Name, p.Code, c.Name, p.StockQuantity;

-- View: Customer Purchase Summary
CREATE OR REPLACE VIEW vw_CustomerSummary AS
SELECT 
    c.Id AS CustomerId,
    c.Name AS CustomerName,
    c.Phone,
    c.CustomerType,
    c.Points,
    COUNT(i.Id) AS TotalInvoices,
    SUM(i.Total) AS TotalSpent,
    MAX(i.CreatedDate) AS LastPurchaseDate
FROM Customers c
LEFT JOIN Invoices i ON c.Id = i.CustomerId
GROUP BY c.Id, c.Name, c.Phone, c.CustomerType, c.Points;

-- View: Daily Sales Report
CREATE OR REPLACE VIEW vw_DailySales AS
SELECT 
    DATE(i.CreatedDate) AS SaleDate,
    COUNT(i.Id) AS TotalInvoices,
    SUM(i.Subtotal) AS TotalSubtotal,
    SUM(i.TaxAmount) AS TotalTax,
    SUM(i.DiscountAmount) AS TotalDiscount,
    SUM(i.Total) AS TotalRevenue,
    AVG(i.Total) AS AverageInvoiceValue
FROM Invoices i
WHERE i.Status = 'Completed'
GROUP BY DATE(i.CreatedDate)
ORDER BY SaleDate DESC;

-- ============================================================================
-- STORED PROCEDURES
-- ============================================================================

DELIMITER //

-- Procedure: Update Customer Tier based on Points
CREATE PROCEDURE sp_UpdateCustomerTier(IN customerId INT)
BEGIN
    DECLARE customerPoints INT;
    DECLARE newTier VARCHAR(50);
    
    SELECT Points INTO customerPoints FROM Customers WHERE Id = customerId;
    
    SET newTier = CASE
        WHEN customerPoints >= 10000 THEN 'Platinum'
        WHEN customerPoints >= 5000 THEN 'Gold'
        WHEN customerPoints >= 2000 THEN 'Silver'
        WHEN customerPoints >= 500 THEN 'Bronze'
        ELSE 'Regular'
    END;
    
    UPDATE Customers SET CustomerType = newTier WHERE Id = customerId;
END//

-- Procedure: Calculate Low Stock Products
CREATE PROCEDURE sp_GetLowStockProducts()
BEGIN
    SELECT 
        p.Id,
        p.Name,
        p.Code,
        p.StockQuantity,
        p.MinStockLevel,
        c.Name AS CategoryName
    FROM Products p
    LEFT JOIN Categories c ON p.CategoryId = c.Id
    WHERE p.StockQuantity <= p.MinStockLevel
    AND p.IsActive = TRUE
    ORDER BY p.StockQuantity ASC;
END//

DELIMITER ;

-- ============================================================================
-- TRIGGERS
-- ============================================================================

DELIMITER //

-- Trigger: Update stock after invoice item insert
CREATE TRIGGER trg_AfterInvoiceItemInsert
AFTER INSERT ON InvoiceItems
FOR EACH ROW
BEGIN
    UPDATE Products 
    SET StockQuantity = StockQuantity - NEW.Quantity
    WHERE Id = NEW.ProductId;
END//

-- Trigger: Update customer total spent after invoice
CREATE TRIGGER trg_AfterInvoiceInsert
AFTER INSERT ON Invoices
FOR EACH ROW
BEGIN
    UPDATE Customers 
    SET TotalSpent = TotalSpent + NEW.Total,
        LastPurchaseDate = NEW.CreatedDate
    WHERE Id = NEW.CustomerId;
END//

-- Trigger: Update voucher used count
CREATE TRIGGER trg_AfterVoucherUsed
AFTER INSERT ON Invoices
FOR EACH ROW
BEGIN
    IF NEW.VoucherId IS NOT NULL THEN
        UPDATE Vouchers 
        SET UsedCount = UsedCount + 1
        WHERE Id = NEW.VoucherId;
    END IF;
END//

DELIMITER ;

-- ============================================================================
-- INDEXES FOR PERFORMANCE OPTIMIZATION
-- ============================================================================

-- Additional composite indexes for common queries
CREATE INDEX idx_invoice_customer_date ON Invoices(CustomerId, CreatedDate);
CREATE INDEX idx_invoice_employee_date ON Invoices(EmployeeId, CreatedDate);
CREATE INDEX idx_product_category_active ON Products(CategoryId, IsActive);
CREATE INDEX idx_customer_type_active ON Customers(CustomerType, IsActive);

-- ============================================================================
-- NOTES & DOCUMENTATION
-- ============================================================================

/*
CURRENCY INFORMATION:
- All monetary values are in VND (Vietnamese Dong)
- PurchaseUnit field is set to 'VND' by default
- No decimal places needed for VND, but DECIMAL(15,2) used for precision

CUSTOMER TIER SYSTEM:
- Regular: 0-499 points
- Bronze: 500-1999 points
- Silver: 2000-4999 points
- Gold: 5000-9999 points
- Platinum: 10000+ points

VOUCHER TYPES:
- Percentage: Discount as percentage of invoice total
- FixedAmount: Fixed amount discount in VND

PAYMENT METHODS:
- Cash: Cash payment
- Card: Credit/Debit card
- Transfer: Bank transfer
- QR: QR code payment (Momo, ZaloPay, etc.)

STOCK MOVEMENT TYPES:
- Import: Stock received from supplier
- Sale: Stock sold to customer
- Return: Stock returned by customer
- Adjustment: Manual stock adjustment

APPLICATION NOTES:
- The application will automatically hash passwords on first run
- Default admin credentials: username=admin, password=admin
- All tables use InnoDB engine for transaction support
- UTF-8 encoding (utf8mb4) for full Unicode support including emojis
*/

-- ============================================================================
-- END OF SCHEMA
-- ============================================================================
