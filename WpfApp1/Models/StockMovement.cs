using System;

namespace FashionStore
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string MovementType { get; set; } = string.Empty; // Import, Sale, Adjustment, Return, Transfer
        public int Quantity { get; set; }
        public int PreviousStock { get; set; }
        public int NewStock { get; set; }
        public string ReferenceType { get; set; } = string.Empty; // Invoice, PurchaseOrder, Adjustment, etc.
        public int? ReferenceId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
