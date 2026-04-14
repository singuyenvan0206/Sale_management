using FashionStore.App.Core;
using FashionStore.App.Views;
using FashionStore.Services;
using System.Windows;
using System.Windows.Input;

namespace FashionStore.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            // Cho PasswordBox, binding thẳng vào thuộc tính thuờng gặp khó khăn về bảo mật. 
            // Ta dùng properties giả lập hoặc truyền qua parameter đối với PasswordBox.
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand OpenChangePasswordCommand { get; }

        public MainViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            OpenChangePasswordCommand = new RelayCommand(ExecuteOpenChangePassword);
        }

        private bool CanExecuteLogin(object? parameter)
        {
            // Cho phép click nếu pass Parameter (PasswordBox) không null và TextBox không trống.
            return !string.IsNullOrWhiteSpace(Username);
        }

        private void ExecuteLogin(object? parameter)
        {
            // parameter được bind từ PasswordBox
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            string password = passwordBox?.Password ?? "";

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string roleStatus = UserService.ValidateLogin(Username, password);
            if (roleStatus == "true")
            {
                var userRole = UserService.GetUserRoleEnum(Username);

                // Mở Dashboard
                Window dashboard = new DashboardWindow(Username, userRole.ToString());
                Application.Current.MainWindow = dashboard;
                dashboard.Show();

                // Đóng Window đăng nhập
                if (parameter is System.Windows.Controls.PasswordBox pb)
                {
                    var window = Window.GetWindow(pb);
                    window?.Close();
                }
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteOpenChangePassword(object? parameter)
        {
            var changePasswordWindow = new ChangePasswordWindow();
            if (parameter is UIElement element)
            {
                var owner = Window.GetWindow(element);
                if (owner != null && owner.IsLoaded)
                {
                    changePasswordWindow.Owner = owner;
                    changePasswordWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
            }
            changePasswordWindow.ShowDialog();
        }
    }
}
