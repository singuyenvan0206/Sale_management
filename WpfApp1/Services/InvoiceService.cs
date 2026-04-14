using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FashionStore.Data.Interfaces;
using FashionStore.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<bool> SaveInvoiceAsync(Invoice invoice, int? voucherId)
        {
            if (invoice.Id == 0)
            {
                invoice.Id = await _invoiceRepository.GetNextInvoiceIdAsync();
            }
            return await _invoiceRepository.SaveInvoiceAsync(invoice, voucherId);
        }

        public async Task<IEnumerable<Invoice>> SearchInvoicesAsync(DateTime? from, DateTime? to, int? customerId, string search)
        {
            return await _invoiceRepository.SearchInvoicesAsync(from, to, customerId, search);
        }

        public async Task<(Invoice Header, List<InvoiceItem> Items)> GetInvoiceDetailsAsync(int invoiceId)
        {
            return await _invoiceRepository.GetInvoiceDetailsAsync(invoiceId);
        }

        public async Task<decimal> GetRevenueAsync(DateTime from, DateTime to)
        {
            return await _invoiceRepository.GetTotalRevenueAsync(from, to);
        }

        public async Task<int> GetInvoiceCountAsync(DateTime from, DateTime to)
        {
            return await _invoiceRepository.GetInvoiceCountAsync(from, to);
        }

        public async Task<IEnumerable<(DateTime Day, decimal Revenue)>> GetRevenueByDayAsync(DateTime from, DateTime to)
        {
            return await _invoiceRepository.GetRevenueByDayAsync(from, to);
        }

        public async Task<IEnumerable<(string CategoryName, decimal Revenue)>> GetRevenueByCategoryAsync(DateTime from, DateTime to)
        {
            return await _invoiceRepository.GetRevenueByCategoryAsync(from, to);
        }

        public async Task<bool> ExportInvoicesToCsvAsync(string filePath)
        {
            try
            {
                var invoices = await _invoiceRepository.SearchInvoicesAsync(null, null, null, "");
                var lines = new System.Collections.Generic.List<string>
                {
                    "Mã HĐ,Ngày tạo,Tên khách hàng,Tiền hàng,Thuế,Giảm giá,Tổng tiền,Đã trả"
                };
                foreach (var inv in invoices)
                {
                    string date = inv.CreatedDate.ToString("dd/MM/yyyy HH:mm");
                    string custName = (inv.CustomerName ?? "").Replace(",", " ");
                    lines.Add($"{inv.Id},{date},{custName},{inv.Subtotal},{inv.TaxAmount},{inv.Discount},{inv.Total},{inv.Paid}");
                }
                await System.IO.File.WriteAllLinesAsync(filePath, lines, System.Text.Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> ImportInvoicesFromCsvAsync(string filePath)
        {
            return 0;
        }

        public async Task<bool> DeleteAllInvoicesAsync()
        {
            return await _invoiceRepository.DeleteAllInvoicesAsync();
        }

        #region Legacy Static Bridge - SHOULD BE DEPRECATED

        private static IInvoiceService GetService() => App.ServiceProvider?.GetRequiredService<IInvoiceService>() ?? throw new InvalidOperationException("DI not initialized");

        // Safe sync-over-async: runs on thread pool to avoid UI thread SynchronizationContext deadlock
        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();
        private static void RunSync(Func<Task> func) => Task.Run(func).GetAwaiter().GetResult();

        public static bool SaveInvoice(int customerId, int employeeId, decimal subtotal, decimal taxPercent, decimal taxAmount, decimal discount, decimal total, decimal paid, List<(int ProductId, int Quantity, decimal UnitPrice)> items, DateTime? createdDate = null, int? invoiceId = null, int? voucherId = null)
        {
            var invoice = new Invoice
            {
                Id = invoiceId ?? 0,
                CustomerId = customerId,
                EmployeeId = employeeId,
                Subtotal = subtotal,
                TaxPercent = taxPercent,
                TaxAmount = taxAmount,
                Discount = discount,
                Total = total,
                Paid = paid,
                CreatedDate = createdDate ?? DateTime.Now,
                Items = items.Select(i => new InvoiceItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice,
                    EmployeeId = employeeId
                }).ToList()
            };
            bool success = RunSync(() => GetService().SaveInvoiceAsync(invoice, voucherId));
            if (success) LastSavedInvoiceId = invoice.Id;
            return success;
        }

        public static decimal GetRevenueBetween(DateTime from, DateTime to)
            => RunSync(() => GetService().GetRevenueAsync(from, to));

        public static int GetInvoiceCountBetween(DateTime from, DateTime to)
            => RunSync(() => GetService().GetInvoiceCountAsync(from, to));

        public static List<(int Id, DateTime CreatedDate, string CustomerName, decimal Subtotal, decimal TaxAmount, decimal Discount, decimal Total, decimal Paid)>
            QueryInvoices(DateTime? from, DateTime? to, int? customerId, string search)
            => RunSync(() => GetService().SearchInvoicesAsync(from, to, customerId, search))
               .Select(i => (i.Id, i.CreatedDate, i.CustomerName, i.Subtotal, i.TaxAmount, i.Discount, i.Total, i.Paid)).ToList();

        public static int GetTotalInvoices()
            => RunSync(() => GetService().GetInvoiceCountAsync(DateTime.MinValue, DateTime.MaxValue));

        public static decimal GetTotalRevenue()
            => RunSync(() => GetService().GetRevenueAsync(DateTime.MinValue, DateTime.MaxValue));

        public static (DateTime? oldestDate, DateTime? newestDate) GetInvoiceDateRange()
            => (DateTime.Now.AddMonths(-1), DateTime.Now); // Placeholder

        public static int ImportInvoicesFromCsv(string filePath)
            => RunSync(() => GetService().ImportInvoicesFromCsvAsync(filePath));

        public static bool ExportInvoicesToCsv(string filePath)
            => RunSync(() => GetService().ExportInvoicesToCsvAsync(filePath));

        public static List<(DateTime Day, decimal Total)> GetRevenueByDay(DateTime from, DateTime to)
            => RunSync(() => GetService().GetRevenueByDayAsync(from, to))
               .Select(r => (r.Day, r.Revenue)).ToList();

        public static List<(string Name, decimal Total)> GetRevenueByCategory(DateTime from, DateTime to, int limit = 10)
            => RunSync(() => GetService().GetRevenueByCategoryAsync(from, to))
               .Select(r => (r.CategoryName, r.Revenue)).Take(limit).ToList();

        public static List<(string Name, int Qty)> GetTopProducts(int limit = 10)
            => new(); // Placeholder

        public static bool DeleteInvoice(int id) => false; // Placeholder

        public static bool DeleteAllInvoices()
            => RunSync(() => GetService().DeleteAllInvoicesAsync());

        public static int LastSavedInvoiceId { get; set; }

        public static (InvoiceHeader Header, List<InvoiceItemDetail> Items) GetInvoiceDetails(int invoiceId)
        {
            var res = RunSync(() => GetService().GetInvoiceDetailsAsync(invoiceId));
            if (res.Header == null) return (null!, null!);

            var header = new InvoiceHeader
            {
                Id = res.Header.Id,
                CreatedDate = res.Header.CreatedDate,
                Subtotal = res.Header.Subtotal,
                TaxPercent = res.Header.TaxPercent,
                TaxAmount = res.Header.TaxAmount,
                DiscountAmount = res.Header.Discount,
                Total = res.Header.Total,
                Paid = res.Header.Paid,
                EmployeeId = res.Header.EmployeeId,
                CustomerName = res.Header.CustomerName,
                CustomerPhone = res.Header.CustomerPhone,
                CustomerEmail = res.Header.CustomerEmail,
                CustomerAddress = res.Header.CustomerAddress
            };

            var invoiceItems = res.Items?.Select(i => new InvoiceItemDetail
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal
            }).ToList() ?? new List<InvoiceItemDetail>();

            return (header, invoiceItems);
        }

        public class InvoiceHeader
        {
            public int Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public decimal Subtotal { get; set; }
            public decimal TaxPercent { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal Total { get; set; }
            public decimal Paid { get; set; }
            public string CustomerPhone { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public string CustomerAddress { get; set; } = string.Empty;
            public int EmployeeId { get; set; }
        }

        public class InvoiceItemDetail
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal { get; set; }
        }

        #endregion
    }
}
