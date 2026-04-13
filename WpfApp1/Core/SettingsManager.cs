using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace FashionStore.Core
{
    public class SettingsConfig
    {
        public string Server { get; set; } = "localhost";
        public uint Port { get; set; } = 3306;
        public string Database { get; set; } = "main";
        public string UserId { get; set; } = "root";
        public string Password { get; set; } = "02062003";
        public bool IsPasswordEncrypted { get; set; } = false;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FashionStore", "settings.json");

        private static readonly byte[] OptionalEntropy = Encoding.UTF8.GetBytes("FashionStoreEntropy");

        public static SettingsConfig Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var config = System.Text.Json.JsonSerializer.Deserialize<SettingsConfig>(json) ?? new SettingsConfig();
                    
                    if (config.IsPasswordEncrypted && !string.IsNullOrEmpty(config.Password))
                    {
                        config.Password = DecryptPassword(config.Password);
                    }
                    
                    return config;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
            return new SettingsConfig();
        }

        public static bool Save(SettingsConfig config, out string error)
        {
            try
            {
                // Create a clone to avoid encrypting the password in the live config object if needed
                var configToSave = new SettingsConfig
                {
                    Server = config.Server,
                    Port = config.Port,
                    Database = config.Database,
                    UserId = config.UserId,
                    Password = config.Password
                };

                if (!string.IsNullOrEmpty(configToSave.Password))
                {
                    configToSave.Password = EncryptPassword(configToSave.Password);
                    configToSave.IsPasswordEncrypted = true;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                var json = System.Text.Json.JsonSerializer.Serialize(configToSave, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static string EncryptPassword(string password)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(password);
                byte[] encrypted = ProtectedData.Protect(data, OptionalEntropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            catch
            {
                return password;
            }
        }

        private static string DecryptPassword(string encryptedPassword)
        {
            try
            {
                byte[] data = Convert.FromBase64String(encryptedPassword);
                byte[] decrypted = ProtectedData.Unprotect(data, OptionalEntropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return encryptedPassword;
            }
        }

        public static string BuildConnectionString()
        {
            var c = Load();
            var csb = new MySqlConnectionStringBuilder
            {
                Server = c.Server,
                Port = c.Port,
                Database = c.Database,
                UserID = c.UserId,
                Password = c.Password,
                CharacterSet = "utf8mb4",
                AllowUserVariables = true,
                DefaultCommandTimeout = 30,
                ConnectionTimeout = 10,
                AllowPublicKeyRetrieval = true,
                SslMode = MySqlSslMode.Disabled
            };
            return csb.ConnectionString;
        }

        public static bool TestConnection(SettingsConfig cfg, out string message)
        {
            try
            {
                var csb = new MySqlConnectionStringBuilder
                {
                    Server = cfg.Server,
                    Port = cfg.Port,
                    Database = cfg.Database,
                    UserID = cfg.UserId,
                    Password = cfg.Password,
                    AllowPublicKeyRetrieval = true,
                    SslMode = MySqlSslMode.Disabled
                };
                using var conn = new MySqlConnection(csb.ConnectionString);
                conn.Open();
                message = "Kết nối thành công.";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Kết nối thất bại: {ex.Message}";
                return false;
            }
        }

        public static bool SetAuthPluginToMysqlNative(SettingsConfig cfg, out string message)
        {
            var log = new StringBuilder();
            try
            {
                var csb = new MySqlConnectionStringBuilder
                {
                    Server = cfg.Server,
                    Port = cfg.Port,
                    UserID = cfg.UserId,
                    Password = cfg.Password,
                    AllowPublicKeyRetrieval = true,
                    SslMode = MySqlSslMode.Disabled
                };

                using var conn = new MySqlConnection(csb.ConnectionString);
                conn.Open();

                using var cmd = conn.CreateCommand();

                var hosts = new[] { "localhost", "127.0.0.1", "%" };
                foreach (var host in hosts)
                {
                    try
                    {
                        cmd.Parameters.Clear();
                        // identifiers (userId and host) are still interpolated but we used parameters for the password at least.
                        // To be even safer, we should validate userId and host against a whitelist or escape them.
                        // For this specific admin tool, it's mostly internal.
                        cmd.CommandText = "ALTER USER @user@host IDENTIFIED WITH mysql_native_password BY @pwd;";
                        // Note: MySQL doesn't always support parameters for identifiers in ALTER USER.
                        // We will stick to the previous version but with parameterized password if possible, 
                        // or just ensure cfg.UserId and host don't contain malicious chars.
                        
                        cmd.CommandText = $"ALTER USER '{cfg.UserId.Replace("'", "''")}'@'{host.Replace("'", "''")}' IDENTIFIED WITH mysql_native_password BY @pwd;";
                        cmd.Parameters.AddWithValue("@pwd", cfg.Password);
                        cmd.ExecuteNonQuery();
                        log.AppendLine($"Đã thay đổi '{cfg.UserId}'@'{host}' thành mysql_native_password.");
                    }
                    catch (MySqlException mex)
                    {
                        log.AppendLine($"Lỗi ALTER USER cho '{cfg.UserId}'@'{host}': {mex.Message}");
                    }
                }

                message = log.ToString();
                return true;
            }
            catch (Exception ex)
            {
                message = $"Thao tác thất bại: {ex.Message}\n{log}";
                return false;
            }
        }
    }
}
