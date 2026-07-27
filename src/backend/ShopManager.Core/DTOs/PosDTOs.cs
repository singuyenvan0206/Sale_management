using ShopManager.Core.Enums;

namespace ShopManager.Core.DTOs
{
    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public string? Note { get; set; }
    }

    public class CalculateDiscountRequest
    {
        public int? CustomerId { get; set; }
        public string? VoucherCode { get; set; }
        public decimal Subtotal { get; set; }
        public List<CartItemRequest> Items { get; set; } = new();
    }

    public class CalculateDiscountResponse
    {
        public decimal Subtotal { get; set; }
        public decimal MemberDiscount { get; set; }
        public decimal VoucherDiscount { get; set; }
        public decimal PromotionDiscount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal FinalTotal { get; set; }
        public string? MemberTier { get; set; }
        public string? Message { get; set; }
    }

    public class CheckoutApiRequest
    {
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public string PaymentMethod { get; set; } = Enums.PaymentMethod.Cash.ToDbString(); // Cash, Banking, QR, Card
        public string? VoucherCode { get; set; }
        public decimal PaidAmount { get; set; }
        public string? Note { get; set; }
        public List<CartItemRequest> Items { get; set; } = new();
    }
}
