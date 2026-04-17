using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
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

        public InventoryController(
            IProductRepository productRepository,
            IStockMovementRepository stockRepository,
            ISupplierRepository supplierRepository)
        {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _supplierRepository = supplierRepository;
        }

        public async Task<IActionResult> History()
        {
            var movements = await _stockRepository.GetLatestMovementsAsync(100);
            var products = await _productRepository.GetAllWithCategoriesAsync();
            
            ViewBag.Products = products.ToDictionary(p => p.Id, p => p.Name);
            return View(movements);
        }

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
                return BadRequest("No items to import");
            }

            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    int prevStock = product.StockQuantity;
                    product.StockQuantity += item.Quantity;
                    
                    // Update product stock
                    await _productRepository.UpdateAsync(product);

                    // Log movement
                    var movement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        MovementType = "Import",
                        Quantity = item.Quantity,
                        PreviousStock = prevStock,
                        NewStock = product.StockQuantity,
                        ReferenceType = "PurchaseOrder",
                        Notes = $"Imported from Supplier ID: {request.SupplierId}. Notes: {request.Notes}",
                        EmployeeId = 1, // Default Admin
                        CreatedDate = DateTime.Now
                    };
                    await _stockRepository.AddAsync(movement);
                }
            }

            return Ok(new { success = true });
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
    }
}
