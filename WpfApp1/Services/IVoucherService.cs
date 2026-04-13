using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Models;

namespace FashionStore.Services
{
    public interface IVoucherService
    {
        Task<IEnumerable<Voucher>> GetAllVouchersAsync();
        Task<Voucher?> GetVoucherByIdAsync(int id);
        Task<Voucher?> GetVoucherByCodeAsync(string code);
        Task<bool> AddVoucherAsync(Voucher voucher);
        Task<bool> UpdateVoucherAsync(Voucher voucher);
        Task<bool> DeleteVoucherAsync(int id);
        Task<int> GetVoucherUsageCountForCustomerAsync(int voucherId, int customerId);
        Task<bool> MarkVoucherAsUsedAsync(int voucherId);
    }
}
