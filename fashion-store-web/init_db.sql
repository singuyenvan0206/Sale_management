-- 1. Xóa các bảng cũ nếu tồn tại (theo thứ tự ngược để tránh lỗi khóa ngoại)
DROP TABLE IF EXISTS "customervoucherusage" CASCADE;
DROP TABLE IF EXISTS "stockmovements" CASCADE;
DROP TABLE IF EXISTS "invoiceitems" CASCADE;
DROP TABLE IF EXISTS "invoices" CASCADE;
DROP TABLE IF EXISTS "products" CASCADE;
DROP TABLE IF EXISTS "suppliers" CASCADE;
DROP TABLE IF EXISTS "vouchers" CASCADE;
DROP TABLE IF EXISTS "promotions" CASCADE;
DROP TABLE IF EXISTS "customers" CASCADE;
DROP TABLE IF EXISTS "categories" CASCADE;
DROP TABLE IF EXISTS "accounts" CASCADE;

-- 2. Tạo bảng Tài khoản (Accounts)
CREATE TABLE "accounts" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(255) NOT NULL UNIQUE,
    "Password" VARCHAR(255) NOT NULL,
    "Role" VARCHAR(20) NOT NULL DEFAULT 'Cashier',
    "EmployeeName" VARCHAR(255),
    "LastLoginDate" TIMESTAMP,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Tạo bảng Danh mục (Categories)
CREATE TABLE "categories" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL UNIQUE,
    "TaxPercent" DECIMAL(5,2) DEFAULT 0.00,
    "Description" TEXT,
    "IsActive" BOOLEAN DEFAULT TRUE
);

-- 4. Tạo bảng Khách hàng (Customers)
CREATE TABLE "customers" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "Phone" VARCHAR(20),
    "Email" VARCHAR(255),
    "Address" TEXT,
    "CustomerType" VARCHAR(50) DEFAULT 'Regular',
    "Points" INT DEFAULT 0,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 5. Tạo bảng Nhà cung cấp (Suppliers)
CREATE TABLE "suppliers" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "Phone" VARCHAR(20),
    "Email" VARCHAR(255),
    "Address" TEXT,
    "Description" TEXT,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 6. Tạo bảng Sản phẩm (Products)
CREATE TABLE "products" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "Code" VARCHAR(50) UNIQUE,
    "CategoryId" INT REFERENCES "categories"("Id"),
    "SupplierId" INT REFERENCES "suppliers"("Id"),
    "SalePrice" DECIMAL(15,2) NOT NULL,
    "PurchasePrice" DECIMAL(15,2) DEFAULT 0.00,
    "StockQuantity" INT DEFAULT 0,
    "Description" TEXT,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 7. Tạo bảng Voucher (Vouchers)
CREATE TABLE "vouchers" (
    "Id" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) UNIQUE NOT NULL,
    "DiscountType" VARCHAR(10) DEFAULT 'VND', -- 'VND' hoặc '%'
    "DiscountValue" DECIMAL(15,2) NOT NULL,
    "MinInvoiceAmount" DECIMAL(15,2) DEFAULT 0.00,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "UsageLimit" INT DEFAULT 100,
    "UsedCount" INT DEFAULT 0,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 8. Tạo bảng Khuyến mãi (Promotions)
CREATE TABLE "promotions" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "Type" VARCHAR(50) DEFAULT 'FlashSale',
    "DiscountPercent" DECIMAL(5,2) DEFAULT 0.00,
    "DiscountAmount" DECIMAL(15,2) DEFAULT 0.00,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "TargetCategoryId" INT REFERENCES "categories"("Id"),
    "RequiredProductId" INT REFERENCES "products"("Id"),
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 9. Tạo bảng Hóa đơn (Invoices)
CREATE TABLE "invoices" (
    "Id" SERIAL PRIMARY KEY,
    "InvoiceNumber" VARCHAR(50) UNIQUE,
    "CustomerId" INT REFERENCES "customers"("Id"),
    "EmployeeId" INT REFERENCES "accounts"("Id"),
    "VoucherId" INT REFERENCES "vouchers"("Id"),
    "Subtotal" DECIMAL(15,2) NOT NULL,
    "TaxAmount" DECIMAL(15,2) DEFAULT 0.00,
    "DiscountAmount" DECIMAL(15,2) DEFAULT 0.00,
    "Total" DECIMAL(15,2) NOT NULL,
    "Paid" DECIMAL(15,2) DEFAULT 0.00,
    "PaymentMethod" VARCHAR(50) DEFAULT 'Cash',
    "Status" VARCHAR(20) DEFAULT 'Completed',
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 10. Tạo bảng Chi tiết hóa đơn (InvoiceItems)
CREATE TABLE "invoiceitems" (
    "Id" SERIAL PRIMARY KEY,
    "InvoiceId" INT REFERENCES "invoices"("Id") ON DELETE CASCADE,
    "ProductId" INT REFERENCES "products"("Id"),
    "EmployeeId" INT REFERENCES "accounts"("Id"),
    "UnitPrice" DECIMAL(15,2) NOT NULL,
    "Quantity" INT NOT NULL,
    "LineTotal" DECIMAL(15,2) NOT NULL
);

-- 11. Tạo bảng Biến động kho (StockMovements)
CREATE TABLE "stockmovements" (
    "Id" SERIAL PRIMARY KEY,
    "ProductId" INT REFERENCES "products"("Id"),
    "Type" VARCHAR(20) NOT NULL, -- 'IN' (Nhập hàng), 'OUT' (Bán hàng), 'ADJUST' (Điều chỉnh)
    "Quantity" INT NOT NULL,
    "ReferenceId" INT, -- ID của Hóa đơn nếu là Bán hàng
    "Note" TEXT,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 12. Tạo bảng Lịch sử dùng Voucher (CustomerVoucherUsage)
CREATE TABLE "customervoucherusage" (
    "Id" SERIAL PRIMARY KEY,
    "CustomerId" INT REFERENCES "customers"("Id"),
    "VoucherId" INT REFERENCES "vouchers"("Id"),
    "InvoiceId" INT REFERENCES "invoices"("Id"),
    "UsedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 13. Dữ liệu mẫu ban đầu
INSERT INTO "accounts" ("Username", "Password", "Role", "EmployeeName") 
VALUES ('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'Admin', 'Administrator');

INSERT INTO "categories" ("Name", "Description") VALUES 
('Thời trang Nam', 'Áo thun, quần tây, vest nam'),
('Thời trang Nữ', 'Váy, đầm, thời trang công sở nữ'),
('Phụ kiện', 'Túi xách, thắt lưng, ví');

INSERT INTO "customers" ("Name", "Phone", "Address", "CustomerType") VALUES 
('Khách lẻ', '0000000000', 'Tại cửa hàng', 'Regular'),
('Nguyễn Văn A', '0912345678', 'Hà Nội', 'VIP');

INSERT INTO "suppliers" ("Name", "Phone", "Email", "Address") VALUES 
('Xưởng May Việt Tiến', '0281234567', 'contact@viettien.com', 'TP.HCM'),
('Ninh Hiệp Fashion', '0988888888', 'ninhhiep@gmail.com', 'Hà Nội');

INSERT INTO "products" ("Name", "Code", "CategoryId", "SalePrice", "StockQuantity") VALUES 
('Áo Sơ Mi Trắng Premium', 'SHIRT001', 1, 450000, 50),
('Quần Jean Slim-fit', 'JEAN001', 1, 650000, 30),
('Váy Hoa Vintage', 'DRESS001', 2, 890000, 20),
('Túi Xách Da Cao Cấp', 'BAG001', 3, 1200000, 15);

INSERT INTO "vouchers" ("Code", "DiscountType", "DiscountValue", "MinInvoiceAmount", "StartDate", "EndDate") VALUES 
('WELCOME2024', 'VND', 50000, 200000, '2024-01-01', '2026-12-31'),
('SALE10', '%', 10, 500000, '2024-01-01', '2026-12-31');
