using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionStore.Core.Interfaces
{
    public interface IBankStatementService
    {
        /// <summary>
        /// Quét các giao dịch gần đây để tìm lệnh chuyển khoản khớp với số tiền và nội dung.
        /// </summary>
        /// <param name="expectedAmount">Số tiền cần thanh toán</param>
        /// <param name="description">Mã nội dung (VD: FS12345)</param>
        /// <returns>Thông tin giao dịch nếu tìm thấy, ngược lại null</returns>
        Task<BankTransaction?> VerifyPaymentAsync(decimal expectedAmount, string description);
    }

    public class BankTransaction
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
    }
}
