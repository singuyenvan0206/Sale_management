using System.Windows;

namespace FashionStore
{
    using FashionStore.Services;
    public partial class PasswordMigrationWindow : Window
    {
        public PasswordMigrationWindow()
        {
            InitializeComponent();
        }

        private void RunMigrationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RunMigrationButton.IsEnabled = false;
                StatusBorder.Visibility = Visibility.Visible;
                StatusText.Text = "Äang cháº¡y migration... Vui lĂ²ng Ä‘á»£i.";

                // Cháº¡y migration
                var (success, migratedCount, message) = UserService.RunPasswordMigration();

                if (success)
                {
                    StatusBorder.Background = migratedCount > 0 
                        ? System.Windows.Media.Brushes.LightGreen 
                        : System.Windows.Media.Brushes.LightBlue;
                    StatusBorder.BorderBrush = migratedCount > 0 
                        ? System.Windows.Media.Brushes.Green 
                        : System.Windows.Media.Brushes.Blue;
                    StatusText.Foreground = migratedCount > 0 
                        ? System.Windows.Media.Brushes.DarkGreen 
                        : System.Windows.Media.Brushes.DarkBlue;
                    StatusText.Text = $"âœ… {message}";
                }
                else
                {
                    StatusBorder.Background = System.Windows.Media.Brushes.LightPink;
                    StatusBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                    StatusText.Foreground = System.Windows.Media.Brushes.DarkRed;
                    StatusText.Text = $"âŒ {message}";
                }
            }
            catch (System.Exception ex)
            {
                StatusBorder.Background = System.Windows.Media.Brushes.LightPink;
                StatusBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                StatusText.Foreground = System.Windows.Media.Brushes.DarkRed;
                StatusText.Text = $"âŒ Lá»—i: {ex.Message}";
            }
            finally
            {
                RunMigrationButton.IsEnabled = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}


