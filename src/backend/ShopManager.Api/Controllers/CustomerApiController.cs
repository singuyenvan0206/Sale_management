using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManager.Core.DTOs;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;
using ShopManager.Services;

namespace ShopManager.Web.Controllers.Api
{
    [ApiController]
    [Route("api/v1/customers")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
    public class CustomerApiController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerApiController> _logger;

        public CustomerApiController(ICustomerService customerService, ILogger<CustomerApiController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách khách hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(ApiResponse<IEnumerable<Customer>>.Ok(customers));
        }

        /// <summary>
        /// Tìm kiếm khách hàng theo tên hoặc số điện thoại
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var all = await _customerService.GetAllCustomersAsync();
                return Ok(ApiResponse<IEnumerable<Customer>>.Ok(all));
            }

            var results = await _customerService.SearchCustomersAsync(query);
            return Ok(ApiResponse<IEnumerable<Customer>>.Ok(results));
        }

        /// <summary>
        /// Lấy thông tin chi tiết khách hàng theo ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse.Fail($"Không tìm thấy khách hàng với Id = {id}"));
            }
            return Ok(ApiResponse<Customer>.Ok(customer));
        }

        /// <summary>
        /// Thêm mới khách hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Dữ liệu khách hàng không hợp lệ."));
            }

            try
            {
                bool success = await _customerService.AddCustomerAsync(customer);
                if (!success)
                {
                    return BadRequest(ApiResponse.Fail("Thêm mới khách hàng thất bại."));
                }
                return Ok(ApiResponse<Customer>.Ok(customer, "Tạo khách hàng thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi thêm khách hàng API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi tạo khách hàng: {ex.Message}"));
            }
        }
    }
}
