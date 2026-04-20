using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Core.Models;

namespace FashionStore.Core.Interfaces
{
    public interface IShiftRepository
    {
        Task<int> ClockInAsync(EmployeeShift shift);
        Task<bool> ClockOutAsync(int shiftId, decimal closingBalance, string? notes);
        Task<EmployeeShift?> GetActiveShiftByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<EmployeeShift>> GetShiftHistoryAsync(int count);
        Task<EmployeeShift?> GetByIdAsync(int id);
    }
}
