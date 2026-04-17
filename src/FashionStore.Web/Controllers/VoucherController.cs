using FashionStore.Core.Models;
using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class VoucherController : Controller
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        public async Task<IActionResult> Index()
        {
            var vouchers = await _voucherService.GetAllVouchersAsync();
            return View(vouchers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Voucher { StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(1), IsActive = true });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                await _voucherService.AddVoucherAsync(voucher);
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null) return NotFound();
            return View(voucher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                await _voucherService.UpdateVoucherAsync(voucher);
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _voucherService.DeleteVoucherAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
