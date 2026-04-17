using FashionStore.Core.Interfaces;
using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to, string search = "")
        {
            var invoices = await _invoiceService.SearchInvoicesAsync(from, to, null, search);
            return View(invoices);
        }

        public async Task<IActionResult> Details(int id)
        {
            var (header, items) = await _invoiceService.GetInvoiceDetailsAsync(id);
            if (header == null) return NotFound();
            
            ViewBag.Items = items;
            return View(header);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {
            string fileName = $"hoadon_{DateTime.Now:yyyyMMddHHmm}.csv";
            string filePath = Path.Combine(Path.GetTempPath(), fileName);
            
            bool success = await _invoiceService.ExportInvoicesToCsvAsync(filePath);
            if (!success) return BadRequest("Lỗi khi xuất file CSV");

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "text/csv", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn file CSV");

            string filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            int count = await _invoiceService.ImportInvoicesFromCsvAsync(filePath);
            if (count < 0) return BadRequest("Lỗi khi nhập file CSV. Vui lòng kiểm tra định dạng.");

            TempData["Success"] = $"Đã nhập thành công {count} hoá đơn.";
            return RedirectToAction(nameof(Index));
        }
    }
}
