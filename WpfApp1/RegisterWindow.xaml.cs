using System.Windows;

namespace WpfApp1
{
    public partial class RegisterWindow : Window
    {
        private MainWindow _loginWindow;

        public RegisterWindow(MainWindow loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu không khớp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DatabaseHelper.RegisterAccount(username, password))
            {
                MessageBox.Show("Đăng ký thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _loginWindow.Show();
            this.Close();
        }
    }
}