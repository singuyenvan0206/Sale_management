using System;
using System.Collections.Generic;

namespace FashionStore.Core.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Pending, Received, Cancelled
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}
