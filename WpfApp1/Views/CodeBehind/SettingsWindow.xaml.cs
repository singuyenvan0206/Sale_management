using System.Windows;
using System.Windows.Input;
using FashionStore.ViewModels;

namespace FashionStore
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
        }

        // Kept in code-behind: PasswordBox bridge
        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
            => _vm.TestConnection(PasswordBox.Password);

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
            => _vm.SaveDbSettings(PasswordBox.Password);

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e) { }
    }
}
