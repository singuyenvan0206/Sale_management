using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Models;

namespace FashionStore.Data.Interfaces
{
    public interface IVoucherRepository : IRepository<Voucher>
    {
        Task<Voucher?> GetByCodeAsync(string code);
        Task<int> GetUsageCountForCustomerAsync(int voucherId, int customerId);
        Task<bool> IncrementUsedCountAsync(int voucherId);
    }
}
