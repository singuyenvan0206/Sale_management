using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManager.Core.DTOs;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;
using ShopManager.Services;

namespace ShopManager.Web.Controllers.Api
{
    [ApiController]
    [Route("api/v1/products")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ProductApiController> _logger;

        public ProductApiController(
            IProductService productService,
            ICategoryService categoryService,
            ILogger<ProductApiController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách sản phẩm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] string? search, [FromQuery] int? categoryId)
        {
            try
            {
                var products = await _productService.GetAllProductsWithCategoriesAsync();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    products = products.Where(p => 
                        p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(p.Code) && p.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
                }

                return Ok(ApiResponse<IEnumerable<Product>>.Ok(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lấy danh sách sản phẩm API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết sản phẩm theo ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse.Fail($"Không tìm thấy sản phẩm với Id = {id}"));
            }
            return Ok(ApiResponse<Product>.Ok(product));
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo Mã sản phẩm (Code / Barcode)
        /// </summary>
        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetProductByCode(string code)
        {
            var product = await _productService.GetProductByCodeAsync(code);
            if (product == null)
            {
                return NotFound(ApiResponse.Fail($"Không tìm thấy sản phẩm có mã = '{code}'"));
            }
            return Ok(ApiResponse<Product>.Ok(product));
        }

        /// <summary>
        /// Thêm mới sản phẩm
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Dữ liệu không hợp lệ."));
            }

            try
            {
                bool success = await _productService.AddProductAsync(product);
                if (!success)
                {
                    return BadRequest(ApiResponse.Fail("Không thể tạo sản phẩm."));
                }
                return Ok(ApiResponse<Product>.Ok(product, "Tạo sản phẩm thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi thêm mới sản phẩm API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi thêm sản phẩm: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật sản phẩm
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest(ApiResponse.Fail("Id không khớp."));
            }

            try
            {
                bool updated = await _productService.UpdateProductAsync(product);
                if (!updated)
                {
                    return NotFound(ApiResponse.Fail("Cập nhật thất bại. Sản phẩm không tồn tại."));
                }
                return Ok(ApiResponse.Ok("Cập nhật sản phẩm thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi cập nhật sản phẩm API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi cập nhật: {ex.Message}"));
            }
        }

        /// <summary>
        /// Xóa sản phẩm
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                bool deleted = await _productService.DeleteProductAsync(id);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("Xóa thất bại. Sản phẩm không tồn tại."));
                }
                return Ok(ApiResponse.Ok("Xóa sản phẩm thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xóa sản phẩm API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi xóa: {ex.Message}"));
            }
        }
    }
}
