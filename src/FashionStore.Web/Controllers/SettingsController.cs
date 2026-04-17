using FashionStore.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly FashionStore.Services.ICustomerService _customerService;
        private readonly FashionStore.Core.Interfaces.ISystemSettingsService _settingsService;

        public SettingsController(FashionStore.Services.ICustomerService customerService, FashionStore.Core.Interfaces.ISystemSettingsService settingsService)
        {
            _customerService = customerService;
            _settingsService = settingsService;
        }

        [HttpGet]
        public IActionResult Tiers()
        {
            var settings = TierSettingsManager.Load();
            return View(settings);
        }

        [HttpPost]
        public IActionResult Tiers(TierSettings settings)
        {
            if (ModelState.IsValid)
            {
                TierSettingsManager.Save(settings);
                TempData["Success"] = "Đã lưu cài đặt phân hạng thành công!";
            }
            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> RefreshLoyalty()
        {
            var settings = TierSettingsManager.Load();
            int count = await _customerService.RefreshAllLoyaltyAsync(
                settings.SpendPerPoint,
                settings.SilverMinPoints,
                settings.GoldMinPoints,
                settings.VIPMinPoints);

            TempData["Success"] = $"Đã cập nhật phân hạng cho {count} khách hàng dựa trên lịch sử chi tiêu.";
            return RedirectToAction(nameof(Tiers));
        }

        [HttpGet]
        public IActionResult Store()
        {
            // Placeholder for store info settings
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Payment()
        {
            var settings = FashionStore.Core.Settings.PaymentSettingsManager.Load();
            settings.SePayToken = await _settingsService.GetSePayTokenAsync() ?? "";
            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> Payment(FashionStore.Core.Settings.PaymentSettings settings)
        {
            if (ModelState.IsValid)
            {
                // Save other settings to file (legacy)
                FashionStore.Core.Settings.PaymentSettingsManager.Save(settings);
                
                // Save Token to Database (modern/secure)
                await _settingsService.SaveSePayTokenAsync(settings.SePayToken);
                
                TempData["Success"] = "Đã lưu cài đặt thanh toán thành công!";
            }
            return View(settings);
        }
    }
}
