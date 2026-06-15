using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;
using ShopManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ShopManager.Web.Controllers
{
    [Authorize]
    public class POSController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IInvoiceService _invoiceService;
        private readonly IVoucherService _voucherService;
        private readonly IBankStatementService _bankService;
        private readonly ICalculationService _calcService;
        private readonly ILogger<POSController> _logger;
        private readonly ICategoryService _categoryService;

        public POSController(
            IProductService productService,
            ICustomerService customerService,
            IInvoiceService invoiceService,
            IVoucherService voucherService,
            IBankStatementService bankService,
            ICalculationService calcService,
            ICategoryService categoryService,
            ILogger<POSController> logger)
        {
            _productService = productService;
            _customerService = customerService;
            _invoiceService = invoiceService;
            _voucherService = voucherService;
            _bankService = bankService;
            _calcService = calcService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsWithCategoriesAsync();
            var customers = await _customerService.GetAllCustomersAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            ViewBag.Products = products;
            ViewBag.Customers = customers;
            ViewBag.Categories = categories;
            
            var paymentSettings = ShopManager.Core.Settings.PaymentSettingsManager.Load();
            ViewBag.BankConfig = new {
                code = paymentSettings.BankCode,
                account = paymentSettings.BankAccount,
                name = paymentSettings.AccountHolder
            };
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SearchProduct(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return BadRequest();

            barcode = barcode.Trim();

            // 1. Try search by base product code
            var product = await _productService.GetProductByCodeAsync(barcode);
            if (product != null)
            {
                return Ok(new
                {
                    id = product.Id,
                    name = product.Name,
                    price = product.SalePrice,
                    code = product.Code,
                    taxPercent = product.CategoryTaxPercent
                });
            }

            // 2. Try search by variant barcode
            var variant = await _productService.GetVariantByBarcodeAsync(barcode);
            if (variant != null)
            {
                // Fetch the parent product to get the name and tax
                var parent = await _productService.GetProductByIdAsync(variant.ProductId);
                return Ok(new
                {
                    id = variant.ProductId,
                    name = $"{parent?.Name} ({variant.Size} - {variant.Color})",
                    price = (parent?.SalePrice ?? 0) + variant.PriceAdjustment,
                    code = variant.Barcode,
                    taxPercent = parent?.CategoryTaxPercent ?? 0m
                });
            }

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerHistory(int id)
        {
            if (id <= 0) return BadRequest();
            var history = await _customerService.GetCustomerPurchaseHistoryAsync(id);
            return Ok(history.Take(5).Select(h => new {
                id = h.InvoiceId,
                date = h.CreatedAt.ToString("dd/MM/yyyy"),
                amount = h.Total,
                items = h.ItemCount
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest("Giỏ hàng đang trống.");
            }

            if (request.Items.Any(i => i.Quantity <= 0 || i.UnitPrice < 0))
            {
                return BadRequest("Số lượng sản phẩm phải lớn hơn 0 và đơn giá không được âm.");
            }

            if (request.Total < 0)
            {
                return BadRequest("Tổng số tiền không hợp lệ.");
            }

            try
            {
                var invoice = new Invoice
                {
                    CustomerId = request.CustomerId == 0 ? 1 : request.CustomerId,
                    EmployeeId = 1, // Default Admin for demo
                    Subtotal = request.Subtotal,
                    TaxPercent = request.TaxPercent,
                    TaxAmount = request.TaxAmount,
                    Discount = request.Discount,
                    Total = request.Total,
                    Paid = request.Paid,
                    PaymentMethod = request.PaymentMethod,
                    VoucherId = request.VoucherId,
                    CreatedDate = DateTime.Now,
                    Items = request.Items.Select(i => new InvoiceItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.LineTotal,
                        EmployeeId = 1
                    }).ToList()
                };

                bool success = await _invoiceService.SaveInvoiceAsync(invoice, request.VoucherId);

                if (success)
                {
                    return Ok(new { invoiceId = invoice.Id });
                }

                return StatusCode(500, "Failed to save invoice infrastructure-wise.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerLoyalty(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();

            decimal discountPercent = _calcService.GetTierDiscountPercent(customer.CustomerType);
            return Ok(new { tier = customer.CustomerType, discountPercent });
        }

        [HttpGet]
        public async Task<IActionResult> ValidateVoucher(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return BadRequest();
            
            var voucher = await _voucherService.GetVoucherByCodeAsync(code.Trim());
            if (voucher == null || !voucher.IsActive || (voucher.EndDate < DateTime.Now))
            {
                return NotFound("Voucher không hợp lệ hoặc đã hết hạn.");
            }

            return Ok(new { 
                id = voucher.Id, 
                code = voucher.Code, 
                discountValue = voucher.DiscountValue,
                discountType = voucher.DiscountType
            });
        }

        [HttpGet]
        public async Task<IActionResult> VerifyPayment(string refCode, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(refCode)) return BadRequest();

            _logger.LogInformation("Checking transaction - RefCode: {RefCode}, Amount: {Amount}", refCode, amount);

            try
            {
                var result = await _bankService.VerifyPaymentAsync(amount, refCode);
                if (result != null)
                {
                    _logger.LogInformation("Transaction VERIFIED successfully! - RefCode: {RefCode}, TxId: {TxId}, Amount: {Amount}", refCode, result.Id, result.Amount);
                    return Ok(new { success = true });
                }
                else
                {
                    _logger.LogWarning("Transaction NOT FOUND yet - RefCode: {RefCode}, Amount: {Amount}", refCode, amount);
                    return Ok(new { success = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during payment verification for RefCode: {RefCode}", refCode);
                return StatusCode(500, "Internal error checking payment status");
            }
        }
    }

    public class CheckoutRequest
    {
        public int CustomerId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public decimal Paid { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public int? VoucherId { get; set; }
        public List<CartItemRequest> Items { get; set; } = new();
    }

    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
