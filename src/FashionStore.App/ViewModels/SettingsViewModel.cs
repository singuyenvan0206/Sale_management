using FashionStore.App.Core;
using FashionStore.App.Views;
using FashionStore.Core.Models;
using FashionStore.Core.Settings;
using FashionStore.Services;
using System.Windows;
using System.Windows.Input;
using FashionStore.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace FashionStore.App.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        // -------- DB Config --------
        private string _server = "";
        public string Server { get => _server; set => SetProperty(ref _server, value); }

        private string _database = "";
        public string Database { get => _database; set => SetProperty(ref _database, value); }

        private string _userId = "";
        public string UserId { get => _userId; set => SetProperty(ref _userId, value); }

        // Password is read from the PasswordBox in code-behind — not bound here

        private string _statusText = "";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        // -------- Payment Settings --------
        private bool _enableQRCode;
        public bool EnableQRCode
        {
            get => _enableQRCode;
            set
            {
                if (SetProperty(ref _enableQRCode, value))
                    QRCodeStatusText = value ? "QR Code đã được bật" : "QR Code đã được tắt";
            }
        }

        private string _qrCodeStatusText = "";
        public string QRCodeStatusText { get => _qrCodeStatusText; set => SetProperty(ref _qrCodeStatusText, value); }

        private string _bankAccount = "";
        public string BankAccount { get => _bankAccount; set => SetProperty(ref _bankAccount, value); }

        private string _bankCode = "";
        public string BankCode { get => _bankCode; set => SetProperty(ref _bankCode, value); }

        private string _accountHolder = "";
        public string AccountHolder { get => _accountHolder; set => SetProperty(ref _accountHolder, value); }

        private string _sePayToken = "";
        public string SePayToken { get => _sePayToken; set => SetProperty(ref _sePayToken, value); }

        // -------- Role Permissions --------
        private bool _isTierSettingsVisible = true;
        public bool IsTierSettingsVisible { get => _isTierSettingsVisible; set => SetProperty(ref _isTierSettingsVisible, value); }

        // -------- Commands --------
        public ICommand TestConnectionCommand { get; }
        public ICommand SaveDbSettingsCommand { get; }
        public ICommand SavePaymentSettingsCommand { get; }
        public ICommand OpenTierSettingsCommand { get; }
        public ICommand SaveGeneralSettingsCommand { get; }

        private readonly ISystemSettingsService _settingsService;


        // Password is passed as a parameter from code-behind
        public SettingsViewModel()
        {
            TestConnectionCommand = new RelayCommand(p => TestConnection(p?.ToString() ?? ""));
            SaveDbSettingsCommand = new RelayCommand(p => SaveDbSettings(p?.ToString() ?? ""));
            SavePaymentSettingsCommand = new RelayCommand(p => SavePaymentSettings(p?.ToString() ?? ""));
            OpenTierSettingsCommand = new RelayCommand(_ => OpenTierSettings());
            SaveGeneralSettingsCommand = new RelayCommand(p => SaveGeneralSettings(p?.ToString() ?? ""));

            _settingsService = App.ServiceProvider?.GetService<ISystemSettingsService>()
                               ?? throw new Exception("SystemSettingsService not found");


            LoadDbSettings();
            LoadPaymentSettings();
            ApplyRolePermissions();
        }

        private void LoadDbSettings()
        {
            var cfg = SettingsManager.Load();
            Server = cfg.Server;
            Database = cfg.Database;
            UserId = cfg.UserId;
            // Password loaded in code-behind via PasswordBox
        }

        private async void LoadPaymentSettings()
        {
            var ps = PaymentSettingsManager.Load();
            EnableQRCode = ps.EnableQRCode;
            BankAccount = ps.BankAccount;
            BankCode = ps.BankCode;
            AccountHolder = ps.AccountHolder;
            
            // Load SePay Token from DB
            SePayToken = await _settingsService.GetSePayTokenAsync() ?? "";
        }

        public async Task<string?> GetSePayTokenAsync()
        {
            return await _settingsService.GetSePayTokenAsync();
        }

        private void ApplyRolePermissions()
        {
            var currentUser = Application.Current.Resources["CurrentUser"] as string;
            if (string.IsNullOrEmpty(currentUser)) return;
            var roleStr = UserService.GetUserRole(currentUser);
            var role = ParseRole(roleStr);
            IsTierSettingsVisible = role == UserRole.Admin || role == UserRole.Manager;
        }

        private static UserRole ParseRole(string role) => role?.Trim().ToLower() switch
        {
            "admin" => UserRole.Admin,
            "manager" => UserRole.Manager,
            _ => UserRole.Cashier
        };

        public void TestConnection(string password)
        {
            var cfg = new SettingsConfig { Server = Server, Database = Database, UserId = UserId, Password = password };
            bool ok = SettingsManager.TestConnection(cfg, out string message);
            StatusText = message;
            MessageBox.Show(message, ok ? "Thành công" : "Lỗi", MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Error);
        }

        public void SaveDbSettings(string password)
        {
            var cfg = new SettingsConfig { Server = Server, Database = Database, UserId = UserId, Password = password };
            if (SettingsManager.Save(cfg, out string error))
            {
                StatusText = "Cài đặt đã được lưu. Khởi động lại ứng dụng để áp dụng.";
                MessageBox.Show("Cài đặt đã được lưu. Vui lòng khởi động lại ứng dụng để áp dụng kết nối cơ sở dữ liệu mới.", "Đã lưu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show(error, "Lưu thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void SavePaymentSettings(string token)
        {
            if (string.IsNullOrWhiteSpace(BankAccount)) { MessageBox.Show("Vui lòng nhập số tài khoản ngân hàng.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (string.IsNullOrWhiteSpace(BankCode)) { MessageBox.Show("Vui lòng nhập mã ngân hàng.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            var ps = new PaymentSettings { EnableQRCode = EnableQRCode, BankAccount = BankAccount, BankCode = BankCode, BankName = "Ngân hàng", AccountHolder = AccountHolder };
            
            bool fileSaved = PaymentSettingsManager.Save(ps);
            bool dbSaved = await _settingsService.SaveSePayTokenAsync(token);

            if (fileSaved && dbSaved)
            {
                StatusText = "Thông tin thanh toán đã được lưu thành công vào Database!";
                MessageBox.Show("Thông tin thanh toán đã được lưu thành công!", "Đã lưu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Có lỗi xảy ra khi lưu thông tin. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SaveGeneralSettings(string password)
        {
            var cfg = new SettingsConfig
            {
                Server = Server,
                Database = Database,
                UserId = UserId,
                Password = password
            };

            if (SettingsManager.Save(cfg, out string error))
            {
                StatusText = "Cài đặt chung đã được lưu.";
                MessageBox.Show("Cài đặt đã được lưu thành công.", "Đã lưu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show(error, "Lưu thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void OpenTierSettings()
        {
            try
            {
                var w = new TierSettingsWindow { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                w.ShowDialog();
            }
            catch (System.Exception ex) { MessageBox.Show($"Lỗi khi mở cài đặt hạng thành viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }



        public static string LoadPassword()
        {
            var cfg = SettingsManager.Load();
            return cfg.Password;
        }
    }
}
