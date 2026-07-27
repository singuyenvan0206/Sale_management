using ShopManager.App.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ShopManager.App.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _vm;

        public SettingsWindow()
        {
            InitializeComponent();
            _vm = new SettingsViewModel();
            DataContext = _vm;

            // PasswordBox cannot bind directly — load from settings
            PasswordBox.Password = SettingsViewModel.LoadPassword();
            UseApiCheckBox.IsChecked = _vm.UseApiMode;
            ApiUrlTextBox.Text = _vm.ApiUrl;
            ApiTokenTextBox.Text = _vm.ApiToken;
            LoadSePayToken();
        }

        private async void LoadSePayToken()
        {
            // Wait for VM to load token
            string? token = await _vm.GetSePayTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                SePayTokenBox.Password = token;
            }
        }

        // Kept in code-behind: PasswordBox bridge
        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
            => _vm.TestConnection(PasswordBox.Password);

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.SaveDbSettings(PasswordBox.Password);
            PasswordBox.Clear();
        }

        private void SavePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.SavePaymentSettingsCommand.Execute(SePayTokenBox.Password);
            SePayTokenBox.Clear();
        }

        private async void TestApiConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.UseApiMode = UseApiCheckBox.IsChecked == true;
            _vm.ApiUrl = ApiUrlTextBox.Text;
            _vm.ApiToken = ApiTokenTextBox.Text;
            await _vm.TestApiConnectionAsync();
        }

        private void SaveApiSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.UseApiMode = UseApiCheckBox.IsChecked == true;
            _vm.ApiUrl = ApiUrlTextBox.Text;
            _vm.ApiToken = ApiTokenTextBox.Text;
            _vm.SaveApiSettings();
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e) { }
    }
}
