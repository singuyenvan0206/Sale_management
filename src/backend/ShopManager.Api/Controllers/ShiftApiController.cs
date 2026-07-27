using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManager.Core.DTOs;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;
using ShopManager.Services;

namespace ShopManager.Web.Controllers.Api
{
    [ApiController]
    [Route("api/v1/shifts")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
    public class ShiftApiController : ControllerBase
    {
        private readonly IShiftService _shiftService;
        private readonly ILogger<ShiftApiController> _logger;

        public ShiftApiController(IShiftService shiftService, ILogger<ShiftApiController> logger)
        {
            _shiftService = shiftService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin ca làm việc hiện tại của nhân viên
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentShift()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int employeeId))
                {
                    return Unauthorized(ApiResponse.Fail("Không xác định được nhân viên."));
                }

                var shift = await _shiftService.GetCurrentShiftAsync(employeeId);
                return Ok(ApiResponse<EmployeeShift?>.Ok(shift));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lấy ca hiện tại API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mở ca làm việc mới
        /// </summary>
        [HttpPost("open")]
        public async Task<IActionResult> OpenShift([FromQuery] decimal initialCash, [FromQuery] string? note)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int employeeId))
                {
                    return Unauthorized(ApiResponse.Fail("Không xác định được nhân viên."));
                }

                var existingShift = await _shiftService.GetCurrentShiftAsync(employeeId);
                if (existingShift != null)
                {
                    return BadRequest(ApiResponse.Fail("Bạn đang có một ca làm việc chưa kết thúc."));
                }

                int shiftId = await _shiftService.ClockInAsync(employeeId, initialCash, note);
                return Ok(ApiResponse<object>.Ok(new { ShiftId = shiftId, InitialCash = initialCash }, "Mở ca làm việc thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi mở ca làm việc API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi mở ca: {ex.Message}"));
            }
        }

        /// <summary>
        /// Đóng ca làm việc hiện tại
        /// </summary>
        [HttpPost("close")]
        public async Task<IActionResult> CloseShift([FromQuery] decimal actualCash, [FromQuery] string? note)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int employeeId))
                {
                    return Unauthorized(ApiResponse.Fail("Không xác định được nhân viên."));
                }

                var currentShift = await _shiftService.GetCurrentShiftAsync(employeeId);
                if (currentShift == null)
                {
                    return BadRequest(ApiResponse.Fail("Không tìm thấy ca làm việc đang mở."));
                }

                bool success = await _shiftService.ClockOutAsync(currentShift.Id, actualCash, note);
                if (!success)
                {
                    return BadRequest(ApiResponse.Fail("Chốt ca thất bại."));
                }
                return Ok(ApiResponse.Ok("Chốt ca làm việc thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi đóng ca làm việc API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi chốt ca: {ex.Message}"));
            }
        }
    }
}
