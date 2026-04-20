using System;

namespace FashionStore.Core.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty; // Rent, Utilities, Salary, Marketing, Other
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
