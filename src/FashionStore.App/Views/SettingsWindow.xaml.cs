using FashionStore.App.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace FashionStore.App.Views
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

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e) { }
    }
}
