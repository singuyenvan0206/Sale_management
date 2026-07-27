namespace ShopManager.Core.Enums
{
    public enum VoucherActiveFilter
    {
        All,
        Active,
        Inactive
    }

    public enum VoucherValidityFilter
    {
        All,
        ValidNow,
        Expired,
        Upcoming
    }

    public enum VoucherUsageFilter
    {
        All,
        Available,
        FullyUsed
    }

    public enum ShiftStatus
    {
        Open,
        Closed
    }

    public static class ShiftStatusExtensions
    {
        public static string ToDbString(this ShiftStatus status) => status switch
        {
            ShiftStatus.Open => "Open",
            ShiftStatus.Closed => "Closed",
            _ => "Open"
        };

        public static ShiftStatus ParseShiftStatus(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "closed" => ShiftStatus.Closed,
            _ => ShiftStatus.Open
        };
    }
}
