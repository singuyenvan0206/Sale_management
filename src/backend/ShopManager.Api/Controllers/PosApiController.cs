using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManager.Core.DTOs;
using ShopManager.Core.Enums;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;
using ShopManager.Services;

namespace ShopManager.Web.Controllers.Api
{
    [ApiController]
    [Route("api/v1/pos")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
    public class PosApiController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly ICategoryService _categoryService;
        private readonly IVoucherService _voucherService;
        private readonly IInvoiceService _invoiceService;
        private readonly ICalculationService _calcService;
        private readonly ILogger<PosApiController> _logger;

        public PosApiController(
            IProductService productService,
            ICustomerService customerService,
            ICategoryService categoryService,
            IVoucherService voucherService,
            IInvoiceService invoiceService,
            ICalculationService calcService,
            ILogger<PosApiController> logger)
        {
            _productService = productService;
            _customerService = customerService;
            _categoryService = categoryService;
            _voucherService = voucherService;
            _invoiceService = invoiceService;
            _calcService = calcService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy dữ liệu khởi tạo cho màn hình POS
        /// </summary>
        [HttpGet("init")]
        public async Task<IActionResult> GetPosInitData()
        {
            try
            {
                var products = await _productService.GetAllProductsWithCategoriesAsync();
                var customers = await _customerService.GetAllCustomersAsync();
                var categories = await _categoryService.GetAllCategoriesAsync();

                var data = new
                {
                    Products = products,
                    Customers = customers,
                    Categories = categories
                };

                return Ok(ApiResponse<object>.Ok(data, "Khởi tạo dữ liệu POS thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khởi tạo POS API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Tính toán giảm giá và tổng tiền hóa đơn POS
        /// </summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateDiscount([FromBody] CalculateDiscountRequest request)
        {
            if (request == null || request.Items == null)
            {
                return BadRequest(ApiResponse.Fail("Dữ liệu yêu cầu không hợp lệ."));
            }

            try
            {
                decimal subtotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
                decimal memberDiscount = 0;
                string? memberTier = null;

                if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId.Value);
                    if (customer != null)
                    {
                        memberTier = customer.CustomerType;
                        decimal discountPercent = _calcService.GetTierDiscountPercent(customer.CustomerType);
                        memberDiscount = _calcService.CalculateTierDiscount(subtotal, discountPercent);
                    }
                }

                decimal voucherDiscount = 0;
                string? voucherMsg = null;
                if (!string.IsNullOrWhiteSpace(request.VoucherCode))
                {
                    var voucher = await _voucherService.GetVoucherByCodeAsync(request.VoucherCode.Trim());
                    if (voucher != null && voucher.IsActive && voucher.StartDate <= DateTime.Now && voucher.EndDate >= DateTime.Now)
                    {
                        if (subtotal >= voucher.MinInvoiceAmount)
                        {
                            voucherDiscount = _calcService.CalculateVoucherValue(subtotal, voucher);
                        }
                        else
                        {
                            voucherMsg = $"Đơn hàng chưa đạt giá trị tối thiểu {voucher.MinInvoiceAmount:N0}đ để áp dụng voucher.";
                        }
                    }
                    else
                    {
                        voucherMsg = "Mã voucher không hợp lệ hoặc đã hết hạn.";
                    }
                }

                decimal totalDiscount = memberDiscount + voucherDiscount;
                decimal finalTotal = Math.Max(0, subtotal - totalDiscount);

                var response = new CalculateDiscountResponse
                {
                    Subtotal = subtotal,
                    MemberDiscount = memberDiscount,
                    VoucherDiscount = voucherDiscount,
                    TotalDiscount = totalDiscount,
                    FinalTotal = finalTotal,
                    MemberTier = memberTier,
                    Message = voucherMsg
                };

                return Ok(ApiResponse<CalculateDiscountResponse>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tính toán giảm giá POS API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi tính toán: {ex.Message}"));
            }
        }

        /// <summary>
        /// Thanh toán & Xuất hóa đơn POS qua API
        /// </summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutApiRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest(ApiResponse.Fail("Giỏ hàng không được trống."));
            }

            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int currentUserId = int.TryParse(userIdClaim, out int uid) ? uid : (request.EmployeeId > 0 ? request.EmployeeId : 1);

                decimal subtotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
                
                // Member discount
                decimal memberDiscount = 0;
                if (request.CustomerId > 0)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);
                    if (customer != null)
                    {
                        decimal discountPercent = _calcService.GetTierDiscountPercent(customer.CustomerType);
                        memberDiscount = _calcService.CalculateTierDiscount(subtotal, discountPercent);
                    }
                }

                // Voucher discount
                decimal voucherDiscount = 0;
                int? voucherId = null;
                if (!string.IsNullOrWhiteSpace(request.VoucherCode))
                {
                    var voucher = await _voucherService.GetVoucherByCodeAsync(request.VoucherCode.Trim());
                    if (voucher != null && voucher.IsActive)
                    {
                        voucherId = voucher.Id;
                        voucherDiscount = _calcService.CalculateVoucherValue(subtotal, voucher);
                    }
                }

                decimal totalDiscount = memberDiscount + voucherDiscount;
                decimal finalTotal = Math.Max(0, subtotal - totalDiscount);

                var invoice = new Invoice
                {
                    CustomerId = request.CustomerId,
                    EmployeeId = currentUserId,
                    Subtotal = subtotal,
                    Discount = totalDiscount,
                    Total = finalTotal,
                    Paid = request.PaidAmount,
                    PaymentMethod = request.PaymentMethod ?? ShopManager.Core.Enums.PaymentMethod.Cash.ToDbString(),
                    VoucherId = voucherId,
                    Note = request.Note,
                    Status = ShopManager.Core.Enums.InvoiceStatus.Completed.ToDbString(),
                    CreatedDate = DateTime.Now,
                    Items = request.Items.Select(i => new InvoiceItem
                    {
                        ProductId = i.ProductId,
                        EmployeeId = currentUserId,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        LineTotal = i.UnitPrice * i.Quantity,
                        Note = i.Note
                    }).ToList()
                };

                bool saved = await _invoiceService.SaveInvoiceAsync(invoice, voucherId);
                if (!saved)
                {
                    return StatusCode(500, ApiResponse.Fail("Lưu hóa đơn thất bại."));
                }

                return Ok(ApiResponse<Invoice>.Ok(invoice, "Thanh toán thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo hóa đơn thanh toán POS API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi thanh toán: {ex.Message}"));
            }
        }
    }
}
