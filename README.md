# 👗 FashionStore ERP - Hệ thống quản lý bán hàng thời trang toàn diện

![Banner](https://img.shields.io/badge/FashionStore-ERP-blue?style=for-the-badge&logo=dotnet)
![Status](https://img.shields.io/badge/Status-Development-green?style=for-the-badge)
![Security](https://img.shields.io/badge/Security-Hardened-success?style=for-the-badge)

**FashionStore ERP** là một giải pháp quản trị doanh nghiệp bán lẻ hiện đại, được thiết kế chuyên biệt cho ngành thời trang. Hệ thống kết hợp sức mạnh của ứng dụng Desktop (WPF) để quản lý kho/vận hành và ứng dụng Web (ASP.NET Core) để bán hàng linh hoạt (POS).

---

## ✨ Tính năng nổi bật

### 🛒 Điểm bán hàng (Web POS)
- **Thanh toán QR tự động**: Tích hợp **SePay API** để tạo mã VietQR động và tự động kiểm tra trạng thái thanh toán.
- **Quản lý giỏ hàng thông minh**: Tìm kiếm sản phẩm nhanh, quét mã vạch (Barcode/QR) trực tiếp qua Camera.
- **Tiền thưởng & Ưu đãi**: Tự động áp dụng giảm giá theo phân hạng khách hàng (Loyalty) và mã Voucher.

### 📦 Quản trị Kho & Sản phẩm
- **Sản phẩm đa biến thể**: Quản lý màu sắc, kích thước, số lượng tồn kho theo từng biến thể.
- **Lịch sử nhập xuất**: Theo dõi di biến động hàng hóa chi tiết.
- **Quản lý nhà cung cấp**: Tối ưu hóa chuỗi cung ứng.

### 👥 Khách hàng & Marketing
- **Hệ thống phân hạng**: Tự động nâng hạng khách hàng (Regular, Silver, Gold, VIP) dựa trên doanh số.
- **Chiến dịch Khuyến mãi**: Thiết lập các chương trình giảm giá, mua X tặng Y linh hoạt.

### 🛡️ Bảo mật chuẩn Enterprise
- **Mã hóa dữ liệu**: Sử dụng AES-256 để bảo vệ các Token API nhạy cảm.
- **An toàn mật khẩu**: Băm mật khẩu bằng thuật toán **BCrypt**.
- **Chống tấn công SQL**: 100% sử dụng Parameterized Queries với Dapper.
- **Phòng thủ Web**: Tích hợp Anti-Forgery Token (CSRF), HSTS, và chính sách Cookie nghiêm ngặt.

---

## 🛠️ Công nghệ sử dụng

| Lớp (Layer) | Công nghệ |
| :--- | :--- |
| **Backend** | .NET 9.0 / 10.0, ASP.NET Core MVC |
| **Desktop App** | WPF (Windows Presentation Foundation) |
| **Database** | MySQL |
| **ORM** | Dapper (High Performance) |
| **UI/UX** | Vanilla CSS, FontAwesome, JavaScript (ES6+) |
| **Security** | BCrypt.Net, System.Security.Cryptography |

---

## 📂 Cấu trúc thư mục

```text
FashionStore/
├── src/
│   ├── FashionStore.Core/       # Chứa Model, Interface và Logic dùng chung
│   ├── FashionStore.Data/       # Tầng truy xuất dữ liệu (Repositories)
│   ├── FashionStore.Services/   # Tầng nghiệp vụ (Business Logic)
│   ├── FashionStore.Web/        # Ứng dụng Web (POS, Dashboard)
│   └── FashionStore.App/        # Ứng dụng Desktop (WPF Management)
├── FashionStore.Tests/          # Các bài kiểm tra unit test
└── main.sql                     # Script khởi tạo cơ sở dữ liệu
```

---

## 🚀 Hướng dẫn cài đặt

### 1. Yêu cầu hệ thống
- .NET SDK 9.0 trở lên.
- MySQL Server 8.0+.
- Visual Studio 2022 hoặc VS Code.

### 2. Cấu hình Cơ sở dữ liệu
1. Tạo một database mới trong MySQL.
2. Chạy file `main.sql` để khởi tạo cấu trúc bảng.
3. Cập nhật chuỗi kết nối (Connection String) trong `appsettings.json` của dự án Web và App.

### 3. Chạy ứng dụng
- **Web App**: 
  ```bash
  dotnet run --project src/FashionStore.Web/FashionStore.Web.csproj
  ```
- **WPF App**:
  Mở giải pháp (`FashionStore.sln`) bằng Visual Studio và chạy dự án `FashionStore.App`.

---

## 📝 Ghi chú bảo mật
Hệ thống yêu cầu cấu hình **SePay API Token** để tính năng thanh toán QR hoạt động. Token này sẽ được mã hóa an toàn khi lưu vào cơ sở dữ liệu. Vui lòng không chia sẻ file cấu hình hoặc database chứa dữ liệu nhạy cảm.

---
*© 2026 FashionStore ERP Team. Built with ❤️ for the Fashion Industry.*
