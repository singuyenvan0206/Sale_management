namespace WpfApp1
{
    public class Voucher
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "FixedAmount"
        public decimal DiscountValue { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public decimal MinInvoiceAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public int UsageLimitPerCustomer { get; set; }
        public int UsedCount { get; set; }
        public string ApplicableCategories { get; set; } = string.Empty; // JSON array
        public string ApplicableProducts { get; set; } = string.Empty; // JSON array
        public string ApplicableCustomerTypes { get; set; } = string.Empty; // Comma-separated
        public bool IsActive { get; set; } = true;
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsValid()
        {
            if (!IsActive) return false;
            var now = DateTime.Now;
            if (now < StartDate || now > EndDate) return false;
            if (UsageLimit > 0 && UsedCount >= UsageLimit) return false;
            return true;
        }
    }
}
