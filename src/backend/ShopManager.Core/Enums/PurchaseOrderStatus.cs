namespace ShopManager.Core.Enums
{
    public enum PurchaseOrderStatus
    {
        Draft,
        Pending,
        Received,
        Cancelled
    }

    public static class PurchaseOrderStatusExtensions
    {
        public static string ToDbString(this PurchaseOrderStatus status) => status switch
        {
            PurchaseOrderStatus.Draft => "Draft",
            PurchaseOrderStatus.Pending => "Pending",
            PurchaseOrderStatus.Received => "Received",
            PurchaseOrderStatus.Cancelled => "Cancelled",
            _ => "Draft"
        };

        public static PurchaseOrderStatus ParsePurchaseOrderStatus(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "pending" => PurchaseOrderStatus.Pending,
            "received" => PurchaseOrderStatus.Received,
            "cancelled" or "canceled" => PurchaseOrderStatus.Cancelled,
            _ => PurchaseOrderStatus.Draft
        };
    }
}
