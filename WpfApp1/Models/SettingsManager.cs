using System;
using System.IO;
using System.Text;
using MySql.Data.MySqlClient;

namespace FashionStore
{
	public class SettingsConfig
	{
		public string Server { get; set; } = "localhost";
		public uint Port { get; set; } = 3306;
		public string Database { get; set; } = "main";
		public string UserId { get; set; } = "root";
		public string Password { get; set; } = "02062003";
	}

	public static class SettingsManager
	{
		private static readonly string SettingsPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"FashionStore", "settings.json");

		public static SettingsConfig Load()
		{
			try
			{
				if (File.Exists(SettingsPath))
				{
					var json = File.ReadAllText(SettingsPath);
					return System.Text.Json.JsonSerializer.Deserialize<SettingsConfig>(json) ?? new SettingsConfig();
				}
			}
			catch
			{
				// ignore and return defaults
			}
			return new SettingsConfig();
		}

		public static bool Save(SettingsConfig config, out string error)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
				var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
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
				message = "Connection successful.";
				return true;
			}
			catch (Exception ex)
			{
				message = $"Connection failed: {ex.Message}";
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
						cmd.CommandText = $"ALTER USER '{cfg.UserId}'@'{host}' IDENTIFIED WITH mysql_native_password BY @pwd;";
						cmd.Parameters.AddWithValue("@pwd", cfg.Password);
						cmd.ExecuteNonQuery();
						log.AppendLine($"Successfully altered '{cfg.UserId}'@'{host}' to mysql_native_password.");
					}
					catch (MySqlException mex)
					{
						log.AppendLine($"ALTER USER failed for '{cfg.UserId}'@'{host}': {mex.Message}");
					}
				}

				message = log.ToString();
				return true;
			}
			catch (Exception ex)
			{
				message = $"Operation failed: {ex.Message}\n{log}";
				return false;
			}
		}
	}
}
