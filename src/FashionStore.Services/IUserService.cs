using FashionStore.Core.Models;

namespace FashionStore.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAccountAsync(string username, string employeeName, string password, string role = "Cashier");
        Task<string> ValidateLoginAsync(string username, string password);
        Task<string> GetUserRoleAsync(string username);
        Task<UserRole> GetUserRoleEnumAsync(string username);
        Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword);
        Task<IEnumerable<(int Id, string Username, string EmployeeName)>> GetAllAccountsAsync();
        Task<string> GetEmployeeNameAsync(string username);
        Task<int> GetEmployeeIdByUsernameAsync(string username);
        Task<bool> DeleteAccountAsync(string username);
        Task<bool> UpdateAccountAsync(string username, string? newPassword = null, string? newRole = null, string? newEmployeeName = null);
        Task MigratePasswordsToHashedAsync();
    }
}
