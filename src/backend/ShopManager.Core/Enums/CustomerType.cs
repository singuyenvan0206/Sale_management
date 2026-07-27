namespace ShopManager.Core.Enums
{
    public enum CustomerType
    {
        Regular,
        Silver,
        Gold,
        VIP
    }

    public static class CustomerTypeExtensions
    {
        public static string ToDbString(this CustomerType type) => type switch
        {
            CustomerType.Regular => "Regular",
            CustomerType.Silver => "Silver",
            CustomerType.Gold => "Gold",
            CustomerType.VIP => "VIP",
            _ => "Regular"
        };

        public static CustomerType ParseCustomerType(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "silver" => CustomerType.Silver,
            "gold" => CustomerType.Gold,
            "vip" => CustomerType.VIP,
            _ => CustomerType.Regular
        };
    }
}
