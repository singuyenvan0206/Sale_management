# 🚀 Refactor Project Sale Management (Intern-Ready)

## 🎯 Mục tiêu
Biến project hiện tại từ dạng CRUD cơ bản → có structure rõ ràng, áp dụng MVVM, đủ mạnh để apply intern.

---

## 🏗️ 1. Tạo lại structure project

```
SaleManagement/
│
├── Models/
├── Views/
├── ViewModels/
├── Services/
├── Data/
├── App.xaml
└── MainWindow.xaml
```

---

## 🧠 2. Tạo Model

- Tạo class đại diện dữ liệu (VD: Product)
- Chỉ chứa property, không chứa logic

---

## 🔌 3. Tách Database (DatabaseHelper)

- Tạo file `DatabaseHelper.cs`
- Chứa connection string
- Có method trả về connection

👉 Mục tiêu: không hardcode connection ở nhiều nơi

---

## ⚙️ 4. Tạo Service Layer

- Ví dụ: `ProductService`
- Chứa các hàm:
  - GetAll()
  - Add()
  - Update()
  - Delete()

### ⚠️ Lưu ý quan trọng:
- Không viết SQL trong UI
- Luôn dùng parameter để tránh SQL Injection

---

## 🧩 5. Tạo ViewModel

- Tạo `ProductViewModel`
- Implement `INotifyPropertyChanged`

### Chức năng:
- Load dữ liệu từ Service
- Binding dữ liệu ra UI
- Xử lý command (Add, Update...)

---

## 🔘 6. Tạo RelayCommand

- Dùng để bind button với logic
- Thay thế event click trong code-behind

---

## 🖥️ 7. Refactor View (XAML)

### Cần làm:
- Set `DataContext` = ViewModel
- Dùng Binding:
  - TextBox → property
  - DataGrid → list
  - Button → Command

### Tránh:
- Không viết logic trong `.xaml.cs`

---

## 🔄 8. Luồng hoạt động sau refactor

```
View (UI)
   ↓ Binding
ViewModel
   ↓ gọi
Service
   ↓ gọi
Database
```

---

## 📚 9. Viết README

### Nội dung cần có:

```
# Sale Management System

## Tech Stack
- WPF (.NET)
- MySQL

## Features
- Product management
- Order management
- Customer management

## Architecture
- MVVM
- Service Layer

## Setup
1. Import database
2. Config connection string
3. Run project
```

---

## 🔥 10. Thêm feature để nổi bật

Chọn ít nhất 1:

- Dashboard (doanh thu)
- Login + phân quyền
- Export Excel

---

## 🧭 11. Checklist hoàn thiện

- [ ] Tách Models / Views / ViewModels / Services / Data
- [ ] Không còn SQL trong UI
- [ ] Có sử dụng Binding
- [ ] Có ViewModel
- [ ] Có README
- [ ] Thêm ít nhất 1 feature nâng cao

---

## 🏁 Kết quả

Sau khi hoàn thành:

👉 Project từ "bài tập" → "intern-ready"
👉 Dễ pass CV vòng đầu
👉 Recruiter đánh giá có tư duy tốt

---

## 💬 Ghi chú

Không cần làm quá hoàn hảo.
Quan trọng nhất:
- Structure rõ ràng
- Code dễ đọc
- Chạy ổn định

👉 Như vậy là đủ để đi xin intern.

