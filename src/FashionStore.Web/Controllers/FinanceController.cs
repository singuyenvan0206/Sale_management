using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FashionStore.Core.Models;
using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FinanceController : Controller
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        public async Task<IActionResult> Expenses(DateTime? from, DateTime? to)
        {
            var startDate = from ?? DateTime.Now.AddMonths(-1);
            var endDate = to ?? DateTime.Now;
            
            var expenses = await _financeService.GetExpensesAsync(startDate, endDate);
            ViewBag.From = startDate.ToString("yyyy-MM-dd");
            ViewBag.To = endDate.ToString("yyyy-MM-dd");
            ViewBag.Total = expenses.Sum(e => e.Amount);
            
            return View(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense(Expense expense)
        {
            if (ModelState.IsValid)
            {
                expense.EmployeeId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "1");
                await _financeService.AddExpenseAsync(expense);
                TempData["Success"] = "Đã ghi nhận chi phí.";
            }
            return RedirectToAction(nameof(Expenses));
        }
    }
}
