using System;

namespace FashionStore.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TaxPercent { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
