using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Settings;

namespace FashionStore.Services
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
                var response = await _httpClient.GetAsync("https://api.sepay.vn/user/bank/transactions?limit=20");
                
                if (!response.IsSuccessStatusCode) return null;

                var data = await response.Content.ReadFromJsonAsync<SePayResponse>();
                if (data == null || data.Transactions == null) return null;

                // Simple matching logic:
                // 1. Description contains the target code (e.g. FS12345)
                // 2. Amount matches (or exceeds)
                // 3. Happened in the last 24 hours (safety check)
                var match = data.Transactions.FirstOrDefault(t => 
                    t.Content.Contains(description, StringComparison.OrdinalIgnoreCase) && 
                    t.AmountIn >= expectedAmount &&
                    t.TransactionDate >= DateTime.Now.AddDays(-1));

                if (match != null)
                {
                    return new BankTransaction
                    {
                        Id = match.Id.ToString(),
                        Amount = match.AmountIn,
                        Description = match.Content,
                        TransactionDate = match.TransactionDate,
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
            public long Id { get; set; }
            public string Content { get; set; } = string.Empty;
            public decimal AmountIn { get; set; }
            public DateTime TransactionDate { get; set; }
            public string ReferenceNumber { get; set; } = string.Empty;
        }
    }
}
