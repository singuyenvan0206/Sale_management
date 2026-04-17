using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new FashionStore.Core.Models.Customer());
        }

        [HttpPost]
        public async Task<IActionResult> Create(FashionStore.Core.Models.Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _customerService.AddCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FashionStore.Core.Models.Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _customerService.UpdateCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {
            string fileName = $"khachhang_{DateTime.Now:yyyyMMddHHmm}.csv";
            string filePath = Path.Combine(Path.GetTempPath(), fileName);
            
            bool success = await _customerService.ExportCustomersToCsvAsync(filePath);
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

            int count = await _customerService.ImportCustomersFromCsvAsync(filePath);
            if (count < 0) return BadRequest("Lỗi khi nhập file CSV. Vui lòng kiểm tra định dạng.");

            TempData["Success"] = $"Đã nhập thành công {count} khách hàng.";
            return RedirectToAction(nameof(Index));
        }
    }
}
