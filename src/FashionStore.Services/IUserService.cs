using FashionStore.Core.Common;
using FashionStore.Core.Models;

namespace FashionStore.Services
{
    public interface IUserService
    {
        Task<Result> RegisterAccountAsync(string username, string employeeName, string password, string role = "Cashier");
        Task<Result<bool>> ValidateLoginAsync(string username, string password);
        Task<Result<string>> GetUserRoleAsync(string username);
        Task<Result<UserRole>> GetUserRoleEnumAsync(string username);
        Task<Result> ChangePasswordAsync(string username, string oldPassword, string newPassword);
        Task<Result<IEnumerable<(int Id, string Username, string EmployeeName, string Role)>>> GetAllAccountsAsync();
        Task<Result<string>> GetEmployeeNameAsync(string username);
        Task<Result<int>> GetEmployeeIdByUsernameAsync(string username);
        Task<Result> DeleteAccountAsync(string username);
        Task<Result> UpdateAccountAsync(string username, string? newPassword = null, string? newRole = null, string? newEmployeeName = null);
    }
}
