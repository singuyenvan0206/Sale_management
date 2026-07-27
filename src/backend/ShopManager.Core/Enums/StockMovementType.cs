namespace ShopManager.Core.Enums
{
    public enum StockMovementType
    {
        Import,
        Export,
        Sale,
        Adjustment,
        Return,
        Transfer
    }

    public static class StockMovementTypeExtensions
    {
        public static string ToDbString(this StockMovementType type) => type switch
        {
            StockMovementType.Import => "Import",
            StockMovementType.Export => "Export",
            StockMovementType.Sale => "Sale",
            StockMovementType.Adjustment => "Adjustment",
            StockMovementType.Return => "Return",
            StockMovementType.Transfer => "Transfer",
            _ => "Import"
        };

        public static StockMovementType ParseStockMovementType(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "export" => StockMovementType.Export,
            "sale" => StockMovementType.Sale,
            "adjustment" => StockMovementType.Adjustment,
            "return" => StockMovementType.Return,
            "transfer" => StockMovementType.Transfer,
            _ => StockMovementType.Import
        };
    }
}
