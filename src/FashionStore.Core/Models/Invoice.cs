namespace FashionStore.Core.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }

        // DB column is "DiscountAmount" (renamed from "Discount" via migration)
        public decimal Discount { get; set; }  // maps to DiscountAmount in DB

        public decimal Total { get; set; }
        public decimal Paid { get; set; }
        public int? VoucherId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? Note { get; set; }
        public string Status { get; set; } = "Completed";

        // Populated by JOIN queries
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;

        // Navigation properties
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public int EmployeeId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public string? Note { get; set; }

        // Populated by JOIN queries
        public string ProductName { get; set; } = string.Empty;
    }
}
