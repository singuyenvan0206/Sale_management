using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FashionStore.Services
{
    public static class UserService
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static bool RegisterAccount(string username, string employeeName, string password, string role = "Cashier")
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string hashedPassword = PasswordHelper.HashPassword(password);
            using var cmd = new MySqlCommand("INSERT INTO Accounts (Username, EmployeeName, Password, Role) VALUES (@username, @employeeName, @password, @role);", connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@employeeName", employeeName);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            cmd.Parameters.AddWithValue("@role", role);
            try { return cmd.ExecuteNonQuery() > 0; }
            catch { return false; }
        }

        public static string ValidateLogin(string username, string password)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT Password FROM Accounts WHERE Username=@username;", connection);
            cmd.Parameters.AddWithValue("@username", username);
            var storedPassword = cmd.ExecuteScalar()?.ToString();
            
            if (string.IsNullOrEmpty(storedPassword))
                return "false";
            
            bool isValid = PasswordHelper.VerifyPassword(password, storedPassword);
            return isValid ? "true" : "false";
        }

        public static string GetUserRole(string username)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT Role FROM Accounts WHERE Username=@username;", connection);
            cmd.Parameters.AddWithValue("@username", username);
            return cmd.ExecuteScalar()?.ToString() ?? "Cashier";
        }

        public static UserRole GetUserRoleEnum(string username)
        {
            string roleString = GetUserRole(username);
            return roleString.ToLower() switch
            {
                "admin" => UserRole.Admin,
                "manager" => UserRole.Manager,
                "cashier" => UserRole.Cashier,
                _ => UserRole.Cashier
            };
        }

        public static bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            string getPasswordCmd = "SELECT Password FROM Accounts WHERE Username=@username;";
            using var getPassword = new MySqlCommand(getPasswordCmd, connection);
            getPassword.Parameters.AddWithValue("@username", username);
            var storedPassword = getPassword.ExecuteScalar()?.ToString();

            if (string.IsNullOrEmpty(storedPassword))
                return false;

            bool isOldPasswordValid = PasswordHelper.VerifyPassword(oldPassword, storedPassword);
            if (!isOldPasswordValid)
                return false;

            string hashedNewPassword = PasswordHelper.HashPassword(newPassword);
            string updateCmd = "UPDATE Accounts SET Password=@newPassword WHERE Username=@username;";
            using var update = new MySqlCommand(updateCmd, connection);
            update.Parameters.AddWithValue("@username", username);
            update.Parameters.AddWithValue("@newPassword", hashedNewPassword);
            return update.ExecuteNonQuery() > 0;
        }

        public static List<(int Id, string Username, string EmployeeName)> GetAllAccounts()
        {
            var accounts = new List<(int, string, string)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string selectCmd = "SELECT Id, Username, COALESCE(EmployeeName, '') FROM Accounts;";
            using var cmd = new MySqlCommand(selectCmd, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                accounts.Add((reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? "" : reader.GetString(2)));
            }
            return accounts;
        }

        public static string GetEmployeeName(string username)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                string selectCmd = "SELECT COALESCE(EmployeeName, Username) FROM Accounts WHERE Username = @username;";
                using var cmd = new MySqlCommand(selectCmd, connection);
                cmd.Parameters.AddWithValue("@username", username);
                var result = cmd.ExecuteScalar();
                return result?.ToString() ?? username;
            }
            catch
            {
                return username;
            }
        }

        public static int GetEmployeeIdByUsername(string username)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                string selectCmd = "SELECT Id FROM Accounts WHERE Username = @username;";
                using var cmd = new MySqlCommand(selectCmd, connection);
                cmd.Parameters.AddWithValue("@username", username);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 1;
            }
            catch
            {
                return 1;
            }
        }

        public static bool DeleteAccount(string username)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string deleteCmd = "DELETE FROM Accounts WHERE Username=@username;";
            using var cmd = new MySqlCommand(deleteCmd, connection);
            cmd.Parameters.AddWithValue("@username", username);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeleteAllAccountsExceptAdmin()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            try
            {
                string sql = "DELETE FROM Accounts WHERE LOWER(Username) <> 'admin';";
                using var cmd = new MySqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool UpdateAccount(string username, string? newPassword = null, string? newRole = null, string? newEmployeeName = null)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            var sets = new List<string>();
            using var cmd = new MySqlCommand();
            cmd.Connection = connection;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                string hashedPassword = PasswordHelper.HashPassword(newPassword);
                sets.Add("Password=@password");
                cmd.Parameters.AddWithValue("@password", hashedPassword);
            }
            if (!string.IsNullOrWhiteSpace(newRole))
            {
                sets.Add("Role=@role");
                cmd.Parameters.AddWithValue("@role", newRole);
            }
            if (!string.IsNullOrWhiteSpace(newEmployeeName))
            {
                sets.Add("EmployeeName=@employeeName");
                cmd.Parameters.AddWithValue("@employeeName", newEmployeeName);
            }

            if (sets.Count == 0) return true;

            cmd.CommandText = $"UPDATE Accounts SET {string.Join(", ", sets)} WHERE Username=@username;";
            cmd.Parameters.AddWithValue("@username", username);

            try { return cmd.ExecuteNonQuery() > 0; }
            catch { return false; }
        }

        public static void MigratePasswordsToHashed(MySqlConnection? connection = null)
        {
            bool shouldCloseConnection = connection == null;
            if (connection == null)
            {
                connection = new MySqlConnection(ConnectionString);
                connection.Open();
            }

            try
            {
                string selectCmd = "SELECT Id, Username, Password FROM Accounts;";
                using var selectCmdObj = new MySqlCommand(selectCmd, connection);
                using var reader = selectCmdObj.ExecuteReader();

                var accountsToMigrate = new List<(int Id, string Username, string Password)>();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string username = reader.GetString(1);
                    string password = reader.GetString(2);

                    if (!PasswordHelper.IsHashed(password))
                    {
                        accountsToMigrate.Add((id, username, password));
                    }
                }
                reader.Close();

                int migratedCount = 0;
                foreach (var account in accountsToMigrate)
                {
                    try
                    {
                        string hashedPassword = PasswordHelper.HashPassword(account.Password);
                        string updateCmd = "UPDATE Accounts SET Password=@password WHERE Id=@id;";
                        using var updateCmdObj = new MySqlCommand(updateCmd, connection);
                        updateCmdObj.Parameters.AddWithValue("@password", hashedPassword);
                        updateCmdObj.Parameters.AddWithValue("@id", account.Id);
                        updateCmdObj.ExecuteNonQuery();
                        migratedCount++;
                    }
                    catch { }
                }

                if (migratedCount > 0) System.Diagnostics.Debug.WriteLine($"ÄĂ£ mĂ£ hĂ³a {migratedCount} máº­t kháº©u.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lá»—i khi migrate máº­t kháº©u: {ex.Message}");
            }
            finally
            {
                if (shouldCloseConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static (bool Success, int MigratedCount, string Message) RunPasswordMigration()
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                string selectCmd = "SELECT Id, Username, Password FROM Accounts;";
                using var selectCmdObj = new MySqlCommand(selectCmd, connection);
                using var reader = selectCmdObj.ExecuteReader();

                var accountsToMigrate = new List<(int Id, string Username, string Password)>();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string username = reader.GetString(1);
                    string password = reader.GetString(2);

                    if (!PasswordHelper.IsHashed(password))
                    {
                        accountsToMigrate.Add((id, username, password));
                    }
                }
                reader.Close();

                if (accountsToMigrate.Count == 0) return (true, 0, "Táº¥t cáº£ máº­t kháº©u Ä‘Ă£ Ä‘Æ°á»£c mĂ£ hĂ³a.");

                int migratedCount = 0;
                int failedCount = 0;
                foreach (var account in accountsToMigrate)
                {
                    try
                    {
                        string hashedPassword = PasswordHelper.HashPassword(account.Password);
                        string updateCmd = "UPDATE Accounts SET Password=@password WHERE Id=@id;";
                        using var updateCmdObj = new MySqlCommand(updateCmd, connection);
                        updateCmdObj.Parameters.AddWithValue("@password", hashedPassword);
                        updateCmdObj.Parameters.AddWithValue("@id", account.Id);
                        updateCmdObj.ExecuteNonQuery();
                        migratedCount++;
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        System.Diagnostics.Debug.WriteLine($"Lá»—i khi migrate máº­t kháº©u cho {account.Username}: {ex.Message}");
                    }
                }

                string message = $"ÄĂ£ mĂ£ hĂ³a {migratedCount} máº­t kháº©u.";
                if (failedCount > 0) message += $" {failedCount} máº­t kháº©u khĂ´ng thá»ƒ mĂ£ hĂ³a.";

                return (failedCount == 0, migratedCount, message);
            }
            catch (Exception ex)
            {
                return (false, 0, $"Lá»—i khi cháº¡y migration: {ex.Message}");
            }
        }
    }
}

