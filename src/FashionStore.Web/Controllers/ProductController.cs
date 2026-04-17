using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;

        public ProductController(
            IProductService productService, 
            ICategoryService categoryService,
            ISupplierService supplierService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var pagedProducts = await _productService.GetPagedProductsAsync(page, pageSize);
            return View(pagedProducts);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Suppliers = await _supplierService.GetAllSuppliersAsync();
            return View(new Product());
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productService.AddProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Suppliers = await _supplierService.GetAllSuppliersAsync();
            return View(product);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Suppliers = await _supplierService.GetAllSuppliersAsync();
            return View(product);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productService.UpdateProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Suppliers = await _supplierService.GetAllSuppliersAsync();
            return View(product);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            
            // Load variants
            product.Variants = (await _productService.GetProductVariantsAsync(id)).ToList();
            
            return View(product);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> AddVariant([FromBody] ProductVariant variant)
        {
            if (variant == null || variant.ProductId == 0) return BadRequest("Invalid variant data");
            
            bool success = await _productService.AddVariantAsync(variant);
            if (success) return Ok(variant);
            
            return StatusCode(500, "Failed to add variant");
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> UpdateVariant([FromBody] ProductVariant variant)
        {
            if (variant == null || variant.Id == 0) return BadRequest("Invalid variant data");
            
            bool success = await _productService.UpdateVariantAsync(variant);
            if (success) return Ok(variant);
            
            return StatusCode(500, "Failed to update variant");
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {
            string fileName = $"sanpham_{DateTime.Now:yyyyMMddHHmm}.csv";
            string filePath = Path.Combine(Path.GetTempPath(), fileName);
            
            bool success = await _productService.ExportProductsToCsvAsync(filePath);
            if (!success) return BadRequest("Lỗi khi xuất file CSV");

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "text/csv", fileName);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn file CSV");

            string filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            int count = await _productService.ImportProductsFromCsvAsync(filePath);
            if (count < 0) return BadRequest("Lỗi khi nhập file CSV. Vui lòng kiểm tra định dạng.");

            TempData["Success"] = $"Đã nhập thành công {count} sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteVariant(int id)
        {
            bool success = await _productService.DeleteVariantAsync(id);
            if (success) return Ok();
            
            return StatusCode(500, "Failed to delete variant");
        }
    }
}
