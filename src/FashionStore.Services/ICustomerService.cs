using FashionStore.Core.Models;

namespace FashionStore.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer?> GetCustomerByPhoneAsync(string phone);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string? query = null, string? sortBy = null, bool isDescending = false);
        Task<bool> AddCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<bool> UpdateLoyaltyAsync(int customerId, int points, string customerType);
        Task<int> GetOrCreateCustomerIdAsync(string name, string phone, string email, string address);
        Task<IEnumerable<(string Name, decimal TotalSpent)>> GetTopCustomersAsync(int topN);
        Task<int> RefreshAllLoyaltyAsync(decimal spendPerPoint, int silverMin, int goldMin, int vipMin);
        Task<bool> ExportCustomersToCsvAsync(string filePath);
        Task<int> ImportCustomersFromCsvAsync(string filePath);
        Task<IEnumerable<(int InvoiceId, DateTime CreatedAt, int ItemCount, decimal Total)>> GetCustomerPurchaseHistoryAsync(int customerId);
    }
}
