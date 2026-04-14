namespace FashionStore.Core.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = "Cashier";

        // Mật khẩu không nên giữ trên model để binding UI, ngoại trừ lúc Update/Create
        public string Password { get; set; } = string.Empty;
    }
}
