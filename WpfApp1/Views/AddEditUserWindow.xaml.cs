
using System.Windows;
using System.Windows.Controls;
using FashionStore.ViewModels;

namespace FashionStore
{
    using FashionStore.Services;
    public partial class AddEditUserWindow : Window
    {
        private UserManagementItem _user;
        private bool _isEditMode;

        public AddEditUserWindow()
        {
            InitializeComponent();
            _user = new UserManagementItem();
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
                HeaderText.Text = "âœï¸ Sá»­a NgÆ°á»i DĂ¹ng";
                UsernameTextBox.Text = _user.Username;
                EmployeeNameTextBox.Text = _user.EmployeeName ?? "";
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
                    ShowErrorMessage("Vui lĂ²ng nháº­p tĂªn Ä‘Äƒng nháº­p.");
                    UsernameTextBox.Focus();
                    return;
                }

                if(!_isEditMode && string.IsNullOrWhiteSpace(EmployeeNameTextBox.Text))
                {
                    ShowErrorMessage("Vui lĂ²ng nháº­p há» tĂªn nhĂ¢n viĂªn.");
                    EmployeeNameTextBox.Focus();
                    return;
                }

                // Password rules:
                // - Add mode: password required and must match confirm
                // - Edit mode: password optional; if provided, must match confirm
                if (!_isEditMode)
                {
                    if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                    {
                        ShowErrorMessage("Vui lĂ²ng nháº­p máº­t kháº©u.");
                        PasswordBox.Focus();
                        return;
                    }
                    if (PasswordBox.Password != ConfirmPasswordBox.Password)
                    {
                        ShowErrorMessage("Máº­t kháº©u xĂ¡c nháº­n khĂ´ng khá»›p.");
                        ConfirmPasswordBox.Focus();
                        return;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(PasswordBox.Password) || !string.IsNullOrEmpty(ConfirmPasswordBox.Password))
                    {
                        if (PasswordBox.Password != ConfirmPasswordBox.Password)
                        {
                            ShowErrorMessage("Máº­t kháº©u xĂ¡c nháº­n khĂ´ng khá»›p.");
                            ConfirmPasswordBox.Focus();
                            return;
                        }
                    }
                }

                if (RoleComboBox.SelectedItem is not ComboBoxItem selectedRole)
                {
                    ShowErrorMessage("Vui lĂ²ng chá»n vai trĂ².");
                    RoleComboBox.Focus();
                    return;
                }

                string roleString = selectedRole.Tag?.ToString() ?? "Cashier";
                string username = UsernameTextBox.Text.Trim();
                string employeeName = EmployeeNameTextBox.Text.Trim();
                string password = PasswordBox.Password;

                bool success;
                if (_isEditMode)
                {
                    // Edit mode - update only fields provided
                    string? newPassword = string.IsNullOrWhiteSpace(password) ? null : password;
                    string? newRole = roleString;
                    string? newEmployeeName = string.IsNullOrWhiteSpace(employeeName) ? null : employeeName;
                    success = UserService.UpdateAccount(username, newPassword, newRole, newEmployeeName);
                }
                else
                {
                    // Add mode - sá»­ dá»¥ng employeeName má»›i
                    success = UserService.RegisterAccount(username, employeeName, password, roleString);
                }

                if (success)
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    ShowErrorMessage(_isEditMode ? "Lá»—i cáº­p nháº­t ngÆ°á»i dĂ¹ng." : "TĂªn Ä‘Äƒng nháº­p Ä‘Ă£ tá»“n táº¡i.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Lá»—i: {ex.Message}");
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
        private void Grid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (RoleComboBox?.IsDropDownOpen == true)
            {
                e.Handled = true;
            }
        }
    }
}

