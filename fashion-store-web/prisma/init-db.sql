-- Database Initialization Script
CREATE DATABASE IF NOT EXISTS main;
USE main;

-- Accounts
CREATE TABLE IF NOT EXISTS accounts (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Username VARCHAR(255) UNIQUE NOT NULL,
  Password VARCHAR(255) NOT NULL,
  Role VARCHAR(20) DEFAULT 'Cashier',
  employeeName VARCHAR(100),
  LastLoginDate DATETIME,
  IsActive BOOLEAN DEFAULT TRUE,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Categories
CREATE TABLE IF NOT EXISTS categories (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Name VARCHAR(255) UNIQUE NOT NULL,
  TaxPercent DECIMAL(5,2) DEFAULT 0.00,
  Description TEXT,
  IsActive BOOLEAN DEFAULT TRUE
);

-- Suppliers
CREATE TABLE IF NOT EXISTS suppliers (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Name VARCHAR(255) NOT NULL,
  ContactName VARCHAR(255),
  Phone VARCHAR(50),
  Email VARCHAR(255),
  Address TEXT,
  TaxCode VARCHAR(50),
  Website VARCHAR(255),
  Notes TEXT,
  IsActive BOOLEAN DEFAULT TRUE,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Products
CREATE TABLE IF NOT EXISTS products (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Name VARCHAR(255) NOT NULL,
  Code VARCHAR(50) UNIQUE,
  CategoryId INT,
  SalePrice DECIMAL(10,2) NOT NULL,
  PromoDiscountPercent DECIMAL(5,2) DEFAULT 0.00,
  PromoStartDate DATETIME,
  PromoEndDate DATETIME,
  PurchasePrice DECIMAL(10,2) DEFAULT 0.00,
  PurchaseUnit VARCHAR(50) DEFAULT 'Cái',
  ImportQuantity INT DEFAULT 0,
  StockQuantity INT DEFAULT 0,
  Description TEXT,
  SupplierId INT DEFAULT 0,
  MinStockLevel INT DEFAULT 10,
  MaxStockLevel INT DEFAULT 1000,
  IsActive BOOLEAN DEFAULT TRUE,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
  UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (CategoryId) REFERENCES categories(Id)
);

-- Customers
CREATE TABLE IF NOT EXISTS customers (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Name VARCHAR(255) NOT NULL,
  Phone VARCHAR(20),
  Email VARCHAR(255),
  Address TEXT,
  CustomerType VARCHAR(50) DEFAULT 'Regular',
  Points INT DEFAULT 0,
  TotalSpent DECIMAL(15,2) DEFAULT 0.00,
  DateOfBirth DATE,
  Gender VARCHAR(10),
  LastPurchaseDate DATETIME,
  IsActive BOOLEAN DEFAULT TRUE,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
  UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Vouchers
CREATE TABLE IF NOT EXISTS vouchers (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Code VARCHAR(50) UNIQUE NOT NULL,
  Name VARCHAR(255),
  DiscountType VARCHAR(20) NOT NULL,
  DiscountValue DECIMAL(12,2) NOT NULL,
  MinInvoiceAmount DECIMAL(12,2) DEFAULT 0.00,
  StartDate DATETIME NOT NULL,
  EndDate DATETIME NOT NULL,
  UsageLimit INT DEFAULT 0,
  UsedCount INT DEFAULT 0,
  IsActive BOOLEAN DEFAULT TRUE,
  MaxDiscountAmount DECIMAL(12,2),
  UsageLimitPerCustomer INT DEFAULT 0,
  ApplicableCategories TEXT,
  ApplicableProducts TEXT,
  ApplicableCustomerTypes VARCHAR(255),
  CreatedBy INT,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Invoices
CREATE TABLE IF NOT EXISTS invoices (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  InvoiceNumber VARCHAR(50) UNIQUE,
  CustomerId INT NOT NULL,
  EmployeeId INT NOT NULL,
  Subtotal DECIMAL(12,2) NOT NULL,
  TaxPercent DECIMAL(5,2) DEFAULT 0.00,
  TaxAmount DECIMAL(12,2) DEFAULT 0.00,
  DiscountAmount DECIMAL(15,2) DEFAULT 0.00,
  Total DECIMAL(12,2) NOT NULL,
  Paid DECIMAL(12,2) DEFAULT 0.00,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
  VoucherId INT,
  VoucherDiscount DECIMAL(15,2) DEFAULT 0.00,
  TierDiscount DECIMAL(15,2) DEFAULT 0.00,
  PaymentMethod VARCHAR(50) DEFAULT 'Cash',
  PaymentStatus VARCHAR(20) DEFAULT 'Paid',
  ChangeAmount DECIMAL(15,2) DEFAULT 0.00,
  Notes TEXT,
  Status VARCHAR(20) DEFAULT 'Completed',
  UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CompletedDate DATETIME,
  FOREIGN KEY (CustomerId) REFERENCES customers(Id),
  FOREIGN KEY (EmployeeId) REFERENCES accounts(Id),
  FOREIGN KEY (VoucherId) REFERENCES vouchers(Id)
);

-- Invoice Items
CREATE TABLE IF NOT EXISTS invoiceitems (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  InvoiceId INT NOT NULL,
  ProductId INT NOT NULL,
  ProductName VARCHAR(255),
  ProductCode VARCHAR(50),
  EmployeeId INT NOT NULL,
  UnitPrice DECIMAL(12,2) NOT NULL,
  Quantity INT NOT NULL,
  LineTotal DECIMAL(12,2) NOT NULL,
  DiscountPercent DECIMAL(5,2) DEFAULT 0.00,
  DiscountAmount DECIMAL(12,2) DEFAULT 0.00,
  FOREIGN KEY (InvoiceId) REFERENCES invoices(Id) ON DELETE CASCADE,
  FOREIGN KEY (ProductId) REFERENCES products(Id),
  FOREIGN KEY (EmployeeId) REFERENCES accounts(Id)
);

-- Stock Movements
CREATE TABLE IF NOT EXISTS stockmovements (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  ProductId INT NOT NULL,
  MovementType VARCHAR(20) NOT NULL,
  Quantity INT NOT NULL,
  PreviousStock INT NOT NULL,
  NewStock INT NOT NULL,
  ReferenceType VARCHAR(50),
  ReferenceId INT,
  Notes TEXT,
  EmployeeId INT,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (ProductId) REFERENCES products(Id) ON DELETE CASCADE,
  FOREIGN KEY (EmployeeId) REFERENCES accounts(Id)
);

-- Customer Voucher Usage
CREATE TABLE IF NOT EXISTS customervoucherusage (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  CustomerId INT NOT NULL,
  VoucherId INT NOT NULL,
  InvoiceId INT,
  DiscountAmount DECIMAL(12,2) NOT NULL,
  UsedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (CustomerId) REFERENCES customers(Id) ON DELETE CASCADE,
  FOREIGN KEY (VoucherId) REFERENCES vouchers(Id) ON DELETE CASCADE,
  FOREIGN KEY (InvoiceId) REFERENCES invoices(Id)
);

-- Feature Flags
CREATE TABLE IF NOT EXISTS featureflags (
  `Key` VARCHAR(100) PRIMARY KEY,
  IsEnabled BOOLEAN DEFAULT TRUE,
  UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Default Admin Account (password is 'admin' hashed by bcrypt)
-- Using a dummy hash for now, you should update it after first login
INSERT IGNORE INTO accounts (Username, Password, Role, employeeName) 
VALUES ('admin', '$2b$10$EpjXWzO28OuS7lW5RkM8DeR5M.O5O2D3yO5O2D3yO5O2D3yO5O2D', 'Admin', 'Root Admin');
