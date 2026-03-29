using System;

namespace FashionStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        
        public decimal SalePrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public string PurchaseUnit { get; set; } = "VND";
        
        public int ImportQuantity { get; set; }
        public int StockQuantity { get; set; }
        
        public string Description { get; set; } = string.Empty;
        
        public decimal PromoDiscountPercent { get; set; }
        public DateTime? PromoStartDate { get; set; }
        public DateTime? PromoEndDate { get; set; }
        
        public decimal CategoryTaxPercent { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
    }
}
