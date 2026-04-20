using FashionStore.Core.Models;

namespace FashionStore.Services
{
    public interface IInvoiceService
    {
        Task<bool> SaveInvoiceAsync(Invoice invoice, int? voucherId);
        Task<IEnumerable<Invoice>> SearchInvoicesAsync(DateTime? from, DateTime? to, int? customerId, string search, string? status = null, string? sortBy = null, bool isDescending = true);
        Task<(Invoice Header, List<InvoiceItem> Items)> GetInvoiceDetailsAsync(int invoiceId);
        Task<decimal> GetRevenueAsync(DateTime from, DateTime to);
        Task<decimal> GetCostAsync(DateTime from, DateTime to);
        Task<decimal> GetProfitAsync(DateTime from, DateTime to);
        Task<int> GetInvoiceCountAsync(DateTime from, DateTime to);
        Task<IEnumerable<(DateTime Day, decimal Revenue)>> GetRevenueByDayAsync(DateTime from, DateTime to);
        Task<IEnumerable<(string CategoryName, decimal Revenue)>> GetRevenueByCategoryAsync(DateTime from, DateTime to);
        Task<IEnumerable<(string ProductName, int Quantity)>> GetTopProductsAsync(DateTime from, DateTime to, int limit = 5);
        Task<bool> DeleteInvoiceAsync(int invoiceId);
        Task<bool> RefundInvoiceAsync(int invoiceId);
        Task<bool> ExportInvoicesToCsvAsync(string filePath);
        Task<int> ImportInvoicesFromCsvAsync(string filePath);
        Task<bool> DeleteAllInvoicesAsync();
    }
}
