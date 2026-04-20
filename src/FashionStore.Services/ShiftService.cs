using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public interface IShiftService
    {
        Task<int> ClockInAsync(int employeeId, decimal openingBalance, string? notes);
        Task<bool> ClockOutAsync(int shiftId, decimal closingBalance, string? notes);
        Task<EmployeeShift?> GetCurrentShiftAsync(int employeeId);
        Task<IEnumerable<EmployeeShift>> GetHistoryAsync(int count = 50);
    }

    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _shiftRepository;

        public ShiftService(IShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        public async Task<int> ClockInAsync(int employeeId, decimal openingBalance, string? notes)
        {
            var shift = new EmployeeShift
            {
                EmployeeId = employeeId,
                ClockIn = DateTime.Now,
                OpeningBalance = openingBalance,
                Notes = notes
            };
            return await _shiftRepository.ClockInAsync(shift);
        }

        public async Task<bool> ClockOutAsync(int shiftId, decimal closingBalance, string? notes)
        {
            return await _shiftRepository.ClockOutAsync(shiftId, closingBalance, notes);
        }

        public async Task<EmployeeShift?> GetCurrentShiftAsync(int employeeId)
        {
            return await _shiftRepository.GetActiveShiftByEmployeeIdAsync(employeeId);
        }

        public async Task<IEnumerable<EmployeeShift>> GetHistoryAsync(int count = 50)
        {
            return await _shiftRepository.GetShiftHistoryAsync(count);
        }

        // Bridge for legacy static calls (WPF)
        private static IShiftService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<IShiftService>() ?? throw new InvalidOperationException("DI not initialized");
        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();
        private static void RunSync(Func<Task> func) => Task.Run(func).GetAwaiter().GetResult();

        public static int ClockIn(EmployeeShift shift) => RunSync(() => GetService().ClockInAsync(shift.EmployeeId, shift.OpeningBalance, shift.Notes));
        public static bool ClockOut(int shiftId, decimal closingBalance, string? notes) => RunSync(() => GetService().ClockOutAsync(shiftId, closingBalance, notes));
        public static EmployeeShift? GetActiveShiftByEmployeeId(int employeeId) => RunSync(() => GetService().GetCurrentShiftAsync(employeeId));
        public static List<EmployeeShift> GetShiftHistory(int count = 50) => RunSync(() => GetService().GetHistoryAsync(count)).ToList();
    }
}
