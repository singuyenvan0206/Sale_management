using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Models;

namespace FashionStore.Data.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<int> GetNextInvoiceIdAsync();
        Task<bool> SaveInvoiceAsync(Invoice invoice, int? voucherId);
        Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime from, DateTime to);
        Task<IEnumerable<Invoice>> SearchInvoicesAsync(DateTime? from, DateTime? to, int? customerId, string search);
        Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
        Task<int> GetInvoiceCountAsync(DateTime from, DateTime to);
        Task<(Invoice Header, List<InvoiceItem> Items)> GetInvoiceDetailsAsync(int invoiceId);
        Task<bool> DeleteAllInvoicesAsync();
    }
}
