using FashionStore.Core.Models;

namespace FashionStore.Core.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<IEnumerable<Customer>> SearchAsync(string query);
        Task<bool> UpdateLoyaltyAsync(int customerId, int points, string customerType);
        Task<IEnumerable<(string Name, decimal TotalSpent)>> GetTopCustomersAsync(int topN);
        Task<bool> HasInvoicesAsync(int customerId);
        Task<int> RefreshAllLoyaltyAsync(decimal spendPerPoint, int silverMin, int goldMin, int vipMin);
    }
}
