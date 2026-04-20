using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class InventoryController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IPurchaseOrderService _poService;

        public InventoryController(
            IProductRepository productRepository,
            IStockMovementRepository stockRepository,
            ISupplierRepository supplierRepository,
            IPurchaseOrderService poService)
        {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _supplierRepository = supplierRepository;
            _poService = poService;
        }

        public async Task<IActionResult> Index(string tab = "history")
        {
            var products = await _productRepository.GetAllWithCategoriesAsync();
            ViewBag.ProductsMap = products.ToDictionary(p => p.Id, p => p.Name);
            ViewBag.Suppliers = await _supplierRepository.GetAllAsync();
            ViewBag.AllProducts = products;
            ViewBag.CurrentTab = tab;

            if (tab == "po")
            {
                var orders = await _poService.GetAllOrdersAsync();
                return View("Index", orders);
            }
            else // Default to history
            {
                var movements = await _stockRepository.GetLatestMovementsAsync(100);
                return View("Index", movements);
            }
        }

        public async Task<IActionResult> History() => RedirectToAction(nameof(Index), new { tab = "history" });
        public async Task<IActionResult> PurchaseOrders() => RedirectToAction(nameof(Index), new { tab = "po" });

        [HttpGet]
        public async Task<IActionResult> Import()
        {
            ViewBag.Suppliers = await _supplierRepository.GetAllAsync();
            ViewBag.Products = await _productRepository.GetAllWithCategoriesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessImport([FromBody] StockImportRequest request)
        {
            if (request == null || !request.Items.Any())
            {
                return BadRequest("Không có mặt hàng nào để nhập.");
            }

            if (request.Items.Any(i => i.Quantity <= 0 || i.UnitPrice < 0))
            {
                return BadRequest("Số lượng phải lớn hơn 0 và đơn giá không được âm.");
            }

            // Create a formal Purchase Order for this import
            var po = new PurchaseOrder
            {
                SupplierId = request.SupplierId,
                EmployeeId = 1, // Default Admin
                TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice),
                PaidAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice), // Assumed paid if using quick import
                Status = "Draft",
                Notes = request.Notes,
                Items = request.Items.Select(i => new PurchaseOrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _poService.CreateOrderAsync(po);
            await _poService.ReceiveOrderAsync(po.Id);

            return Ok(new { success = true });
        }


        public async Task<IActionResult> PODetails(int id)
        {
            var order = await _poService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> ReceivePO(int id)
        {
            var success = await _poService.ReceiveOrderAsync(id);
            if (success) TempData["Success"] = "Đã nhận hàng và cập nhật kho.";
            else TempData["Error"] = "Cập nhật thất bại.";
            return RedirectToAction(nameof(PODetails), new { id });
        }
    }

    public class StockImportRequest
    {
        public int SupplierId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<ImportItemRequest> Items { get; set; } = new();
    }

    public class ImportItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
