using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Core.Models;

namespace FashionStore.Core.Interfaces
{
    public interface IFinanceRepository
    {
        Task<IEnumerable<Expense>> GetAllExpensesAsync();
        Task<IEnumerable<Expense>> GetExpensesByDateRangeAsync(DateTime from, DateTime to);
        Task<int> CreateExpenseAsync(Expense expense);
        Task<bool> DeleteExpenseAsync(int id);
        
        // Financial aggregation
        Task<decimal> GetTotalExpensesAsync(DateTime from, DateTime to);
    }
}
