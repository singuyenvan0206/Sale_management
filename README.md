# Hệ Thống Quản Lý Bán Hàng (POS Management System)

## Mô tả
Hệ thống quản lý bán hàng (Point of Sale - POS) được phát triển bằng WPF (.NET 8.0) với giao diện hoàn toàn bằng tiếng Việt. Hệ thống hỗ trợ quản lý sản phẩm, khách hàng, hóa đơn và báo cáo với phân quyền theo vai trò.

## Tính năng chính

### 🔐 Quản lý người dùng và phân quyền
- **Admin**: Toàn quyền quản lý hệ thống, bao gồm quản lý người dùng
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
- **Dashboard phân tích dữ liệu tương tác** với 6 loại biểu đồ:
  - 📈 Doanh thu theo tháng (12 tháng gần nhất)
  - 📊 Top 10 sản phẩm bán chạy
  - 📉 Xu hướng khách hàng mới
  - 🎯 Doanh thu theo danh mục
  - 📅 Doanh thu theo ngày (30 ngày gần nhất)
  - 💰 Thống kê tổng quan KPI
- Xuất dữ liệu CSV

## Công nghệ sử dụng

- **Frontend**: WPF (.NET 8.0)
- **Database**: MySQL
- **Architecture**: Layered Architecture (UI, Business Logic, Data Access, Data)
- **Pattern**: MVVM
- **Charts**: OxyPlot.Wpf (với biểu đồ tương tác và dữ liệu thực)
- **Data Visualization**: Biểu đồ đường, cột, tròn với dữ liệu real-time

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
├── DashboardWindow.xaml            # Bảng điều khiển Admin với biểu đồ phân tích
├── CashierDashboardWindow.xaml     # Bảng điều khiển Cashier
├── ProductManagementWindow.xaml    # Quản lý sản phẩm
├── CustomerManagementWindow.xaml   # Quản lý khách hàng
├── InvoiceManagementWindow.xaml    # Quản lý hóa đơn
├── InvoicePrintWindow.xaml         # In hóa đơn
├── ReportsWindow.xaml              # Báo cáo và thống kê
├── UserManagementWindow.xaml       # Quản lý người dùng (Admin)
├── SettingsWindow.xaml             # Cài đặt hệ thống
├── DatabaseHelper.cs               # Xử lý database với các phương thức phân tích dữ liệu
└── SettingsManager.cs              # Quản lý cài đặt
```

## Hướng dẫn sử dụng

### Đăng nhập
- Username: admin
- Password: admin (Admin)
- Username: cashier
- Password: cashier (Cashier)

### Tạo hóa đơn
1. Chọn khách hàng
2. Thêm sản phẩm vào hóa đơn
3. Điều chỉnh số lượng và giá
4. Tính thuế và giảm giá
5. Lưu hóa đơn

### Sử dụng Dashboard phân tích dữ liệu
1. Đăng nhập với quyền Admin
2. Vào Dashboard và click **"📊 Biểu Đồ và Phân Tích"**
3. Chọn loại phân tích từ menu bên trái:
   - **📈 Doanh Thu Theo Tháng**: Biểu đồ đường doanh thu 12 tháng gần nhất
   - **📊 Sản Phẩm Bán Chạy**: Biểu đồ cột top 10 sản phẩm bán chạy
   - **📉 Xu Hướng Khách Hàng**: Biểu đồ đường khách hàng mới theo tháng
   - **🎯 Doanh Thu Theo Danh Mục**: Biểu đồ tròn phân bổ doanh thu
   - **📅 Doanh Thu Theo Ngày**: Biểu đồ đường doanh thu 30 ngày gần nhất
   - **💰 Thống Kê Tổng Quan**: Dashboard KPI tổng hợp
4. Xem biểu đồ tương ứng hiển thị ở bên phải với dữ liệu thực từ database

## Tính năng nổi bật

### 🆕 Dashboard phân tích dữ liệu tương tác
- **Giao diện 2 cột**: Menu lựa chọn bên trái, khu vực hiển thị biểu đồ bên phải
- **Dữ liệu thực**: Tất cả biểu đồ sử dụng dữ liệu thực từ database
- **Xử lý lỗi thông minh**: Hiển thị thông báo phù hợp khi không có dữ liệu
- **Cập nhật real-time**: Dữ liệu được lấy mới mỗi lần mở biểu đồ

## Troubleshooting

### Lỗi thường gặp

#### 1. Lỗi kết nối database
- **Triệu chứng**: Ứng dụng không thể kết nối MySQL
- **Giải pháp**: Kiểm tra cài đặt kết nối trong Settings, đảm bảo MySQL đang chạy

#### 2. Biểu đồ không hiển thị dữ liệu
- **Triệu chứng**: Biểu đồ hiển thị "Chưa có dữ liệu"
- **Giải pháp**: Tạo một số hóa đơn bán hàng để có dữ liệu hiển thị

#### 3. Ứng dụng crash khi mở biểu đồ
- **Triệu chứng**: Ứng dụng đóng đột ngột
- **Giải pháp**: Kiểm tra log lỗi, đảm bảo database có dữ liệu hợp lệ

### Hỗ trợ kỹ thuật
- Kiểm tra file log trong thư mục Debug
- Đảm bảo .NET 8.0 Runtime được cài đặt
- Kiểm tra quyền truy cập database

## Đóng góp

Mọi đóng góp đều được chào đón! Vui lòng tạo issue hoặc pull request.

