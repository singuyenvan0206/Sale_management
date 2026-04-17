using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using FashionStore.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICustomerService _customerService;

        public InvoiceService(IInvoiceRepository invoiceRepository, ICustomerService customerService)
        {
            _invoiceRepository = invoiceRepository;
            _customerService = customerService;
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

        public async Task<decimal> GetCostAsync(DateTime from, DateTime to)
        {
            return await _invoiceRepository.GetTotalCostAsync(from, to);
        }

        public async Task<decimal> GetProfitAsync(DateTime from, DateTime to)
        {
            return await _invoiceRepository.GetTotalProfitAsync(from, to);
        }

        public async Task<int> GetInvoiceCountAsync(DateTime from, DateTime to)
        {
            return await _invoiceRepository.GetInvoiceCountAsync(from, to);
        }

        public async Task<IEnumerable<(DateTime Day, decimal Revenue)>> GetRevenueByDayAsync(DateTime from, DateTime to)
        {
            var data = (await _invoiceRepository.GetRevenueByDayAsync(from, to)).ToDictionary(x => x.Day.Date, x => x.Revenue);
            var result = new List<(DateTime Day, decimal Revenue)>();
            for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
            {
                result.Add((date, data.ContainsKey(date) ? data[date] : 0m));
            }
            return result;
        }

        public async Task<IEnumerable<(string CategoryName, decimal Revenue)>> GetRevenueByCategoryAsync(DateTime from, DateTime to)
        {
            return await _invoiceRepository.GetRevenueByCategoryAsync(from, to);
        }

        public async Task<IEnumerable<(string ProductName, int Quantity)>> GetTopProductsAsync(DateTime from, DateTime to, int limit = 5)
        {
            return await _invoiceRepository.GetTopProductsAsync(from, to, limit);
        }

        public async Task<bool> DeleteInvoiceAsync(int invoiceId)
        {
            return await _invoiceRepository.DeleteInvoiceAsync(invoiceId);
        }

        public async Task<bool> RefundInvoiceAsync(int invoiceId)
        {
            return await _invoiceRepository.RefundInvoiceAsync(invoiceId);
        }

        public async Task<bool> ExportInvoicesToCsvAsync(string filePath)
        {
            try
            {
                // Single JOIN query — O(1) round-trips
                var allData = await _invoiceRepository.GetAllInvoicesWithItemsAsync();

                var lines = new System.Collections.Generic.List<string>(allData.Count * 2 + 1)
                {
                    "Mã HĐ,Ngày tạo,Tên khách hàng,SĐT khách,Email khách,Địa chỉ,Mã SP,Tên sản phẩm,Đơn giá,Số lượng,Thành tiền,Tiền hàng (HĐ),Thuế,Giảm giá,Tổng tiền,Đã trả"
                };

                foreach (var entry in allData)
                {
                    var header = entry.Header;
                    var items = entry.Items;

                    string date = header.CreatedDate.ToString("dd/MM/yyyy HH:mm");
                    string custName = CsvHelper.Escape(header.CustomerName);
                    string custPhone = CsvHelper.Escape(header.CustomerPhone);
                    string custEmail = CsvHelper.Escape(header.CustomerEmail);
                    string custAddr = CsvHelper.Escape(header.CustomerAddress);
                    string subtotal = header.Subtotal.ToString("F0");
                    string tax = header.TaxAmount.ToString("F0");
                    string discount = header.Discount.ToString("F0");
                    string total = header.Total.ToString("F0");
                    string paid = header.Paid.ToString("F0");

                    if (items == null || items.Count == 0)
                    {
                        lines.Add($"{header.Id},{date},{custName},{custPhone},{custEmail},{custAddr},,,,,," +
                                  $"{subtotal},{tax},{discount},{total},{paid}");
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            string prodName = CsvHelper.Escape(item.ProductName);
                            lines.Add($"{header.Id},{date},{custName},{custPhone},{custEmail},{custAddr}," +
                                      $"{item.ProductId},{prodName},{item.UnitPrice:F0},{item.Quantity},{item.LineTotal:F0}," +
                                      $"{subtotal},{tax},{discount},{total},{paid}");
                        }
                    }
                }

                await System.IO.File.WriteAllLinesAsync(filePath, lines, new System.Text.UTF8Encoding(true));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> ImportInvoicesFromCsvAsync(string filePath)
        {
            try
            {
                var allLines = await System.IO.File.ReadAllLinesAsync(filePath, System.Text.Encoding.UTF8);
                if (allLines.Length < 2) return 0;

                var invoiceRows = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string[]>>();
                foreach (var line in allLines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = CsvHelper.ParseLine(line);
                    if (cols.Length < 16) continue;
                    if (!int.TryParse(cols[0], out int invId)) continue;
                    if (!invoiceRows.ContainsKey(invId)) invoiceRows[invId] = new();
                    invoiceRows[invId].Add(cols);
                }

                var invoicesToImport = new System.Collections.Generic.List<Invoice>();
                var customerIdCache = new System.Collections.Generic.Dictionary<string, int>();

                foreach (var kv in invoiceRows)
                {
                    var rows = kv.Value;
                    var first = rows[0];

                    if (!DateTime.TryParseExact(first[1], "dd/MM/yyyy HH:mm",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out DateTime createdDate))
                        createdDate = DateTime.Now;

                    decimal.TryParse(first[11], out decimal subtotal);
                    decimal.TryParse(first[12], out decimal taxAmount);
                    decimal.TryParse(first[13], out decimal discount);
                    decimal.TryParse(first[14], out decimal total);
                    decimal.TryParse(first[15], out decimal paid);

                    var items = new System.Collections.Generic.List<InvoiceItem>();
                    foreach (var row in rows)
                    {
                        if (!int.TryParse(row[6], out int pid) || pid <= 0) continue;
                        int.TryParse(row[9], out int qty);
                        decimal.TryParse(row[8], out decimal unitPrice);
                        if (qty > 0)
                        {
                            items.Add(new InvoiceItem
                            {
                                ProductId = pid,
                                Quantity = qty,
                                UnitPrice = unitPrice,
                                LineTotal = qty * unitPrice,
                                EmployeeId = 1
                            });
                        }
                    }
                    if (items.Count == 0) continue;

                    string custName = first[2].Trim('"');
                    string custPhone = first[3].Trim('"');
                    string custEmail = first[4].Trim('"');
                    string custAddr = first[5].Trim('"');

                    // Simple local cache for customer lookups during this import
                    string cacheKey = $"{custName}|{custPhone}";
                    if (!customerIdCache.TryGetValue(cacheKey, out int customerId))
                    {
                        customerId = await _customerService.GetOrCreateCustomerIdAsync(custName, custPhone, custEmail, custAddr);
                        customerIdCache[cacheKey] = customerId;
                    }

                    invoicesToImport.Add(new Invoice
                    {
                        CustomerId = customerId,
                        EmployeeId = 1,
                        Subtotal = subtotal,
                        TaxPercent = subtotal > 0 ? Math.Round(taxAmount / subtotal * 100, 2) : 0,
                        TaxAmount = taxAmount,
                        Discount = discount,
                        Total = total,
                        Paid = paid,
                        CreatedDate = createdDate,
                        Items = items
                    });
                }

                return await _invoiceRepository.BulkImportInvoicesAsync(invoicesToImport);
            }
            catch
            {
                return -1;
            }
        }

        public async Task<bool> DeleteAllInvoicesAsync()
        {
            return await _invoiceRepository.DeleteAllInvoicesAsync();
        }

        #region Legacy Static Bridge - SHOULD BE DEPRECATED

        private static IInvoiceService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<IInvoiceService>() ?? throw new InvalidOperationException("DI not initialized");

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

        public static decimal GetCostBetween(DateTime from, DateTime to)
            => RunSync(() => GetService().GetCostAsync(from, to));

        public static decimal GetProfitBetween(DateTime from, DateTime to)
            => RunSync(() => GetService().GetProfitAsync(from, to));

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

        public static bool DeleteInvoice(int id)
            => RunSync(() => GetService().DeleteInvoiceAsync(id));

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
                CustomerAddress = res.Header.CustomerAddress,
                PaymentMethod = res.Header.PaymentMethod
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
            public string PaymentMethod { get; set; } = "Cash";
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
