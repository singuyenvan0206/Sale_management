using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public interface IFinanceService
    {
        Task<IEnumerable<Expense>> GetExpensesAsync(DateTime? from = null, DateTime? to = null);
        Task<int> AddExpenseAsync(Expense expense);
        Task<bool> DeleteExpenseAsync(int id);
        Task<decimal> GetTotalExpensesAsync(DateTime from, DateTime to);
    }

    public class FinanceService : IFinanceService
    {
        private readonly IFinanceRepository _financeRepository;

        public FinanceService(IFinanceRepository financeRepository)
        {
            _financeRepository = financeRepository;
        }

        public async Task<IEnumerable<Expense>> GetExpensesAsync(DateTime? from = null, DateTime? to = null)
        {
            if (from.HasValue && to.HasValue)
                return await _financeRepository.GetExpensesByDateRangeAsync(from.Value, to.Value);
            return await _financeRepository.GetAllExpensesAsync();
        }

        public async Task<int> AddExpenseAsync(Expense expense)
        {
            expense.CreatedDate = DateTime.Now;
            return await _financeRepository.CreateExpenseAsync(expense);
        }

        public async Task<decimal> GetTotalExpensesAsync(DateTime from, DateTime to)
        {
            return await _financeRepository.GetTotalExpensesAsync(from, to);
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            return await _financeRepository.DeleteExpenseAsync(id);
        }

        // Bridge for legacy static calls (WPF)
        private static IFinanceService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<IFinanceService>() ?? throw new InvalidOperationException("DI not initialized");
        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<Expense> GetAllExpenses() => RunSync(() => GetService().GetExpensesAsync()).ToList();
        public static bool CreateExpense(Expense expense) => RunSync(() => GetService().AddExpenseAsync(expense)) > 0;
        public static bool DeleteExpense(int id) => RunSync(() => GetService().DeleteExpenseAsync(id));
        public static decimal GetTotalExpenses(DateTime from, DateTime to) => RunSync(() => GetService().GetTotalExpensesAsync(from, to));
}
}