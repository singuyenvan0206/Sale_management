using System;
using System.IO;
using System.Windows;

namespace WpfApp1
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            var cfg = SettingsManager.Load();
            ServerTextBox.Text = cfg.Server;
            DatabaseTextBox.Text = cfg.Database;
            UserIdTextBox.Text = cfg.UserId;
            PasswordBox.Password = cfg.Password;
        }

        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            var cfg = new SettingsConfig
            {
                Server = ServerTextBox.Text.Trim(),
                Database = DatabaseTextBox.Text.Trim(),
                UserId = UserIdTextBox.Text.Trim(),
                Password = PasswordBox.Password
            };

            bool ok = SettingsManager.TestConnection(cfg, out string message);
            StatusTextBlock.Text = message;
            MessageBox.Show(message, ok ? "Success" : "Error", MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Error);
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var cfg = new SettingsConfig
            {
                Server = ServerTextBox.Text.Trim(),
                Database = DatabaseTextBox.Text.Trim(),
                UserId = UserIdTextBox.Text.Trim(),
                Password = PasswordBox.Password
            };

            if (SettingsManager.Save(cfg, out string error))
            {
                StatusTextBlock.Text = "Settings saved. Restart app to apply.";
                MessageBox.Show("Settings saved. Please restart the app to apply the new database connection.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(error, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
