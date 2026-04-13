using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Models;

namespace FashionStore.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<(int Id, string Username, string EmployeeName, string Role)>> GetAllAsync();
        Task<(int Id, string Username, string EmployeeName, string Role)?> GetByUsernameAsync(string username);
        Task<bool> AddAsync(string username, string employeeName, string hashedPassword, string role);
        Task<bool> UpdateAsync(string username, string? hashedPassword, string? role, string? employeeName);
        Task<bool> DeleteAsync(string username);
        Task<string?> GetPasswordHashAsync(string username);
        Task<bool> UpdatePasswordAsync(string username, string newHashedPassword);
    }
}
