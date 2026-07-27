using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Settings;
using System.Text.Json.Serialization;

namespace ShopManager.Services
{
    public class SePayBankService : IBankStatementService
    {
        private readonly HttpClient _httpClient;
        private readonly ISystemSettingsService _settingsService;

        public SePayBankService(ISystemSettingsService settingsService)
        {
            _httpClient = new HttpClient();
            _settingsService = settingsService;
        }

        public async Task<BankTransaction?> VerifyPaymentAsync(decimal expectedAmount, string description)
        {
            try
            {
                var sePayToken = await _settingsService.GetSePayTokenAsync();
                if (string.IsNullOrEmpty(sePayToken))
                {
                    // If no token, we can't verify automatically.
                    return null;
                }

                var settings = PaymentSettingsManager.Load();
                
                // SePay API Endpoint to get transactions
                // Documentation: https://docs.sepay.vn/api-lay-danh-sach-giao-dich.html
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {sePayToken}");

                // Fetch recent transactions (last 20 items to be quick)
                var response = await _httpClient.GetAsync("https://my.sepay.vn/userapi/transactions/list?limit=20");
                
                if (!response.IsSuccessStatusCode) return null;

                var data = await response.Content.ReadFromJsonAsync<SePayResponse>();
                if (data == null || data.Transactions == null) return null;

                // Simple matching logic:
                // 1. Description contains the target code (e.g. FS12345)
                // 2. Amount matches (or exceeds)
                // 3. Happened in the last 24 hours (safety check)
                var match = data.Transactions.FirstOrDefault(t => 
                    t.Content.Contains(description, StringComparison.OrdinalIgnoreCase) && 
                    decimal.TryParse(t.AmountIn, out decimal amt) && amt >= expectedAmount &&
                    DateTime.TryParse(t.TransactionDate, out DateTime dt) && dt >= DateTime.Now.AddDays(-1));

                if (match != null)
                {
                    decimal.TryParse(match.AmountIn, out decimal amt);
                    DateTime.TryParse(match.TransactionDate, out DateTime dt);
                    return new BankTransaction
                    {
                        Id = match.Id,
                        Amount = amt,
                        Description = match.Content,
                        TransactionDate = dt,
                        ReferenceCode = match.ReferenceNumber
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SePay verification failed: {ex.Message}");
            }

            return null;
        }

        // Internal DTOs for SePay API
        private class SePayResponse
        {
            public List<SePayTransaction>? Transactions { get; set; }
        }

        private class SePayTransaction
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("transaction_content")]
            public string Content { get; set; } = string.Empty;

            [JsonPropertyName("amount_in")]
            public string AmountIn { get; set; } = "0";

            [JsonPropertyName("transaction_date")]
            public string TransactionDate { get; set; } = string.Empty;

            [JsonPropertyName("reference_number")]
            public string ReferenceNumber { get; set; } = string.Empty;
        }
    }
}
