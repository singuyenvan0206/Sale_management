using System;
using ShopManager.Core.Enums;

namespace ShopManager.Core.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Category { get; set; } = ExpenseCategory.Other.ToDbString();
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
