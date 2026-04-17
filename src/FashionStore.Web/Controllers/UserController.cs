using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index(string? search = null)
        {
            var result = await _userService.GetAllAccountsAsync();
            var users = result.IsSuccess ? result.Value : new List<(int Id, string Username, string EmployeeName, string Role)>();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => 
                    u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                    u.EmployeeName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.Search = search;
            return View(users);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(string username, string employeeName, string password, string confirmPassword, string role)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            var result = await _userService.RegisterAccountAsync(username, employeeName, password, role);
            if (result.IsSuccess) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Đăng ký không thành công. Tài khoản có thể đã tồn tại.");
            return View();
        }

        public async Task<IActionResult> Edit(string username)
        {
            var result = await _userService.GetAllAccountsAsync();
            var user = result.Value.FirstOrDefault(u => u.Username == username);
            if (user == default) return NotFound();

            ViewBag.Username = user.Username;
            ViewBag.EmployeeName = user.EmployeeName;
            ViewBag.Role = user.Role;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string username, string? employeeName, string? password, string? confirmPassword, string? role)
        {
            if (!string.IsNullOrEmpty(password) && password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            var result = await _userService.UpdateAccountAsync(username, password, role, employeeName);
            if (result.IsSuccess) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Cập nhật không thành công.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(string username, string newRole)
        {
            var result = await _userService.UpdateAccountAsync(username, newRole: newRole);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string username)
        {
            var result = await _userService.DeleteAccountAsync(username);
            return RedirectToAction(nameof(Index));
        }
    }
}
