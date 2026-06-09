# 🛒 ShopManager ERP - Hệ thống quản lý bán hàng bán lẻ toàn diện

![Banner](https://img.shields.io/badge/ShopManager-ERP-blue?style=for-the-badge&logo=dotnet)
![Status](https://img.shields.io/badge/Status-Development-green?style=for-the-badge)
![Security](https://img.shields.io/badge/Security-Hardened-success?style=for-the-badge)

**ShopManager ERP** là một giải pháp quản trị doanh nghiệp bán lẻ hiện đại, được thiết kế cho các cửa hàng bán lẻ (Retail Stores). Hệ thống kết hợp sức mạnh của ứng dụng Desktop (WPF) để quản lý kho/vận hành và ứng dụng Web (ASP.NET Core) để bán hàng linh hoạt (POS) và giám sát báo cáo thời gian thực.

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

### ⚡ Đồng bộ thời gian thực (Real-time Synchronization)
- Tích hợp **ASP.NET Core SignalR** kết nối giữa ứng dụng WPF Desktop và trang quản trị Web.
- Giao dịch thực hiện trên ứng dụng WPF lập tức cập nhật chỉ số doanh thu và bảng hóa đơn trên Dashboard Web mà không cần tải lại trang.

### 🛡️ Bảo mật chuẩn Enterprise
- **Mã hóa dữ liệu**: Sử dụng AES-256 để bảo vệ các Token API nhạy cảm.
- **An toàn mật khẩu**: Băm mật khẩu bằng thuật toán **BCrypt**.
- **Chống tấn công SQL**: 100% sử dụng Parameterized Queries với Dapper.
- **Phòng thủ Web**: Tích hợp Anti-Forgery Token (CSRF), HSTS, và chính sách Cookie nghiêm ngặt.

---

## 🛠️ Công nghệ sử dụng

| Lớp (Layer) | Công nghệ |
| :--- | :--- |
| **Backend API / Web** | .NET 10.0, ASP.NET Core MVC, SignalR Hub |
| **Desktop App** | WPF (Windows Presentation Foundation), SignalR Client |
| **Database** | MySQL / MariaDB |
| **ORM** | Dapper (High Performance) |
| **UI/UX** | Vanilla CSS (Dark Mode/Harmonious Palette), JS (ES6+), FontAwesome |
| **Security** | BCrypt.Net, System.Security.Cryptography |

---

## 📂 Cấu trúc thư mục

```text
ShopManager/
├── src/
│   ├── ShopManager.Core/       # Chứa Model, Interface và Logic dùng chung
│   ├── ShopManager.Data/       # Tầng truy xuất dữ liệu (Repositories)
│   ├── ShopManager.Services/   # Tầng nghiệp vụ (Business Logic)
│   ├── ShopManager.Web/        # Ứng dụng Web (POS, Dashboard)
│   └── ShopManager.App/        # Ứng dụng Desktop (WPF Management)
├── ShopManager.Tests/          # Các bài kiểm tra unit test
├── ShopManager_Setup.iss        # Kịch bản Inno Setup đóng gói ứng dụng Desktop
├── ShopManager_Web_Setup.iss    # Kịch bản Inno Setup đóng gói máy chủ Web
└── main.sql                     # Script khởi tạo cơ sở dữ liệu
```

---

## 🚀 Hướng dẫn chạy môi trường Phát triển (Development)

### 1. Yêu cầu hệ thống
- .NET SDK 9.0 & 10.0.
- MySQL / MariaDB Server 8.0+.
- Visual Studio 2022 hoặc VS Code.

### 2. Cấu hình Cơ sở dữ liệu
1. Tạo một database mới trong MySQL/MariaDB.
2. Chạy file `main.sql` để khởi tạo cấu trúc bảng.
3. Cập nhật chuỗi kết nối (Connection String) trong `appsettings.json` của dự án Web và App.

### 3. Chạy ứng dụng
- **Web App**: 
  ```bash
  dotnet run --project src/ShopManager.Web/ShopManager.Web.csproj
  ```
- **WPF App**:
  Mở giải pháp (`ShopManager.sln`) bằng Visual Studio hoặc chạy lệnh:
  ```bash
  dotnet run --project src/ShopManager.App/ShopManager.App.csproj
  ```

---

## 📦 Hướng dẫn đóng gói ứng dụng (Deployment Packaging)

Để cài đặt ứng dụng trên một **máy tính mới hoàn toàn** (không có sẵn .NET SDK, MySQL hay các công cụ lập trình), chúng tôi sử dụng công cụ **Inno Setup** để đóng gói toàn bộ ứng dụng và Database vào một file Setup duy nhất.

### Bước 1: Publish ứng dụng tự chứa (Self-contained)
Chạy các lệnh sau để xuất bản ứng dụng kèm theo toàn bộ thư viện .NET Runtime cần thiết:

* **Đối với WPF Desktop App:**
  ```bash
  dotnet publish src/ShopManager.App/ShopManager.App.csproj -c Release -r win-x64 --self-contained true
  ```
* **Đối với Web App:**
  ```bash
  dotnet publish src/ShopManager.Web/ShopManager.Web.csproj -c Release -r win-x64 --self-contained true
  ```

### Bước 2: Chuẩn bị bộ cài đặt Database
1. Tải về file cài đặt ngầm MariaDB dạng MSI: `mariadb-10.11.8-winx64.msi`.
2. Đặt file này ở thư mục gốc của dự án (cùng cấp với các file `.iss`).

### Bước 3: Biên dịch file cài đặt bằng Inno Setup
1. Mở phần mềm **Inno Setup Compiler**.
2. Mở file [ShopManager_Setup.iss](file:///c:/Users/Simsimi/OneDrive/M%C3%A1y%20t%C3%ADnh/Fashion_store/ShopManager_Setup.iss) (để đóng gói Desktop) hoặc [ShopManager_Web_Setup.iss](file:///c:/Users/Simsimi/OneDrive/M%C3%A1y%20t%C3%ADnh/Fashion_store/ShopManager_Web_Setup.iss) (để đóng gói Web Server).
3. Nhấn **Compile** (phím `F9`).
4. File cài đặt đầu ra sẽ nằm tại thư mục `setup_output/` (`ShopManager_Setup.exe` hoặc `ShopManager_Web_Setup.exe`).

> [!NOTE]
> Khi người dùng chạy file Setup này, trình cài đặt sẽ tự động cài đặt ngầm (Silent Install) MariaDB, khởi tạo dịch vụ cơ sở dữ liệu trên cổng `3306` với mật khẩu mặc định `02062003` và copy toàn bộ file ứng dụng vào ổ đĩa.

---

## 📱 Hướng dẫn kết nối bằng Điện thoại Di động (Mobile/External Access)

Để truy cập Dashboard Web hoặc POS trên điện thoại di động khi đang chạy Server nội bộ:

### 1. Kết nối qua cùng mạng Wi-Fi (LAN)
1. **Cấu hình Web Server:** Trong file [launchSettings.json](file:///c:/Users/Simsimi/OneDrive/M%C3%A1y%20t%C3%ADnh/Fashion_store/src/ShopManager.Web/Properties/launchSettings.json), cấu hình `"applicationUrl": "http://0.0.0.0:5000"`.
2. **Lấy IP nội bộ của máy chủ:** Mở cmd và gõ `ipconfig`. Ghi lại địa chỉ IPv4 (ví dụ `192.168.1.15`).
3. **Mở Firewall:** Đảm bảo Windows Firewall cho phép kết nối cổng `5000`.
4. **Truy cập:** Trên trình duyệt điện thoại, truy cập `http://192.168.1.15:5000`.

### 2. Kết nối khi khác mạng (Điện thoại dùng 4G) - Dành cho báo cáo Đồ án
Để hội đồng hoặc người dùng ngoài Internet có thể truy cập mà không cần deploy lên server cloud tốn phí:
1. Tải và cài đặt **Ngrok** trên máy chủ.
2. Chạy lệnh:
   ```bash
   ngrok http 5000
   ```
3. Copy đường link công khai dạng `https://xxx.ngrok-free.app` được Ngrok cấp.
4. Mở trình duyệt điện thoại và truy cập link trên.

---

## 📝 Ghi chú bảo mật
Hệ thống yêu cầu cấu hình **SePay API Token** để tính năng thanh toán QR hoạt động. Token này sẽ được mã hóa an toàn khi lưu vào cơ sở dữ liệu. Vui lòng không chia sẻ file cấu hình hoặc database chứa dữ liệu nhạy cảm.

---
*© 2026 ShopManager ERP Team. Built with ❤️ for the Retail Industry.*
