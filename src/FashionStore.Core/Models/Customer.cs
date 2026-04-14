namespace FashionStore.Core.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string CustomerType { get; set; } = "Regular";
        public int Points { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
