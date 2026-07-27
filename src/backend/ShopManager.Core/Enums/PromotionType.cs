namespace ShopManager.Core.Enums
{
    public enum PromotionType
    {
        FlashSale,
        BOGO,
        Combo
    }

    public static class PromotionTypeExtensions
    {
        public static string ToDbString(this PromotionType type) => type switch
        {
            PromotionType.FlashSale => "FlashSale",
            PromotionType.BOGO => "BOGO",
            PromotionType.Combo => "Combo",
            _ => "FlashSale"
        };

        public static PromotionType ParsePromotionType(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "bogo" => PromotionType.BOGO,
            "combo" => PromotionType.Combo,
            _ => PromotionType.FlashSale
        };
    }
}
