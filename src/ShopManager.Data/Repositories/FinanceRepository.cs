using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;

namespace ShopManager.Data.Repositories
{
    public class FinanceRepository : MySqlRepositoryBase, IFinanceRepository
    {
        public async Task<IEnumerable<Expense>> GetAllExpensesAsync()
        {
            using var conn = GetConnection();
            const string sql = @"
                SELECT e.*, a.EmployeeName 
                FROM Expenses e
                LEFT JOIN Accounts a ON e.EmployeeId = a.Id
                ORDER BY e.CreatedDate DESC";
            return await conn.QueryAsync<Expense>(sql);
        }

        public async Task<IEnumerable<Expense>> GetExpensesByDateRangeAsync(DateTime from, DateTime to)
        {
            using var conn = GetConnection();
            const string sql = @"
                SELECT e.*, a.EmployeeName 
                FROM Expenses e
                LEFT JOIN Accounts a ON e.EmployeeId = a.Id
                WHERE e.CreatedDate BETWEEN @from AND @to
                ORDER BY e.CreatedDate DESC";
            return await conn.QueryAsync<Expense>(sql, new { from, to });
        }

        public async Task<int> CreateExpenseAsync(Expense expense)
        {
            using var conn = GetConnection();
            const string sql = @"
                INSERT INTO Expenses (Category, Amount, Description, EmployeeId, CreatedDate)
                VALUES (@Category, @Amount, @Description, @EmployeeId, @CreatedDate);
                SELECT LAST_INSERT_ID();";
            return await conn.ExecuteScalarAsync<int>(sql, expense);
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            using var conn = GetConnection();
            const string sql = "DELETE FROM Expenses WHERE Id = @id";
            return await conn.ExecuteAsync(sql, new { id }) > 0;
        }

        public async Task<decimal> GetTotalExpensesAsync(DateTime from, DateTime to)
        {
            using var conn = GetConnection();
            const string sql = "SELECT SUM(Amount) FROM Expenses WHERE CreatedDate BETWEEN @from AND @to";
            return await conn.ExecuteScalarAsync<decimal?>(sql, new { from, to }) ?? 0;
        }
    }
}

