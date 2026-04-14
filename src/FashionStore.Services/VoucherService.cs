using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace FashionStore.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;

        public VoucherService(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<IEnumerable<Voucher>> GetAllVouchersAsync()
        {
            return await _voucherRepository.GetAllAsync();
        }

        public async Task<Voucher?> GetVoucherByIdAsync(int id)
        {
            return await _voucherRepository.GetByIdAsync(id);
        }

        public async Task<Voucher?> GetVoucherByCodeAsync(string code)
        {
            return await _voucherRepository.GetByCodeAsync(code);
        }

        public async Task<bool> AddVoucherAsync(Voucher voucher)
        {
            if (string.IsNullOrWhiteSpace(voucher.Code)) return false;
            return await _voucherRepository.AddAsync(voucher);
        }

        public async Task<bool> UpdateVoucherAsync(Voucher voucher)
        {
            if (voucher.Id <= 0 || string.IsNullOrWhiteSpace(voucher.Code)) return false;
            return await _voucherRepository.UpdateAsync(voucher);
        }

        public async Task<bool> DeleteVoucherAsync(int id)
        {
            return await _voucherRepository.DeleteAsync(id);
        }

        public async Task<int> GetVoucherUsageCountForCustomerAsync(int voucherId, int customerId)
        {
            return await _voucherRepository.GetUsageCountForCustomerAsync(voucherId, customerId);
        }

        public async Task<bool> MarkVoucherAsUsedAsync(int voucherId)
        {
            return await _voucherRepository.IncrementUsedCountAsync(voucherId);
        }

        #region Legacy Static Bridge - SHOULD BE DEPRECATED

        private static IVoucherService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<IVoucherService>() ?? throw new InvalidOperationException("DI not initialized");

        // Use Task.Run to avoid deadlock when called from UI thread with .GetAwaiter().GetResult()
        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();
        private static void RunSync(Func<Task> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<Voucher> GetAllVouchers()
            => RunSync(() => GetService().GetAllVouchersAsync()).ToList();

        public static bool AddVoucher(Voucher voucher)
            => RunSync(() => GetService().AddVoucherAsync(voucher));

        public static bool UpdateVoucher(Voucher voucher)
            => RunSync(() => GetService().UpdateVoucherAsync(voucher));

        public static bool DeleteVoucher(int id)
            => RunSync(() => GetService().DeleteVoucherAsync(id));

        public static Voucher? GetVoucherByCode(string code)
            => RunSync(() => GetService().GetVoucherByCodeAsync(code));

        public static void UpdateVoucherUsage(int voucherId, MySqlConnection connection, MySqlTransaction tx)
        {
            // Direct DB operation for transaction context - still used by InvoiceService SaveInvoice
            string sql = "UPDATE Vouchers SET UsedCount = UsedCount + 1 WHERE Id = @Id";
            using var cmd = new MySqlCommand(sql, connection, tx);
            cmd.Parameters.AddWithValue("@Id", voucherId);
            cmd.ExecuteNonQuery();
        }

        public static int GetVoucherUsageCountForCustomer(int voucherId, int customerId)
            => RunSync(() => GetService().GetVoucherUsageCountForCustomerAsync(voucherId, customerId));

        #endregion
    }
}
