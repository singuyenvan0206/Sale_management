namespace FashionStore.Core.Models
{
    public class Promotion
    {
        public const string TypeFlashSale = "FlashSale";
        public const string TypeBOGO = "BOGO";
        public const string TypeCombo = "Combo";

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = TypeFlashSale;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        // Config for Flash Sale
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;

        // Config for BOGO
        public int? RequiredProductId { get; set; }
        public int RequiredQuantity { get; set; } = 0;
        public int? RewardProductId { get; set; }
        public int RewardQuantity { get; set; } = 0;

        public int? TargetCategoryId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Display Helpers
        public string StatusDisplay => IsActive && StartDate <= DateTime.Now && EndDate >= DateTime.Now ? "Đang chạy" : (IsActive ? "Đã lên lịch / Kết thúc" : "Đã khoá");
        public string PromotionDetails
        {
            get
            {
                if (Type == TypeFlashSale)
                {
                    if (DiscountPercent > 0) return $"Giảm {DiscountPercent}%";
                    if (DiscountAmount > 0) return $"Giảm {DiscountAmount:N0}đ";
                    return "Giá đồng hạng";
                }
                if (Type == TypeBOGO)
                {
                    return $"Mua {RequiredQuantity} tặng {RewardQuantity}";
                }
                return "Combo ưu đãi";
            }
        }
    }
}
