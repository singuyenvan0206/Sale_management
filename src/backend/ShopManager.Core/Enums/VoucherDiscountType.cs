namespace ShopManager.Core.Enums
{
    public enum VoucherDiscountType
    {
        Percentage,
        FixedAmount
    }

    public static class VoucherDiscountTypeExtensions
    {
        public static string ToDbString(this VoucherDiscountType type) => type switch
        {
            VoucherDiscountType.Percentage => "Percentage",
            VoucherDiscountType.FixedAmount => "FixedAmount",
            _ => "Percentage"
        };

        public static VoucherDiscountType ParseVoucherDiscountType(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "fixedamount" or "fixed" or "vnd" => VoucherDiscountType.FixedAmount,
            "percentage" or "%"               => VoucherDiscountType.Percentage,
            _                                 => VoucherDiscountType.Percentage
        };
    }
}
