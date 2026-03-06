namespace WpfApp1
{
    public class Voucher
    {
        public const string TypePercentage = "Percentage";
        public const string TypeFixedAmount = "FixedAmount";

        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        private string _discountType = TypePercentage;
        public string DiscountType
        {
            get => _discountType;
            set
            {
                if (value == "%") _discountType = TypePercentage;
                else if (value == "VND") _discountType = TypeFixedAmount;
                else _discountType = value;
            }
        }

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

        public bool IsValid(decimal subtotal = 0, int currentCustomerUsage = -1)
        {
            if (!IsActive) return false;
            var now = DateTime.Now;
            if (now < StartDate || now > EndDate) return false;
            
            // Global usage limit
            if (UsageLimit > 0 && UsedCount >= UsageLimit) return false;
            
            // Per customer usage limit (if customer usage count is provided)
            if (UsageLimitPerCustomer > 0 && currentCustomerUsage >= 0 && currentCustomerUsage >= UsageLimitPerCustomer) 
                return false;

            // Minimum invoice amount check
            if (subtotal > 0 && subtotal < MinInvoiceAmount) return false;

            return true;
        }

    }
}
