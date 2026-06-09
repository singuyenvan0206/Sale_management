using ShopManager.Core.Models;

namespace ShopManager.Core.Interfaces
{
    public interface IVoucherRepository : IRepository<Voucher>
    {
        Task<Voucher?> GetByCodeAsync(string code);
        Task<int> GetUsageCountForCustomerAsync(int voucherId, int customerId);
        Task<bool> IncrementUsedCountAsync(int voucherId);
    }
}
