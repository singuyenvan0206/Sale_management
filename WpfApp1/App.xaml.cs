using System.Configuration;
using System.Data;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            TryInitializeDatabaseWithFallback();
        }

        private void TryInitializeDatabaseWithFallback()
        {
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Database initialization failed.\n\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                var result = MessageBox.Show("Open Settings to configure the database connection now?", "Database Connection", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var settings = new SettingsWindow();
                    settings.ShowDialog();
                    try
                    {
                        DatabaseHelper.InitializeDatabase();
                    }
                    catch (System.Exception retryEx)
                    {
                        MessageBox.Show($"Database is still unavailable. The app may not function correctly.\n\n{retryEx.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

}
