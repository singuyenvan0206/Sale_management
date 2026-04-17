using Dapper;
using FashionStore.Core.Settings;
using MySql.Data.MySqlClient;

namespace FashionStore.Data
{
    public static class DatabaseHelper
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static void InitializeDatabase()
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                // Run all migrations (Schema creation, Columns update, Seed data, Data fixes)
                DatabaseMigration.RunAllMigrations(connection);
            }
            catch (Exception ex)
            {
                // Rethrow to be caught by App.xaml.cs for UI notification
                throw new InvalidOperationException($"Không thể khởi tạo cơ sở dữ liệu: {ex.Message}", ex);
            }
        }
    }
}

