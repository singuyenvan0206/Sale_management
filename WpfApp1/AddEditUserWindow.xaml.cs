using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class AddEditUserWindow : Window
    {
        private UserManagementItem _user;
        private bool _isEditMode;

        public AddEditUserWindow()
        {
            InitializeComponent();
            _isEditMode = false;
            RoleComboBox.SelectedIndex = 0; // Default to Cashier
            UsernameTextBox.Focus();
        }

        public AddEditUserWindow(UserManagementItem user) : this()
        {
            _user = user;
            _isEditMode = true;
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (_user != null)
            {
                HeaderText.Text = "✏️ Sửa Người Dùng";
                UsernameTextBox.Text = _user.Username;
                UsernameTextBox.IsEnabled = false; // Cannot change username when editing
                
                // Set role
                for (int i = 0; i < RoleComboBox.Items.Count; i++)
                {
                    if (RoleComboBox.Items[i] is ComboBoxItem item && 
                        item.Tag?.ToString() == _user.Role.ToString())
                    {
                        RoleComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HideErrorMessage();

                // Validation
                if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                {
                    ShowErrorMessage("Vui lòng nhập tên đăng nhập.");
                    UsernameTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    ShowErrorMessage("Vui lòng nhập mật khẩu.");
                    PasswordBox.Focus();
                    return;
                }

                if (PasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    ShowErrorMessage("Mật khẩu xác nhận không khớp.");
                    ConfirmPasswordBox.Focus();
                    return;
                }

                if (RoleComboBox.SelectedItem is not ComboBoxItem selectedRole)
                {
                    ShowErrorMessage("Vui lòng chọn vai trò.");
                    RoleComboBox.Focus();
                    return;
                }

                string roleString = selectedRole.Tag?.ToString() ?? "Cashier";
                string username = UsernameTextBox.Text.Trim();
                string password = PasswordBox.Password;

                bool success;
                if (_isEditMode)
                {
                    // Edit mode - only change password and role
                    success = DatabaseHelper.ChangePassword(username, "", password); // Empty old password for admin edit
                    if (success)
                    {
                        // Update role if needed
                        var currentRole = DatabaseHelper.GetUserRole(username);
                        if (currentRole != roleString)
                        {
                            // For simplicity, we'll just show a message that role change requires database update
                            MessageBox.Show("Thay đổi vai trò cần cập nhật trực tiếp trong cơ sở dữ liệu.", 
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    // Add mode
                    success = DatabaseHelper.RegisterAccount(username, password, roleString);
                }

                if (success)
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    ShowErrorMessage(_isEditMode ? "Lỗi cập nhật người dùng." : "Tên đăng nhập đã tồn tại.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Lỗi: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowErrorMessage(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }

        private void HideErrorMessage()
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;
        }
    }
}
