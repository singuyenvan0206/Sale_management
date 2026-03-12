
using System.Windows;
using System.Windows.Controls;

namespace FashionStore
{
    using FashionStore.Repositories;
    public partial class UserManagementWindow : Window
    {
        private List<UserManagementItem> _allUsers = new();
        private List<UserManagementItem> _filteredUsers = new();

        public UserManagementWindow()
        {
            InitializeComponent();
            LoadUsers();
            RoleFilterComboBox.SelectedIndex = 0; // Select "All roles"
        }

        private void LoadUsers()
        {
            try
            {
                // Use GetAllAccounts and map to UserManagementItem
                var accounts = UserRepository.GetAllAccounts();
                _allUsers = accounts.Select(a => new UserManagementItem
                {
                    Id = a.Id,
                    Username = a.Username,
                    EmployeeName = a.EmployeeName, // Will be fixed when GetAllAccounts is updated
                    Role = UserRepository.GetUserRoleEnum(a.Username)
                }).ToList();

                FilterUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterUsers()
        {
            try
            {
                string searchText = SearchTextBox.Text?.ToLower() ?? "";
                string roleFilter = (RoleFilterComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";

                _filteredUsers = _allUsers.Where(u => 
                    (string.IsNullOrEmpty(searchText) ||
                        u.Username.ToLower().Contains(searchText) ||
                        (u.EmployeeName ?? string.Empty).ToLower().Contains(searchText)) &&
                    (string.IsNullOrEmpty(roleFilter) || u.Role.ToString() == roleFilter)
                ).ToList();

                UsersDataGrid.ItemsSource = _filteredUsers;
                StatusTextBlock.Text = $"Tìm thấy {_filteredUsers.Count} người dùng";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lọc danh sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UsersDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is UserManagementItem user)
            {
                var editUserWindow = new AddEditUserWindow(user);
                if (editUserWindow.ShowDialog() == true)
                {
                    LoadUsers();
                    MessageBox.Show("Cập nhật người dùng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterUsers();
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            FilterUsers();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addUserWindow = new AddEditUserWindow();
                if (addUserWindow.ShowDialog() == true)
                {
                    LoadUsers();
                    MessageBox.Show("Thêm người dùng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is UserManagementItem user)
                {
                    var editUserWindow = new AddEditUserWindow(user);
                    if (editUserWindow.ShowDialog() == true)
                    {
                        LoadUsers();
                        MessageBox.Show("Cập nhật người dùng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi sửa người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is UserManagementItem user)
                {
                    if (user.Username == "admin")
                    {
                        MessageBox.Show("Không thể xóa tài khoản admin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa người dùng '{user.Username}'?", 
                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        if (UserRepository.DeleteAccount(user.Username))
                        {
                            LoadUsers();
                            MessageBox.Show("Xóa người dùng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Lỗi xóa người dùng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeleteAllUsersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_allUsers.Count <= 1)
                {
                    MessageBox.Show("Không có người dùng nào để xóa (ngoại trừ admin).", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                int deletableCount = _allUsers.Count(u => !string.Equals(u.Username, "admin", StringComparison.OrdinalIgnoreCase));
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa TẤT CẢ {deletableCount} người dùng (trừ admin)?\n\nHành động này không thể hoàn tác.",
                    "Xác nhận xóa tất cả", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (UserRepository.DeleteAllAccountsExceptAdmin())
                    {
                        LoadUsers();
                        MessageBox.Show("Đã xóa tất cả người dùng (trừ admin) thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa tất cả người dùng. Vui lòng thử lại hoặc kiểm tra ràng buộc dữ liệu.", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa tất cả người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Grid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (RoleFilterComboBox?.IsDropDownOpen == true)
            {
                e.Handled = true;
            }
        }
    }

    public class UserManagementItem
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        // Hiển thị vai trò thân thiện cho DataGrid
        public string RoleDisplayName =>
            Role switch
            {
                UserRole.Admin => "Quản trị viên",
                UserRole.Manager => "Quản lý",
                UserRole.Cashier => "Thu ngân",
                _ => "Không xác định"
            };

        // Mô tả chi tiết hơn về vai trò
        public string RoleDescription =>
            Role switch
            {
                UserRole.Admin => "Toàn quyền quản lý hệ thống, người dùng, sản phẩm và báo cáo.",
                UserRole.Manager => "Quản lý bán hàng, sản phẩm, báo cáo; không quản lý tài khoản hệ thống.",
                UserRole.Cashier => "Thu ngân, tạo hóa đơn và xem lịch sử giao dịch của cửa hàng.",
                _ => string.Empty
            };
    }
}
