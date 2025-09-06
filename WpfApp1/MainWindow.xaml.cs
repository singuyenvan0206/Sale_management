using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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