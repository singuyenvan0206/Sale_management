using System.Windows;
using System.Windows.Media.Animation;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowRegisterForm_Click(object sender, RoutedEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Visible;
            LoginPanel.Visibility = Visibility.Collapsed;
            var sb = (Storyboard)FindResource("ShowRegisterStoryboard");
            sb.Begin();
        }

        private void ShowLoginForm_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            var sb = (Storyboard)FindResource("ShowLoginStoryboard");
            sb.Begin();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string role = DatabaseHelper.ValidateLogin(username, password);
            if (role == "true")
            {
                // Get the actual role from database
                var userRole = DatabaseHelper.GetUserRoleEnum(username);
                
                // Open appropriate dashboard based on role
                Window dashboard = userRole switch
                {
                    UserRole.Cashier => new CashierDashboardWindow(username, userRole),
                    UserRole.Manager => new DashboardWindow(username, userRole.ToString()),
                    UserRole.Admin => new DashboardWindow(username, userRole.ToString()),
                    _ => new CashierDashboardWindow(username, userRole)
                };
                
                dashboard.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegUsernameTextBox.Text;
            string password = RegPasswordBox.Password;
            string confirmPassword = RegConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DatabaseHelper.RegisterAccount(username, password))
            {
                MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Optionally, switch back to login form
                ShowLoginForm_Click(null, null);
            }
            else
            {
                MessageBox.Show("Username already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordWindow = new ChangePasswordWindow();
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }
    }
}