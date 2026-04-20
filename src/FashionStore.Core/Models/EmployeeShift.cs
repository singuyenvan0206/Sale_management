using System;

namespace FashionStore.Core.Models
{
    public class EmployeeShift
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal? ClosingBalance { get; set; }
        public string? Notes { get; set; }

        public bool IsActive => !ClockOut.HasValue;
    }
}
