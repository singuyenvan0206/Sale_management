using FashionStore.Core.Models;

namespace FashionStore.Core.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<int> GetNextInvoiceIdAsync();
        Task<bool> SaveInvoiceAsync(Invoice invoice, int? voucherId);
        Task<int> BulkImportInvoicesAsync(List<Invoice> invoices);
        Task<List<(Invoice Header, List<InvoiceItem> Items)>> GetAllInvoicesWithItemsAsync();
        Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime from, DateTime to);
        Task<IEnumerable<Invoice>> SearchInvoicesAsync(DateTime? from, DateTime? to, int? customerId, string search, string? status = null, string? sortBy = null, bool isDescending = true);
        Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalCostAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalProfitAsync(DateTime from, DateTime to);
        Task<int> GetInvoiceCountAsync(DateTime from, DateTime to);
        Task<(Invoice Header, List<InvoiceItem> Items)> GetInvoiceDetailsAsync(int invoiceId);
        Task<IEnumerable<(DateTime Day, decimal Revenue)>> GetRevenueByDayAsync(DateTime from, DateTime to);
        Task<IEnumerable<(string CategoryName, decimal Revenue)>> GetRevenueByCategoryAsync(DateTime from, DateTime to);
        Task<IEnumerable<(string ProductName, int Quantity)>> GetTopProductsAsync(DateTime from, DateTime to, int limit = 5);
        Task<bool> DeleteInvoiceAsync(int invoiceId);
        Task<bool> RefundInvoiceAsync(int invoiceId);
        Task<bool> DeleteAllInvoicesAsync();
    }
}
