
using System.IO;
using System.Text.Json;

namespace FashionStore
{
    using FashionStore.Services;
    public class TierSettings
    {
        // Regular Tier
        public int RegularMinPoints { get; set; } = 0;
        public decimal RegularDiscountPercent { get; set; } = 0;
        public string RegularBenefits { get; set; } = "KhĂ´ng cĂ³ Æ°u Ä‘Ă£i Ä‘áº·c biá»‡t";
        public string RegularDescription { get; set; } = "Háº¡ng thĂ nh viĂªn cÆ¡ báº£n";

        // Silver Tier
        public int SilverMinPoints { get; set; } = 500;
        public decimal SilverDiscountPercent { get; set; } = 3;
        public string SilverBenefits { get; set; } = "Æ¯u tiĂªn há»— trá»£ khĂ¡ch hĂ ng";
        public string SilverDescription { get; set; } = "Háº¡ng thĂ nh viĂªn báº¡c vá»›i Æ°u Ä‘Ă£i cÆ¡ báº£n";

        // Gold Tier
        public int GoldMinPoints { get; set; } = 1000;
        public decimal GoldDiscountPercent { get; set; } = 7;
        public string GoldBenefits { get; set; } = "Miá»…n phĂ­ giao hĂ ng, Æ°u tiĂªn Ä‘áº·t hĂ ng";
        public string GoldDescription { get; set; } = "Háº¡ng thĂ nh viĂªn vĂ ng vá»›i nhiá»u Æ°u Ä‘Ă£i";

        // Platinum Tier
        public int PlatinumMinPoints { get; set; } = 2000;
        public decimal PlatinumDiscountPercent { get; set; } = 10;
        public string PlatinumBenefits { get; set; } = "TÆ° váº¥n cĂ¡ nhĂ¢n, quĂ  táº·ng sinh nháº­t, sá»± kiá»‡n VIP";
        public string PlatinumDescription { get; set; } = "Háº¡ng thĂ nh viĂªn cao cáº¥p nháº¥t vá»›i Ä‘áº§y Ä‘á»§ Æ°u Ä‘Ă£i";

        // Helper methods
        public decimal GetDiscountForTier(string tier)
        {
            return tier?.ToLower() switch
            {
                "silver" => SilverDiscountPercent,
                "gold" => GoldDiscountPercent,
                "platinum" => PlatinumDiscountPercent,
                _ => RegularDiscountPercent
            };
        }

        public string GetBenefitsForTier(string tier)
        {
            return tier?.ToLower() switch
            {
                "silver" => SilverBenefits,
                "gold" => GoldBenefits,
                "platinum" => PlatinumBenefits,
                _ => RegularBenefits
            };
        }

        public string GetDescriptionForTier(string tier)
        {
            return tier?.ToLower() switch
            {
                "silver" => SilverDescription,
                "gold" => GoldDescription,
                "platinum" => PlatinumDescription,
                _ => RegularDescription
            };
        }

        public string DetermineTierByPoints(int points)
        {
            if (points >= PlatinumMinPoints) return "Platinum";
            if (points >= GoldMinPoints) return "Gold";
            if (points >= SilverMinPoints) return "Silver";
            return "Regular";
        }
    }

    public static class TierSettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FashionStore", "tier_settings.json");

        public static TierSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<TierSettings>(json) ?? new TierSettings();
                }
            }
            catch
            {
                // Silent failure
            }
            return new TierSettings();
        }

        public static bool Save(TierSettings settings)
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


        public static decimal GetTierDiscount(string tier)
        {
            var settings = Load();
            return settings.GetDiscountForTier(tier);
        }


        public static string DetermineTierByPoints(int points)
        {
            var settings = Load();
            return settings.DetermineTierByPoints(points);
        }


        public static int UpdateAllCustomerTiers()
        {
            try
            {
                var customers = CustomerService.GetAllCustomers();
                int updatedCount = 0;

                foreach (var customer in customers)
                {
                    var loyalty = CustomerService.GetCustomerLoyalty(customer.Id);
                    var newTier = DetermineTierByPoints(loyalty.Points);
                    
                    // Only update if tier has changed
                    if (loyalty.Tier != newTier)
                    {
                        if (CustomerService.UpdateCustomerLoyalty(customer.Id, loyalty.Points, newTier))
                        {
                            updatedCount++;
                        }
                    }
                }

                return updatedCount;
            }
            catch
            {
                return -1;
            }
        }
    }
}

