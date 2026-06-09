using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;

namespace ShopManager.Data.Repositories
{
    public class ShiftRepository : MySqlRepositoryBase, IShiftRepository
    {
        public async Task<int> ClockInAsync(EmployeeShift shift)
        {
            using var conn = GetConnection();
            const string sql = @"
                INSERT INTO EmployeeShifts (EmployeeId, ClockIn, OpeningBalance, Notes)
                VALUES (@EmployeeId, @ClockIn, @OpeningBalance, @Notes);
                SELECT LAST_INSERT_ID();";
            return await conn.ExecuteScalarAsync<int>(sql, shift);
        }

        public async Task<bool> ClockOutAsync(int shiftId, decimal closingBalance, string? notes)
        {
            using var conn = GetConnection();
            const string sql = @"
                UPDATE EmployeeShifts 
                SET ClockOut = @ClockOut, ClosingBalance = @closingBalance, Notes = COALESCE(CONCAT(Notes, '\n', @notes), Notes)
                WHERE Id = @shiftId";
            return await conn.ExecuteAsync(sql, new { shiftId, closingBalance, notes, ClockOut = DateTime.Now }) > 0;
        }

        public async Task<EmployeeShift?> GetActiveShiftByEmployeeIdAsync(int employeeId)
        {
            using var conn = GetConnection();
            const string sql = @"
                SELECT s.*, a.EmployeeName
                FROM EmployeeShifts s
                JOIN Accounts a ON s.EmployeeId = a.Id
                WHERE s.EmployeeId = @employeeId AND s.ClockOut IS NULL
                LIMIT 1";
            return await conn.QueryFirstOrDefaultAsync<EmployeeShift>(sql, new { employeeId });
        }

        public async Task<IEnumerable<EmployeeShift>> GetShiftHistoryAsync(int count)
        {
            using var conn = GetConnection();
            const string sql = @"
                SELECT s.*, a.EmployeeName
                FROM EmployeeShifts s
                JOIN Accounts a ON s.EmployeeId = a.Id
                ORDER BY s.ClockIn DESC
                LIMIT @count";
            return await conn.QueryAsync<EmployeeShift>(sql, new { count });
        }

        public async Task<EmployeeShift?> GetByIdAsync(int id)
        {
            using var conn = GetConnection();
            const string sql = @"
                SELECT s.*, a.EmployeeName
                FROM EmployeeShifts s
                JOIN Accounts a ON s.EmployeeId = a.Id
                WHERE s.Id = @id";
            return await conn.QueryFirstOrDefaultAsync<EmployeeShift>(sql, new { id });
        }
    }
}

