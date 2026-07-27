namespace ShopManager.Core.DTOs
{
    public class CreateQrRequest
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
    }

    public class CreateQrResponse
    {
        public string QrCodeUrl { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class SePayWebhookPayload
    {
        public long id { get; set; }
        public string gateway { get; set; } = string.Empty;
        public string transactionDate { get; set; } = string.Empty;
        public string accountNo { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public string transferType { get; set; } = string.Empty;
        public decimal transferAmount { get; set; }
        public decimal accumulated { get; set; }
        public string subAccount { get; set; } = string.Empty;
        public string referenceCode { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}
