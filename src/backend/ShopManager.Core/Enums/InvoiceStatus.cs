namespace ShopManager.Core.Enums
{
    public enum InvoiceStatus
    {
        Completed,
        Pending,
        Cancelled,
        Refunded
    }

    public static class InvoiceStatusExtensions
    {
        public static string ToDbString(this InvoiceStatus status) => status switch
        {
            InvoiceStatus.Completed => "Completed",
            InvoiceStatus.Pending => "Pending",
            InvoiceStatus.Cancelled => "Cancelled",
            InvoiceStatus.Refunded => "Refunded",
            _ => "Completed"
        };

        public static InvoiceStatus ParseInvoiceStatus(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "pending" => InvoiceStatus.Pending,
            "cancelled" or "canceled" => InvoiceStatus.Cancelled,
            "refunded" => InvoiceStatus.Refunded,
            _ => InvoiceStatus.Completed
        };
    }
}
