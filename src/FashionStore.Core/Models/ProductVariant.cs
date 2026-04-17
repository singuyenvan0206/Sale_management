namespace FashionStore.Core.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Size { get; set; } = string.Empty; // S, M, L, XL, etc.
        public string Color { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal PriceAdjustment { get; set; } // Extra charge for specific variant

        // Navigation property or reference for UI
        public string ProductName { get; set; } = string.Empty;
    }
}
