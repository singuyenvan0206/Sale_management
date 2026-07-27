using Dapper;
using ShopManager.Core.Settings;
using MySql.Data.MySqlClient;

namespace ShopManager.Data
{
    public static class DatabaseHelper
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static void InitializeDatabase()
        {
            try
            {
                // Extract database name from connection string
                var builder = new MySqlConnectionStringBuilder(ConnectionString);
                string dbName = builder.Database;

                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    // Connect to MySQL server without specifying a database to create the database if it doesn't exist
                    var masterBuilder = new MySqlConnectionStringBuilder(ConnectionString)
                    {
                        Database = ""
                    };

                    using (var masterConnection = new MySqlConnection(masterBuilder.ConnectionString))
                    {
                        masterConnection.Open();
                        using var command = masterConnection.CreateCommand();
                        command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
                        command.ExecuteNonQuery();
                    }
                }

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

