using System.Text.Json;

namespace FashionStore.Core.Settings
{
    public class PaymentSettings
    {
        public string BankAccount { get; set; } = "";
        public string BankCode { get; set; } = "";
        public string BankName { get; set; } = "";
        public string AccountHolder { get; set; } = "";
        public bool EnableQRCode { get; set; } = true;
        public string SePayToken { get; set; } = ""; // Set via Environment Variable or UI
    }

    public static class PaymentSettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FashionStore", "payment_settings.json");

        public static PaymentSettings Load()
        {
            // Priority 1: Environment Variable (Recommended for security)
            var envToken = Environment.GetEnvironmentVariable("SEPAY_TOKEN");
            
            PaymentSettings settings;
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    settings = JsonSerializer.Deserialize<PaymentSettings>(json) ?? new PaymentSettings();
                }
                else
                {
                    settings = new PaymentSettings();
                }
            }
            catch
            {
                settings = new PaymentSettings();
            }

            // Override file settings if Environment Variable is present
            if (!string.IsNullOrEmpty(envToken))
            {
                settings.SePayToken = envToken;
            }

            return settings;
        }

        public static bool Save(PaymentSettings settings)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

