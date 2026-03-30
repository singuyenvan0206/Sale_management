using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FashionStore;
using FashionStore.Services;
using FashionStore.Core;

namespace FashionStore.ViewModels
{
    public class UserManagementViewModel : BaseViewModel
    {
        private List<UserManagementItem> _allUsers = new();

        public ObservableCollection<UserManagementItem> FilteredUsers { get; } = new();
        public List<string> RoleFilters { get; } = new() { "", "Admin", "Manager", "Cashier" };

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) FilterUsers(); }
        }

        private string _selectedRoleFilter = "";
        public string SelectedRoleFilter
        {
            get => _selectedRoleFilter;
            set { if (SetProperty(ref _selectedRoleFilter, value)) FilterUsers(); }
        }

        private UserManagementItem? _selectedUser;
        public UserManagementItem? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        private string _statusText = "";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand DeleteAllCommand { get; }
        public ICommand RefreshCommand { get; }

        public UserManagementViewModel()
        {
            AddUserCommand = new RelayCommand(_ => AddUser());
            EditUserCommand = new RelayCommand(p => EditUser(p as UserManagementItem ?? SelectedUser));
            DeleteUserCommand = new RelayCommand(p => DeleteUser(p as UserManagementItem ?? SelectedUser));
            DeleteAllCommand = new RelayCommand(_ => DeleteAll());
            RefreshCommand = new RelayCommand(_ => Load());
            Load();
        }

        private void Load()
        {
            try
            {
                var accounts = UserService.GetAllAccounts();
                _allUsers = accounts.Select(a => new UserManagementItem
                {
                    Id = a.Id,
                    Username = a.Username,
                    EmployeeName = a.EmployeeName,
                    Role = UserService.GetUserRoleEnum(a.Username)
                }).ToList();
                FilterUsers();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterUsers()
        {
            var filtered = _allUsers.Where(u =>
                (string.IsNullOrEmpty(SearchText) ||
                    u.Username.ToLower().Contains(SearchText.ToLower()) ||
                    (u.EmployeeName ?? "").ToLower().Contains(SearchText.ToLower())) &&
                (string.IsNullOrEmpty(SelectedRoleFilter) || u.Role.ToString() == SelectedRoleFilter)
            );

            FilteredUsers.Clear();
            foreach (var u in filtered) FilteredUsers.Add(u);
            StatusText = $"Tìm thấy {FilteredUsers.Count} người dùng";
        }

        private void AddUser()
        {
            var win = new AddEditUserWindow();
            if (win.ShowDialog() == true)
            {
                Load();
                MessageBox.Show("Thêm người dùng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditUser(UserManagementItem? user)
        {
            if (user == null) return;
            var win = new AddEditUserWindow(user);
            if (win.ShowDialog() == true)
            {
                Load();
                MessageBox.Show("Cập nhật người dùng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteUser(UserManagementItem? user)
        {
            if (user == null) return;
            if (user.Username == "admin")
            {
                MessageBox.Show("Không thể xóa tài khoản admin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var r = MessageBox.Show($"Bạn có chắc chắn muốn xóa người dùng '{user.Username}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes)
            {
                if (UserService.DeleteAccount(user.Username)) { Load(); MessageBox.Show("Xóa người dùng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information); }
                else MessageBox.Show("Lỗi xóa người dùng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAll()
        {
            int deletableCount = _allUsers.Count(u => !string.Equals(u.Username, "admin", System.StringComparison.OrdinalIgnoreCase));
            if (deletableCount == 0) { MessageBox.Show("Không có người dùng nào để xóa (ngoài admin).", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            var r = MessageBox.Show($"Bạn có chắc chắn muốn xóa TẤT CẢ {deletableCount} người dùng (trừ admin)?\n\nHành động này không thể hoàn tác.", "Xác nhận xóa tất cả", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r == MessageBoxResult.Yes)
            {
                if (UserService.DeleteAllAccountsExceptAdmin()) { Load(); MessageBox.Show("Đã xóa tất cả người dùng (trừ admin) thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information); }
                else MessageBox.Show("Không thể xóa tất cả người dùng.", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class UserManagementItem
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public string RoleDisplayName => Role switch
        {
            UserRole.Admin => "Quản trị viên",
            UserRole.Manager => "Quản lý",
            UserRole.Cashier => "Thu ngân",
            _ => "Không xác định"
        };

        public string RoleDescription => Role switch
        {
            UserRole.Admin => "Toàn quyền quản lý hệ thống, người dùng, sản phẩm và báo cáo.",
            UserRole.Manager => "Quản lý bán hàng, sản phẩm, báo cáo; không quản lý tài khoản hệ thống.",
            UserRole.Cashier => "Thu ngân, tạo hóa đơn và xem lịch sử giao dịch của cửa hàng.",
            _ => string.Empty
        };
    }
}
