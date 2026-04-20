using FashionStore.Core.Interfaces;
using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IFinanceService _financeService;

        public DashboardController(
            IInvoiceService invoiceService,
            IProductService productService,
            ICustomerService customerService,
            IFinanceService financeService)
        {
            _invoiceService = invoiceService;
            _productService = productService;
            _customerService = customerService;
            _financeService = financeService;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceService.SearchInvoicesAsync(null, null, null, "");
            var products = await _productService.GetAllProductsWithCategoriesAsync();
            var customers = await _customerService.GetAllCustomersAsync();

            ViewBag.TotalRevenue = invoices.Sum(i => i.Total);
            ViewBag.TotalInvoices = invoices.Count();
            ViewBag.TotalProducts = products.Count();
            ViewBag.TotalCustomers = customers.Count();

            // Financial Metrics for current month
            var firstOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var now = DateTime.Now;
            
            decimal grossProfit = await _invoiceService.GetProfitAsync(firstOfMonth, now);
            decimal totalExpenses = await _financeService.GetTotalExpensesAsync(firstOfMonth, now);
            
            ViewBag.MonthlyCost = await _invoiceService.GetCostAsync(firstOfMonth, now);
            ViewBag.MonthlyExpenses = totalExpenses;
            ViewBag.MonthlyNetProfit = grossProfit - totalExpenses;
            ViewBag.ProfitMargin = (ViewBag.TotalRevenue > 0) ? (ViewBag.MonthlyNetProfit / ViewBag.TotalRevenue * 100) : 0;

            // Analytics data for charts
            var last30Days = DateTime.Now.AddDays(-30);
            var today = DateTime.Now;
            
            var tempRevenue = await _invoiceService.GetRevenueByDayAsync(last30Days, today);
            var revenueByDay = tempRevenue.Select(d => new { Date = d.Day.ToString("yyyy-MM-dd"), Revenue = d.Revenue }).ToList();
            
            // Force a test point to verify rendering
            revenueByDay.Add(new { Date = DateTime.Now.ToString("yyyy-MM-dd"), Revenue = 5000000m });
            var revenueByCategory = (await _invoiceService.GetRevenueByCategoryAsync(last30Days, today))
                .Select(c => new { Category = c.CategoryName, Revenue = c.Revenue });
            var topProducts = (await _invoiceService.GetTopProductsAsync(last30Days, today, 5))
                .Select(p => new { Name = p.ProductName, Qty = p.Quantity });

            ViewBag.RevenueByDayJson = System.Text.Json.JsonSerializer.Serialize(revenueByDay);
            ViewBag.RevenueByCategoryJson = System.Text.Json.JsonSerializer.Serialize(revenueByCategory);
            ViewBag.TopProductsJson = System.Text.Json.JsonSerializer.Serialize(topProducts);

            // Last 5 invoices
            ViewBag.RecentInvoices = invoices.OrderByDescending(i => i.CreatedDate).Take(5);

            return View();
        }
    }
}
