using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManager.Core.DTOs;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;
using ShopManager.Services;

namespace ShopManager.Web.Controllers.Api
{
    [ApiController]
    [Route("api/v1/invoices")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
    public class InvoiceApiController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<InvoiceApiController> _logger;

        public InvoiceApiController(IInvoiceService invoiceService, ILogger<InvoiceApiController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách hóa đơn bán hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? search)
        {
            try
            {
                var invoices = await _invoiceService.SearchInvoicesAsync(startDate, endDate, null, search ?? string.Empty);
                return Ok(ApiResponse<IEnumerable<Invoice>>.Ok(invoices));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lấy danh sách hóa đơn API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết hóa đơn theo ID (kèm danh sách món hàng)
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            try
            {
                var details = await _invoiceService.GetInvoiceDetailsAsync(id);
                if (details.Header == null || details.Header.Id == 0)
                {
                    return NotFound(ApiResponse.Fail($"Không tìm thấy hóa đơn với Id = {id}"));
                }
                var invoice = details.Header;
                invoice.Items = details.Items;
                return Ok(ApiResponse<Invoice>.Ok(invoice));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lấy chi tiết hóa đơn API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi hệ thống: {ex.Message}"));
            }
        }
    }
}
