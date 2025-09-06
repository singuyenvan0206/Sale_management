# Hệ Thống Quản Lý Bán Hàng (POS Management System)

## Mô tả
Hệ thống quản lý bán hàng (Point of Sale - POS) được phát triển bằng WPF (.NET 8.0) với giao diện hoàn toàn bằng tiếng Việt. Hệ thống hỗ trợ quản lý sản phẩm, khách hàng, hóa đơn và báo cáo với phân quyền theo vai trò.

## Tính năng chính

### 🔐 Quản lý người dùng và phân quyền
- **Admin**: Toàn quyền quản lý hệ thống, bao gồm quản lý người dùng
- **Manager**: Quản lý sản phẩm, khách hàng, hóa đơn và xem báo cáo
- **Cashier**: Tạo hóa đơn, quản lý khách hàng cơ bản

### 📦 Quản lý sản phẩm
- Thêm, sửa, xóa sản phẩm
- Quản lý danh mục sản phẩm
- Theo dõi tồn kho
- Tìm kiếm sản phẩm nhanh

### 👥 Quản lý khách hàng
- Thông tin khách hàng đầy đủ
- Phân loại khách hàng (Thường, VIP, Sỉ, Doanh nghiệp)
- Lịch sử mua hàng

### 🧾 Quản lý hóa đơn
- Tạo hóa đơn bán hàng
- Tính toán thuế và giảm giá
- In hóa đơn chuyên nghiệp
- Lưu trữ lịch sử giao dịch

### 📊 Báo cáo và thống kê
- Báo cáo doanh thu theo ngày/tháng
- Thống kê sản phẩm bán chạy
- Xuất dữ liệu CSV
- Biểu đồ trực quan

## Công nghệ sử dụng

- **Frontend**: WPF (.NET 8.0)
- **Database**: MySQL
- **Architecture**: Layered Architecture (UI, Business Logic, Data Access, Data)
- **Pattern**: MVVM
- **Charts**: OxyPlot.Wpf

## Cài đặt và chạy

### Yêu cầu hệ thống
- Windows 10/11
- .NET 8.0 Runtime
- MySQL Server

### Cài đặt
1. Clone repository
2. Mở solution trong Visual Studio 2022
3. Cấu hình kết nối database trong Settings
4. Build và chạy ứng dụng

### Cấu hình database
- Mở Settings trong ứng dụng
- Nhập thông tin kết nối MySQL:
  - Server: localhost (hoặc IP server)
  - Database: pos_management
  - Username: root
  - Password: [mật khẩu MySQL]

## Dữ liệu mặc định

Hệ thống tự động tạo dữ liệu mẫu tiếng Việt bao gồm:

### Danh mục sản phẩm
- Thực phẩm
- Đồ uống
- Điện tử
- Quần áo
- Gia dụng
- Sách vở
- Thể thao
- Mỹ phẩm
- Đồ chơi
- Khác

### Sản phẩm mẫu
- Cơm tấm sườn nướng, Phở bò, Bánh mì thịt nướng
- Coca Cola, Nước suối, Trà sữa
- iPhone 15, Samsung Galaxy S24, Laptop Dell
- Áo thun nam, Quần jean nữ, Giày thể thao
- Và nhiều sản phẩm khác...

### Khách hàng mẫu
- Khách lẻ
- Nguyễn Văn An (VIP)
- Trần Thị Bình (Thường)
- Lê Văn Cường (Sỉ)
- Phạm Thị Dung (Doanh nghiệp)

## Cấu trúc dự án

```
WpfApp1/
├── MainWindow.xaml                 # Cửa sổ đăng nhập
├── DashboardWindow.xaml            # Bảng điều khiển Admin/Manager
├── CashierDashboardWindow.xaml     # Bảng điều khiển Cashier
├── ProductManagementWindow.xaml    # Quản lý sản phẩm
├── CustomerManagementWindow.xaml   # Quản lý khách hàng
├── InvoiceManagementWindow.xaml    # Quản lý hóa đơn
├── InvoicePrintWindow.xaml         # In hóa đơn
├── ReportsWindow.xaml              # Báo cáo và thống kê
├── UserManagementWindow.xaml       # Quản lý người dùng (Admin)
├── SettingsWindow.xaml             # Cài đặt hệ thống
├── DatabaseHelper.cs               # Xử lý database
└── SettingsManager.cs              # Quản lý cài đặt
```

## Hướng dẫn sử dụng

### Đăng nhập
- Username: admin
- Password: admin (Admin)
- Username: manager  
- Password: manager (Manager)
- Username: cashier
- Password: cashier (Cashier)

### Tạo hóa đơn
1. Chọn khách hàng
2. Thêm sản phẩm vào hóa đơn
3. Điều chỉnh số lượng và giá
4. Tính thuế và giảm giá
5. Lưu hóa đơn

### Xem báo cáo
1. Chọn khoảng thời gian
2. Lọc theo khách hàng (tùy chọn)
3. Xem biểu đồ doanh thu
4. Xuất dữ liệu CSV

## Đóng góp

Mọi đóng góp đều được chào đón! Vui lòng tạo issue hoặc pull request.

## License

MIT License