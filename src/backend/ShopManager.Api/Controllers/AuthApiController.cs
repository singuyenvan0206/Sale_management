using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManager.Core.DTOs;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;
using ShopManager.Services;

namespace ShopManager.Web.Controllers.Api
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            IUserService userService,
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            ILogger<AuthApiController> logger)
        {
            _userService = userService;
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập tài khoản và nhận JWT Token
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(ApiResponse.Fail("Tên đăng nhập và mật khẩu không được để trống."));
            }

            try
            {
                var loginResult = await _userService.ValidateLoginAsync(request.Username, request.Password);
                if (!loginResult.IsSuccess || !loginResult.Value)
                {
                    return Unauthorized(ApiResponse.Fail(!string.IsNullOrEmpty(loginResult.ErrorMessage) ? loginResult.ErrorMessage : "Tên đăng nhập hoặc mật khẩu không chính xác."));
                }

                var userTuple = await _userRepository.GetByUsernameAsync(request.Username);
                if (!userTuple.HasValue)
                {
                    return Unauthorized(ApiResponse.Fail("Không tìm thấy thông tin tài khoản."));
                }

                var user = new Account
                {
                    Id = userTuple.Value.Id,
                    Username = userTuple.Value.Username,
                    EmployeeName = userTuple.Value.EmployeeName,
                    Role = userTuple.Value.Role
                };

                var token = _jwtTokenService.GenerateToken(user);
                var expiryMinutes = 480;

                var authResponse = new AuthResponse
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                    UserId = user.Id,
                    Username = user.Username,
                    FullName = string.IsNullOrEmpty(user.EmployeeName) ? user.Username : user.EmployeeName,
                    Role = user.Role ?? UserRole.Cashier.ToString()
                };

                _logger.LogInformation("API Login successful for user: {Username}", user.Username);
                return Ok(ApiResponse<AuthResponse>.Ok(authResponse, "Đăng nhập thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi đăng nhập API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi máy chủ: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thông tin người dùng đang đăng nhập
        /// </summary>
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
        public async Task<IActionResult> GetProfile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(ApiResponse.Fail("Không tìm thấy thông tin người dùng."));
            }

            var userTuple = await _userRepository.GetByUsernameAsync(username);
            if (!userTuple.HasValue)
            {
                return NotFound(ApiResponse.Fail("Người dùng không tồn tại."));
            }

            var profile = new UserProfileResponse
            {
                UserId = userTuple.Value.Id,
                Username = userTuple.Value.Username,
                FullName = string.IsNullOrEmpty(userTuple.Value.EmployeeName) ? userTuple.Value.Username : userTuple.Value.EmployeeName,
                Role = userTuple.Value.Role ?? UserRole.Cashier.ToString()
            };

            return Ok(ApiResponse<UserProfileResponse>.Ok(profile));
        }
    }
}
