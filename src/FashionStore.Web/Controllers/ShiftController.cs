using System;
using System.Linq;
using System.Threading.Tasks;
using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize]
    public class ShiftController : Controller
    {
        private readonly IShiftService _shiftService;

        public ShiftController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "1");
            var currentShift = await _shiftService.GetCurrentShiftAsync(userId);
            var history = await _shiftService.GetHistoryAsync();
            
            ViewBag.CurrentShift = currentShift;
            return View(history);
        }

        [HttpPost]
        public async Task<IActionResult> ClockIn(decimal openingBalance, string? notes)
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "1");
            await _shiftService.ClockInAsync(userId, openingBalance, notes);
            TempData["Success"] = "Đã bắt đầu ca làm việc.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ClockOut(decimal closingBalance, string? notes)
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "1");
            var currentShift = await _shiftService.GetCurrentShiftAsync(userId);
            if (currentShift != null)
            {
                await _shiftService.ClockOutAsync(currentShift.Id, closingBalance, notes);
                TempData["Success"] = "Đã kết thúc ca làm việc.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
