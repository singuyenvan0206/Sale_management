# Fashion Store – Sale Management System

Ứng dụng quản lý bán hàng thời trang xây dựng bằng **WPF** trên **.NET 9**, theo mô hình **MVVM**.  
Hỗ trợ quản lý người dùng, sản phẩm, khách hàng, hoá đơn, voucher, báo cáo và tuỳ chỉnh cài đặt hệ thống.

---

## ✨ Tính năng chính

| Nhóm | Tính năng |
|------|-----------|
| **Người dùng** | Đăng nhập, phân quyền (Admin / Staff), quản lý tài khoản, đổi mật khẩu |
| **Sản phẩm** | CRUD sản phẩm & danh mục, quản lý nhà cung cấp, theo dõi xuất nhập kho |
| **Khách hàng** | Quản lý thông tin khách hàng, phân hạng tier |
| **Hoá đơn** | Lập hoá đơn, in hoá đơn, lịch sử giao dịch |
| **Voucher** | Tạo & quản lý mã giảm giá, áp dụng khi thanh toán |
| **Báo cáo** | Dashboard KPI, biểu đồ doanh thu (OxyPlot), tuỳ chỉnh mẫu báo cáo |
| **Cài đặt** | Mức giá/tier, phương thức thanh toán, mã QR, đa ngôn ngữ (🇻🇳 / 🇬🇧) |

---

## 📋 Yêu cầu hệ thống

- **OS:** Windows 10 / 11
- **.NET SDK:** 9.0 trở lên
- **IDE:** Visual Studio 2022 (khuyến nghị) với workload **.NET Desktop Development**
- **Database:** MySQL 8.0+ (qua XAMPP/WAMP hoặc standalone)

---

## 📁 Cấu trúc dự án

```
Fashion_store/
├── FashionStore.sln                  # Solution chính
├── database_schema.sql               # Script tạo database
├── README.md
│
├── WpfApp1/                          # Ứng dụng WPF chính (FashionStore)
│   ├── FashionStore.csproj
│   ├── App.xaml / App.xaml.cs        # Entry point, cấu hình DI & ngôn ngữ
│   ├── MainWindow.xaml               # Shell chính
│   │
│   ├── Models/                       # Data models
│   │   ├── Account.cs
│   │   ├── Product.cs
│   │   ├── Supplier.cs
│   │   ├── StockMovement.cs
│   │   ├── Voucher.cs
│   │   ├── TierSettings.cs
│   │   └── UserRole.cs
│   │
│   ├── Views/                        # XAML views (giao diện)
│   │   ├── DashboardWindow.xaml
│   │   ├── ProductManagementWindow.xaml
│   │   ├── CustomerManagementWindow.xaml
│   │   ├── InvoiceManagementWindow.xaml
│   │   ├── InvoicePrintWindow.xaml
│   │   ├── VoucherManagementWindow.xaml
│   │   ├── SupplierManagementWindow.xaml
│   │   ├── CategoryManagementWindow.xaml
│   │   ├── ReportsWindow.xaml
│   │   ├── ReportsSettingsWindow.xaml
│   │   ├── SettingsWindow.xaml
│   │   ├── UserManagementWindow.xaml
│   │   ├── AddEditUserWindow.xaml
│   │   ├── ChangePasswordWindow.xaml
│   │   ├── TierSettingsWindow.xaml
│   │   ├── TransactionHistoryWindow.xaml
│   │   └── CodeBehind/               # Code-behind cho các views
│   │
│   ├── ViewModels/                   # MVVM ViewModels
│   │   ├── MainViewModel.cs
│   │   ├── DashboardViewModel.cs
│   │   ├── ProductManagementViewModel.cs
│   │   ├── CustomerManagementViewModel.cs
│   │   ├── InvoiceManagementViewModel.cs
│   │   ├── VoucherManagementViewModel.cs
│   │   ├── SupplierManagementViewModel.cs
│   │   ├── CategoryManagementViewModel.cs
│   │   ├── ReportsViewModel.cs
│   │   ├── SettingsViewModel.cs
│   │   ├── TierSettingsViewModel.cs
│   │   └── UserManagementViewModel.cs
│   │
│   ├── Services/                     # Business logic & data access
│   │   ├── CategoryService.cs
│   │   ├── CustomerService.cs
│   │   ├── InvoiceService.cs
│   │   ├── ProductService.cs
│   │   ├── SupplierService.cs
│   │   ├── UserService.cs
│   │   └── VoucherService.cs
│   │
│   ├── Data/                         # Database layer
│   │   ├── DatabaseHelper.cs         # Kết nối & thao tác DB trung tâm
│   │   └── DatabaseMigration.cs      # Quản lý migration
│   │
│   ├── Core/                         # Framework & utilities
│   │   ├── BaseViewModel.cs          # INotifyPropertyChanged base
│   │   ├── RelayCommand.cs           # ICommand implementation
│   │   ├── LanguageService.cs        # Chuyển đổi ngôn ngữ
│   │   ├── PaginationHelper.cs
│   │   ├── PasswordHelper.cs
│   │   ├── QRCodeHelper.cs
│   │   ├── SettingsManager.cs
│   │   └── PaymentSettings.cs
│   │
│   ├── Converters/                   # WPF value converters
│   │   └── ValueConverters.cs
│   │
│   └── Resources/
│       └── Languages/                # Đa ngôn ngữ
│           ├── vi.xaml                # Tiếng Việt
│           └── en.xaml                # English
│
└── FashionStore.Tests/               # Unit tests (xUnit)
    ├── FashionStore.Tests.csproj
    ├── CalculationTests.cs
    └── VoucherTests.cs
```

---

## 🚀 Thiết lập & Chạy

### 1. Clone & mở solution

```bash
git clone https://github.com/singuyenvan0206/Sale_management.git
```

Mở `FashionStore.sln` bằng Visual Studio 2022.

### 2. Khôi phục NuGet packages

Visual Studio tự khôi phục khi mở solution. Nếu cần thủ công:

```powershell
dotnet restore
```

### 3. Cấu hình cơ sở dữ liệu

#### Tuỳ chọn 1: XAMPP / phpMyAdmin *(khuyến nghị cho người mới)*

1. Cài đặt [XAMPP](https://www.apachefriends.org/) và khởi động **MySQL**
2. Mở phpMyAdmin (`http://localhost/phpmyadmin`)
3. Tạo database mới tên `main` và import file `database_schema.sql`
4. Cấu hình kết nối trong ứng dụng (**Settings → Database Settings**):

   | Trường | Giá trị |
   |--------|---------|
   | Server | `localhost` |
   | Port | `3306` |
   | Database | `main` |
   | User ID | `root` |
   | Password | *(để trống nếu XAMPP mặc định)* |

#### Tuỳ chọn 2: MySQL standalone

- Đảm bảo MySQL server đang chạy với user có quyền truy cập
- Ứng dụng sẽ tự tạo các bảng cần thiết khi chạy lần đầu

### 4. Chạy ứng dụng

- Chọn cấu hình **Debug** và **FashionStore** làm startup project
- Nhấn **F5** để chạy

---

## 📦 NuGet Packages

| Package | Version | Mục đích |
|---------|---------|----------|
| `MySql.Data` | 9.4.0 | Kết nối MySQL |
| `OxyPlot.Wpf` | 2.1.0 | Biểu đồ báo cáo |
| `QRCoder` | 1.4.3 | Tạo mã QR thanh toán |

---

## 🏗️ Kiến trúc

Dự án tuân theo mô hình **MVVM** (Model-View-ViewModel):

```
View (XAML) ──binding──▶ ViewModel ──calls──▶ Service ──queries──▶ Data (DatabaseHelper)
                              │
                              └── Model (data classes)
```

- **Views** — Giao diện XAML, tối thiểu code-behind
- **ViewModels** — Logic hiển thị, binding, commands (kế thừa `BaseViewModel`, dùng `RelayCommand`)
- **Services** — Business logic & truy vấn dữ liệu
- **Data** — Kết nối DB & migration
- **Core** — Utilities dùng chung (pagination, QR, ngôn ngữ, settings…)
- **Models** — Các lớp dữ liệu (Product, Account, Voucher…)

---

## 🧪 Chạy Unit Tests

```powershell
dotnet test FashionStore.Tests/FashionStore.Tests.csproj
```

---

## 📤 Build Release / Publish

```powershell
dotnet publish WpfApp1/FashionStore.csproj -c Release -r win-x64
```

Hoặc trong Visual Studio: **Build → Publish** (target `win-x64`, framework `net9.0-windows`).

---

## 🌐 Đa ngôn ngữ

Ứng dụng hỗ trợ **Tiếng Việt** và **English**.  
Chuyển đổi ngôn ngữ trong **Settings**. Các resource string nằm tại:

- `Resources/Languages/vi.xaml`
- `Resources/Languages/en.xaml`

---

## 📄 Tài liệu liên quan

- Database schema: [`database_schema.sql`](database_schema.sql)


