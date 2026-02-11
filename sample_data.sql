-- ============================================================================
-- FASHION STORE - SAMPLE DATA (FIXED VERSION)
-- ============================================================================
-- Version: 2.1
-- Date: 2026-02-07
-- Description: Sample data for testing and demonstration
-- Note: Import this AFTER importing database_schema.sql
-- This version includes all error prevention and explicit IDs
-- ============================================================================

USE main;

-- ============================================================================
-- SAFETY CHECKS AND CLEANUP
-- ============================================================================

-- Disable foreign key checks temporarily for easier insertion
SET FOREIGN_KEY_CHECKS = 0;

-- Clear existing sample data (optional - uncomment if needed)
-- DELETE FROM CustomerVoucherUsage WHERE Id > 0;
-- DELETE FROM StockMovements WHERE Id > 0;
-- DELETE FROM InvoiceItems WHERE Id > 0;
-- DELETE FROM Invoices WHERE Id > 0;
-- DELETE FROM Vouchers WHERE Id > 0;
-- DELETE FROM Products WHERE Id > 0;
-- DELETE FROM Customers WHERE Id > 1; -- Keep walk-in customer
-- DELETE FROM Suppliers WHERE Id > 0;
-- DELETE FROM Categories WHERE Id > 0;
-- DELETE FROM Accounts WHERE Id > 1; -- Keep admin

-- ============================================================================
-- 1. ACCOUNTS (Employees & Users)
-- ============================================================================
-- Note: Password 'admin' will be hashed by application on first run
INSERT IGNORE INTO Accounts (Id, Username, EmployeeName, Password, Role, IsActive, CreatedDate) VALUES
(1, 'admin', 'Nguyễn Văn Admin', 'admin', 'Admin', TRUE, '2024-01-01 08:00:00'),
(2, 'manager1', 'Trần Thị Lan', 'manager123', 'Manager', TRUE, '2024-01-15 09:00:00'),
(3, 'cashier1', 'Lê Văn Hùng', 'cashier123', 'Cashier', TRUE, '2024-02-01 08:30:00'),
(4, 'cashier2', 'Phạm Thị Mai', 'cashier123', 'Cashier', TRUE, '2024-02-01 08:30:00'),
(5, 'cashier3', 'Hoàng Văn Nam', 'cashier123', 'Cashier', TRUE, '2024-03-01 08:30:00');

-- ============================================================================
-- 2. CATEGORIES (Product Categories)
-- ============================================================================
INSERT IGNORE INTO Categories (Id, Name, TaxPercent, Description, IsActive) VALUES
(1, 'Áo Thun', 10.00, 'Áo thun nam nữ các loại', TRUE),
(2, 'Áo Sơ Mi', 10.00, 'Áo sơ mi công sở, casual', TRUE),
(3, 'Quần Jean', 10.00, 'Quần jean nam nữ', TRUE),
(4, 'Quần Tây', 10.00, 'Quần tây công sở', TRUE),
(5, 'Váy', 10.00, 'Váy công sở, dạ hội', TRUE),
(6, 'Áo Khoác', 10.00, 'Áo khoác mùa đông, jacket', TRUE),
(7, 'Phụ Kiện', 8.00, 'Túi xách, ví, thắt lưng', TRUE),
(8, 'Giày Dép', 10.00, 'Giày thể thao, giày cao gót', TRUE),
(9, 'Đồ Thể Thao', 10.00, 'Quần áo thể thao', TRUE),
(10, 'Đồ Ngủ', 8.00, 'Đồ ngủ, đồ mặc nhà', TRUE);

-- ============================================================================
-- 3. SUPPLIERS (Vendors)
-- ============================================================================
INSERT IGNORE INTO Suppliers (Id, Name, ContactName, Phone, Email, Address, TaxCode, IsActive) VALUES
(1, 'Công ty TNHH Thời Trang Việt', 'Nguyễn Văn A', '0901234567', 'contact@thoitrangviet.vn', '123 Nguyễn Huệ, Q.1, TP.HCM', '0123456789', TRUE),
(2, 'Nhà máy May Hà Nội', 'Trần Thị B', '0912345678', 'sales@mayhanoi.vn', '456 Láng Hạ, Đống Đa, Hà Nội', '0987654321', TRUE),
(3, 'Công ty CP Dệt May Việt Tiến', 'Lê Văn C', '0923456789', 'info@viettien.com', '789 Lê Lợi, Q.1, TP.HCM', '0111222333', TRUE),
(4, 'Xưởng May Gia Đình', 'Phạm Thị D', '0934567890', 'giadinh@gmail.com', '321 Trần Hưng Đạo, Q.5, TP.HCM', '0444555666', TRUE),
(5, 'Nhập khẩu Hàn Quốc Fashion', 'Kim Min Soo', '0945678901', 'korea@fashion.kr', '555 Nguyễn Trãi, Q.1, TP.HCM', '0777888999', TRUE);

-- ============================================================================
-- 4. PRODUCTS (Inventory)
-- ============================================================================
INSERT IGNORE INTO Products (Id, Name, Code, CategoryId, SupplierId, SalePrice, PurchasePrice, PurchaseUnit, StockQuantity, ImportQuantity, MinStockLevel, PromoDiscountPercent, Description, IsActive) VALUES
-- Áo Thun (CategoryId: 1)
(1, 'Áo Thun Nam Basic Trắng', 'AT001', 1, 1, 150000, 80000, 'VND', 50, 100, 10, 0, 'Áo thun nam cotton 100%, form regular', TRUE),
(2, 'Áo Thun Nam Basic Đen', 'AT002', 1, 1, 150000, 80000, 'VND', 45, 100, 10, 0, 'Áo thun nam cotton 100%, form regular', TRUE),
(3, 'Áo Thun Nữ Oversize Trắng', 'AT003', 1, 1, 180000, 95000, 'VND', 30, 50, 10, 10, 'Áo thun nữ form rộng, chất liệu cotton', TRUE),
(4, 'Áo Thun Polo Nam', 'AT004', 1, 2, 250000, 130000, 'VND', 25, 50, 10, 0, 'Áo polo nam cao cấp, nhiều màu', TRUE),
(5, 'Áo Thun Graphic Unisex', 'AT005', 1, 1, 200000, 100000, 'VND', 40, 80, 10, 15, 'Áo thun in hình nghệ thuật', TRUE),

-- Áo Sơ Mi (CategoryId: 2)
(6, 'Áo Sơ Mi Nam Trắng Công Sở', 'SM001', 2, 2, 350000, 180000, 'VND', 35, 70, 10, 0, 'Áo sơ mi nam công sở, chất liệu kate', TRUE),
(7, 'Áo Sơ Mi Nam Xanh Navy', 'SM002', 2, 2, 350000, 180000, 'VND', 30, 70, 10, 0, 'Áo sơ mi nam công sở, chống nhăn', TRUE),
(8, 'Áo Sơ Mi Nữ Trắng', 'SM003', 2, 3, 320000, 160000, 'VND', 28, 60, 10, 0, 'Áo sơ mi nữ công sở, form ôm', TRUE),
(9, 'Áo Sơ Mi Kẻ Sọc Nam', 'SM004', 2, 2, 380000, 190000, 'VND', 20, 40, 10, 10, 'Áo sơ mi kẻ sọc, phong cách Hàn Quốc', TRUE),

-- Quần Jean (CategoryId: 3)
(10, 'Quần Jean Nam Slim Fit Đen', 'QJ001', 3, 3, 450000, 220000, 'VND', 40, 80, 15, 0, 'Quần jean nam ôm vừa, màu đen', TRUE),
(11, 'Quần Jean Nam Straight Xanh', 'QJ002', 3, 3, 450000, 220000, 'VND', 38, 80, 15, 0, 'Quần jean nam form suông, màu xanh', TRUE),
(12, 'Quần Jean Nữ Skinny', 'QJ003', 3, 3, 420000, 210000, 'VND', 35, 70, 15, 15, 'Quần jean nữ ôm, co giãn tốt', TRUE),
(13, 'Quần Jean Baggy Unisex', 'QJ004', 3, 4, 480000, 240000, 'VND', 25, 50, 10, 20, 'Quần jean baggy phong cách streetwear', TRUE),

-- Quần Tây (CategoryId: 4)
(14, 'Quần Tây Nam Đen Công Sở', 'QT001', 4, 2, 380000, 190000, 'VND', 30, 60, 10, 0, 'Quần tây nam công sở, chất liệu kate', TRUE),
(15, 'Quần Tây Nam Xám', 'QT002', 4, 2, 380000, 190000, 'VND', 28, 60, 10, 0, 'Quần tây nam màu xám, form slim', TRUE),
(16, 'Quần Tây Nữ Đen', 'QT003', 4, 3, 350000, 175000, 'VND', 25, 50, 10, 0, 'Quần tây nữ công sở, ống đứng', TRUE),

-- Váy (CategoryId: 5)
(17, 'Váy Công Sở Đen', 'V001', 5, 3, 320000, 160000, 'VND', 20, 40, 8, 0, 'Váy công sở dáng chữ A', TRUE),
(18, 'Váy Maxi Hoa', 'V002', 5, 4, 450000, 225000, 'VND', 15, 30, 5, 10, 'Váy maxi họa tiết hoa, dáng dài', TRUE),
(19, 'Váy Dạ Hội Đỏ', 'V003', 5, 5, 850000, 425000, 'VND', 10, 20, 3, 0, 'Váy dạ hội cao cấp, nhập khẩu Hàn Quốc', TRUE),

-- Áo Khoác (CategoryId: 6)
(20, 'Áo Khoác Jean Nam', 'AK001', 6, 3, 550000, 275000, 'VND', 22, 44, 8, 0, 'Áo khoác jean nam, phong cách basic', TRUE),
(21, 'Áo Khoác Hoodie Unisex', 'AK002', 6, 4, 380000, 190000, 'VND', 35, 70, 10, 15, 'Áo hoodie nỉ bông, nhiều màu', TRUE),
(22, 'Áo Khoác Dạ Nữ', 'AK003', 6, 5, 950000, 475000, 'VND', 12, 24, 5, 0, 'Áo khoác dạ nữ cao cấp, mùa đông', TRUE),

-- Phụ Kiện (CategoryId: 7)
(23, 'Túi Xách Nữ Da', 'TX001', 7, 5, 650000, 325000, 'VND', 18, 36, 5, 0, 'Túi xách nữ da PU cao cấp', TRUE),
(24, 'Ví Nam Da Bò', 'VI001', 7, 5, 280000, 140000, 'VND', 25, 50, 8, 0, 'Ví nam da bò thật, nhiều ngăn', TRUE),
(25, 'Thắt Lưng Nam Da', 'TL001', 7, 2, 220000, 110000, 'VND', 30, 60, 10, 0, 'Thắt lưng nam da, khóa tự động', TRUE),
(26, 'Mũ Lưỡi Trai Unisex', 'MU001', 7, 4, 120000, 60000, 'VND', 40, 80, 15, 10, 'Mũ lưỡi trai thêu logo', TRUE),

-- Giày Dép (CategoryId: 8)
(27, 'Giày Thể Thao Nam', 'GT001', 8, 5, 750000, 375000, 'VND', 20, 40, 8, 0, 'Giày thể thao nam, đế êm', TRUE),
(28, 'Giày Cao Gót Nữ 7cm', 'GC001', 8, 5, 580000, 290000, 'VND', 15, 30, 5, 10, 'Giày cao gót nữ, da bóng', TRUE),
(29, 'Dép Sandal Nam', 'DS001', 8, 4, 180000, 90000, 'VND', 35, 70, 10, 0, 'Dép sandal nam, đế cao su', TRUE),

-- Đồ Thể Thao (CategoryId: 9)
(30, 'Bộ Đồ Thể Thao Nam', 'TT001', 9, 4, 420000, 210000, 'VND', 25, 50, 10, 15, 'Bộ đồ thể thao nam, thấm hút mồ hôi', TRUE),
(31, 'Quần Short Thể Thao', 'TT002', 9, 4, 180000, 90000, 'VND', 40, 80, 15, 0, 'Quần short thể thao, nhiều màu', TRUE),

-- Đồ Ngủ (CategoryId: 10)
(32, 'Bộ Đồ Ngủ Nữ Cotton', 'DN001', 10, 4, 250000, 125000, 'VND', 30, 60, 10, 0, 'Bộ đồ ngủ nữ cotton mềm mại', TRUE),
(33, 'Áo Ngủ Nam', 'DN002', 10, 4, 150000, 75000, 'VND', 25, 50, 10, 0, 'Áo ngủ nam thoáng mát', TRUE);

-- ============================================================================
-- 5. CUSTOMERS (Customer Database)
-- ============================================================================
INSERT IGNORE INTO Customers (Id, Name, Phone, Email, Address, CustomerType, Points, TotalSpent, DateOfBirth, Gender, IsActive) VALUES
(1, 'Khách lẻ', '0000000000', NULL, NULL, 'Regular', 0, 0, NULL, NULL, TRUE),
(2, 'Nguyễn Văn An', '0901111111', 'nguyenvanan@gmail.com', '123 Lê Lợi, Q.1, TP.HCM', 'Gold', 6500, 13000000, '1990-05-15', 'Male', TRUE),
(3, 'Trần Thị Bình', '0902222222', 'tranbinhtt@gmail.com', '456 Nguyễn Huệ, Q.1, TP.HCM', 'Platinum', 12000, 24000000, '1985-08-20', 'Female', TRUE),
(4, 'Lê Hoàng Cường', '0903333333', 'cuonglh@yahoo.com', '789 Hai Bà Trưng, Q.3, TP.HCM', 'Silver', 3200, 6400000, '1992-03-10', 'Male', TRUE),
(5, 'Phạm Thị Dung', '0904444444', 'dungpham@outlook.com', '321 Võ Văn Tần, Q.3, TP.HCM', 'Bronze', 800, 1600000, '1995-11-25', 'Female', TRUE),
(6, 'Hoàng Văn Em', '0905555555', 'emhoang@gmail.com', '555 Cách Mạng Tháng 8, Q.10, TP.HCM', 'Regular', 250, 500000, '2000-01-30', 'Male', TRUE),
(7, 'Võ Thị Phương', '0906666666', 'phuongvo@gmail.com', '666 Lý Thường Kiệt, Q.10, TP.HCM', 'Gold', 7800, 15600000, '1988-07-12', 'Female', TRUE),
(8, 'Đặng Văn Giang', '0907777777', 'giangdv@gmail.com', '777 Điện Biên Phủ, Q.Bình Thạnh, TP.HCM', 'Silver', 2500, 5000000, '1993-09-18', 'Male', TRUE),
(9, 'Bùi Thị Hà', '0908888888', 'habt@gmail.com', '888 Xô Viết Nghệ Tĩnh, Q.Bình Thạnh, TP.HCM', 'Bronze', 600, 1200000, '1997-12-05', 'Female', TRUE),
(10, 'Ngô Văn Ích', '0909999999', 'ichngo@gmail.com', '999 Phan Văn Trị, Q.Gò Vấp, TP.HCM', 'Regular', 150, 300000, '1999-04-22', 'Male', TRUE);

-- ============================================================================
-- 6. VOUCHERS (Discount Codes)
-- ============================================================================
INSERT IGNORE INTO Vouchers (Id, Code, Name, DiscountType, DiscountValue, MaxDiscountAmount, MinInvoiceAmount, StartDate, EndDate, UsageLimit, UsageLimitPerCustomer, UsedCount, IsActive, CreatedBy) VALUES
(1, 'WELCOME10', 'Giảm 10% cho khách hàng mới', 'Percentage', 10.00, 100000, 0, '2024-01-01 00:00:00', '2024-12-31 23:59:59', 0, 1, 45, TRUE, 1),
(2, 'SUMMER50K', 'Giảm 50K cho đơn từ 500K', 'FixedAmount', 50000, NULL, 500000, '2024-06-01 00:00:00', '2024-08-31 23:59:59', 100, 2, 67, TRUE, 1),
(3, 'VIP20', 'Giảm 20% cho khách VIP', 'Percentage', 20.00, 200000, 1000000, '2024-01-01 00:00:00', '2024-12-31 23:59:59', 0, 5, 23, TRUE, 1),
(4, 'FLASH100K', 'Flash Sale giảm 100K', 'FixedAmount', 100000, NULL, 1000000, '2024-07-01 00:00:00', '2024-07-31 23:59:59', 50, 1, 48, TRUE, 2),
(5, 'NEWYEAR15', 'Tết giảm 15%', 'Percentage', 15.00, 150000, 500000, '2024-01-20 00:00:00', '2024-02-15 23:59:59', 200, 3, 156, TRUE, 1),
(6, 'FREESHIP', 'Miễn phí vận chuyển', 'FixedAmount', 30000, NULL, 200000, '2024-01-01 00:00:00', '2024-12-31 23:59:59', 0, 10, 234, TRUE, 1);

-- ============================================================================
-- 7. INVOICES (Sample Transactions)
-- ============================================================================
INSERT IGNORE INTO Invoices (Id, InvoiceNumber, CustomerId, EmployeeId, VoucherId, Subtotal, TaxPercent, TaxAmount, DiscountAmount, VoucherDiscount, TierDiscount, Total, Paid, PaymentMethod, PaymentStatus, ChangeAmount, Status, CreatedDate, CompletedDate) VALUES
(1, 'INV-2024-0001', 1, 3, NULL, 300000, 10.00, 30000, 0, 0, 0, 330000, 500000, 'Cash', 'Paid', 170000, 'Completed', '2024-01-15 10:30:00', '2024-01-15 10:35:00'),
(2, 'INV-2024-0002', 2, 3, 3, 1500000, 10.00, 150000, 200000, 200000, 0, 1450000, 1450000, 'Card', 'Paid', 0, 'Completed', '2024-01-20 14:20:00', '2024-01-20 14:25:00'),
(3, 'INV-2024-0003', 3, 4, 5, 2000000, 10.00, 200000, 150000, 150000, 0, 2050000, 2050000, 'Transfer', 'Paid', 0, 'Completed', '2024-02-01 11:15:00', '2024-02-01 11:20:00'),
(4, 'INV-2024-0004', 5, 3, 1, 200000, 10.00, 20000, 20000, 20000, 0, 200000, 200000, 'Cash', 'Paid', 0, 'Completed', '2024-02-05 09:45:00', '2024-02-05 09:50:00'),
(5, 'INV-2024-0005', 6, 4, NULL, 850000, 10.00, 85000, 0, 0, 0, 935000, 1000000, 'QR', 'Paid', 65000, 'Completed', '2024-02-10 16:30:00', '2024-02-10 16:35:00'),
(6, 'INV-2024-0006', 4, 3, 2, 650000, 10.00, 65000, 50000, 50000, 0, 665000, 700000, 'Cash', 'Paid', 35000, 'Completed', '2024-02-14 13:20:00', '2024-02-14 13:25:00'),
(7, 'INV-2024-0007', 7, 5, NULL, 450000, 10.00, 45000, 0, 0, 0, 495000, 500000, 'Card', 'Paid', 5000, 'Completed', '2024-02-20 10:00:00', '2024-02-20 10:05:00'),
(8, 'INV-2024-0008', 8, 3, 4, 1200000, 10.00, 120000, 100000, 100000, 0, 1220000, 1220000, 'Transfer', 'Paid', 0, 'Completed', '2024-03-01 15:45:00', '2024-03-01 15:50:00');

-- ============================================================================
-- 8. INVOICE ITEMS (Transaction Details)
-- ============================================================================
INSERT IGNORE INTO InvoiceItems (InvoiceId, ProductId, EmployeeId, ProductName, ProductCode, UnitPrice, Quantity, DiscountPercent, DiscountAmount, LineTotal) VALUES
-- Invoice 1 items
(1, 1, 3, 'Áo Thun Nam Basic Trắng', 'AT001', 150000, 2, 0, 0, 300000),

-- Invoice 2 items (VIP customer)
(2, 6, 3, 'Áo Sơ Mi Nam Trắng Công Sở', 'SM001', 350000, 2, 0, 0, 700000),
(2, 10, 3, 'Quần Jean Nam Slim Fit Đen', 'QJ001', 450000, 1, 0, 0, 450000),
(2, 14, 3, 'Quần Tây Nam Đen Công Sở', 'QT001', 380000, 1, 0, 0, 380000),

-- Invoice 3 items (Platinum)
(3, 19, 4, 'Váy Dạ Hội Đỏ', 'V003', 850000, 1, 0, 0, 850000),
(3, 23, 4, 'Túi Xách Nữ Da', 'TX001', 650000, 1, 0, 0, 650000),
(3, 28, 4, 'Giày Cao Gót Nữ 7cm', 'GC001', 580000, 1, 10, 58000, 522000),

-- Invoice 4 items
(4, 3, 3, 'Áo Thun Nữ Oversize Trắng', 'AT003', 180000, 1, 10, 18000, 162000),

-- Invoice 5 items
(5, 20, 4, 'Áo Khoác Jean Nam', 'AK001', 550000, 1, 0, 0, 550000),
(5, 24, 4, 'Ví Nam Da Bò', 'VI001', 280000, 1, 0, 0, 280000),

-- Invoice 6 items
(6, 11, 3, 'Quần Jean Nam Straight Xanh', 'QJ002', 450000, 1, 0, 0, 450000),
(6, 5, 3, 'Áo Thun Graphic Unisex', 'AT005', 200000, 1, 15, 30000, 170000),

-- Invoice 7 items
(7, 21, 5, 'Áo Khoác Hoodie Unisex', 'AK002', 380000, 1, 15, 57000, 323000),
(7, 26, 5, 'Mũ Lưỡi Trai Unisex', 'MU001', 120000, 1, 10, 12000, 108000),

-- Invoice 8 items
(8, 13, 3, 'Quần Jean Baggy Unisex', 'QJ004', 480000, 2, 20, 192000, 768000),
(8, 30, 3, 'Bộ Đồ Thể Thao Nam', 'TT001', 420000, 1, 15, 63000, 357000);

-- ============================================================================
-- 9. STOCK MOVEMENTS (Inventory History) - Optional
-- ============================================================================
INSERT IGNORE INTO StockMovements (ProductId, MovementType, Quantity, PreviousStock, NewStock, ReferenceType, ReferenceId, Notes, EmployeeId, CreatedDate) VALUES
-- Initial imports
(1, 'Import', 100, 0, 100, 'PurchaseOrder', 1, 'Nhập hàng đầu tiên', 1, '2024-01-01 08:00:00'),
(2, 'Import', 100, 0, 100, 'PurchaseOrder', 1, 'Nhập hàng đầu tiên', 1, '2024-01-01 08:00:00'),
(3, 'Import', 50, 0, 50, 'PurchaseOrder', 2, 'Nhập hàng đầu tiên', 1, '2024-01-01 08:00:00'),

-- Sales from invoices
(1, 'Sale', -2, 100, 98, 'Invoice', 1, 'Bán qua hóa đơn INV-2024-0001', 3, '2024-01-15 10:30:00'),
(6, 'Sale', -2, 70, 68, 'Invoice', 2, 'Bán qua hóa đơn INV-2024-0002', 3, '2024-01-20 14:20:00'),
(10, 'Sale', -1, 80, 79, 'Invoice', 2, 'Bán qua hóa đơn INV-2024-0002', 3, '2024-01-20 14:20:00'),

-- Stock adjustments
(1, 'Adjustment', -2, 98, 96, NULL, NULL, 'Điều chỉnh do kiểm kê', 2, '2024-02-01 09:00:00'),
(5, 'Return', 1, 40, 41, 'Invoice', 4, 'Khách trả hàng', 3, '2024-02-06 10:00:00');

-- ============================================================================
-- 10. CUSTOMER VOUCHER USAGE (Tracking) - Optional
-- ============================================================================
INSERT IGNORE INTO CustomerVoucherUsage (CustomerId, VoucherId, InvoiceId, DiscountAmount, UsedDate) VALUES
(2, 3, 2, 200000, '2024-01-20 14:20:00'),
(3, 5, 3, 150000, '2024-02-01 11:15:00'),
(5, 1, 4, 20000, '2024-02-05 09:45:00'),
(4, 2, 6, 50000, '2024-02-14 13:20:00'),
(8, 4, 8, 100000, '2024-03-01 15:45:00');

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- Check total records
SELECT 'Accounts' as TableName, COUNT(*) as RecordCount FROM Accounts
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'Suppliers', COUNT(*) FROM Suppliers
UNION ALL
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'Customers', COUNT(*) FROM Customers
UNION ALL
SELECT 'Vouchers', COUNT(*) FROM Vouchers
UNION ALL
SELECT 'Invoices', COUNT(*) FROM Invoices
UNION ALL
SELECT 'InvoiceItems', COUNT(*) FROM InvoiceItems
UNION ALL
SELECT 'StockMovements', COUNT(*) FROM StockMovements
UNION ALL
SELECT 'CustomerVoucherUsage', COUNT(*) FROM CustomerVoucherUsage;

-- ============================================================================
-- SUMMARY
-- ============================================================================
/*
Sample Data Summary:
- 5 Accounts (1 Admin, 1 Manager, 3 Cashiers)
- 10 Categories (Various product types)
- 5 Suppliers (Vietnamese and Korean suppliers)
- 33 Products (Clothing, accessories, shoes)
- 10 Customers (Including walk-in customer)
- 6 Vouchers (Various discount types)
- 8 Invoices (Sample transactions)
- 17 Invoice Items (Transaction details)
- 8 Stock Movements (Inventory tracking)
- 5 Customer Voucher Usage records

Total Sample Revenue: ~8,365,000 VND
Average Invoice Value: ~1,045,625 VND

IMPORT INSTRUCTIONS:
1. First import database_schema.sql
2. Then import this file (sample_data.sql)
3. Run the verification query above to confirm all data was inserted
4. Login to the application with username: admin, password: admin
*/

-- ============================================================================
-- END OF SAMPLE DATA
-- ============================================================================
