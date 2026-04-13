using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FashionStore.Core;
using FashionStore.Models;
using FashionStore.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> RegisterAccountAsync(string username, string employeeName, string password, string role = "Cashier")
        {
            string hashedPassword = PasswordHelper.HashPassword(password);
            return await _userRepository.AddAsync(username, employeeName, hashedPassword, role);
        }

        public async Task<string> ValidateLoginAsync(string username, string password)
        {
            var storedPassword = await _userRepository.GetPasswordHashAsync(username);

            if (string.IsNullOrEmpty(storedPassword))
                return "false";

            bool isValid = PasswordHelper.VerifyPassword(password, storedPassword);
            
            // Auto-upgrade password hash if it's using the old SHA256 format
            if (isValid && PasswordHelper.NeedsUpgrade(storedPassword))
            {
                try
                {
                    string newHashedPassword = PasswordHelper.HashPassword(password);
                    await _userRepository.UpdatePasswordAsync(username, newHashedPassword);
                }
                catch { /* Ignore upgrade errors, login still works */ }
            }

            return isValid ? "true" : "false";
        }

        public async Task<string> GetUserRoleAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user?.Role ?? "Cashier";
        }

        public async Task<UserRole> GetUserRoleEnumAsync(string username)
        {
            string roleString = await GetUserRoleAsync(username);
            return roleString.ToLower() switch
            {
                "admin" => UserRole.Admin,
                "manager" => UserRole.Manager,
                "cashier" => UserRole.Cashier,
                _ => UserRole.Cashier
            };
        }

        public async Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            var storedPassword = await _userRepository.GetPasswordHashAsync(username);

            if (string.IsNullOrEmpty(storedPassword))
                return false;

            bool isOldPasswordValid = PasswordHelper.VerifyPassword(oldPassword, storedPassword);
            if (!isOldPasswordValid)
                return false;

            string hashedNewPassword = PasswordHelper.HashPassword(newPassword);
            return await _userRepository.UpdatePasswordAsync(username, hashedNewPassword);
        }

        public async Task<IEnumerable<(int Id, string Username, string EmployeeName)>> GetAllAccountsAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => (u.Id, u.Username, u.EmployeeName));
        }

        public async Task<string> GetEmployeeNameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user?.EmployeeName ?? username;
        }

        public async Task<int> GetEmployeeIdByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user?.Id ?? 1;
        }

        public async Task<bool> DeleteAccountAsync(string username)
        {
            return await _userRepository.DeleteAsync(username);
        }

        public async Task<bool> UpdateAccountAsync(string username, string? newPassword = null, string? newRole = null, string? newEmployeeName = null)
        {
            string? hashedPassword = !string.IsNullOrWhiteSpace(newPassword) 
                ? PasswordHelper.HashPassword(newPassword) 
                : null;
                
            return await _userRepository.UpdateAsync(username, hashedPassword, newRole, newEmployeeName);
        }

        public async Task MigratePasswordsToHashedAsync()
        {
            var users = await _userRepository.GetAllAsync();
            foreach (var user in users)
            {
                var hash = await _userRepository.GetPasswordHashAsync(user.Username);
                if (hash != null && PasswordHelper.NeedsUpgrade(hash))
                {
                    // This is only possible if we have the plain text or if we are upgrading from legacy hash to bcrypt
                    // But legacy hash is already "hashed". 
                    // Actually, the legacy code had a MigratePasswordsToHashed method that handled plain text -> SHA256.
                    // Now PasswordHelper handles Legacy SHA256 -> BCrypt during login.
                }
            }
        }

        // Bridge for legacy static calls - SHOULD BE DEPRECATED
        private static IUserService GetService() => App.ServiceProvider?.GetRequiredService<IUserService>() ?? throw new InvalidOperationException("DI not initialized");

        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();
        private static void RunSync(Func<Task> func) => Task.Run(func).GetAwaiter().GetResult();

        public static bool RegisterAccount(string u, string e, string p, string r = "Cashier") => RunSync(() => GetService().RegisterAccountAsync(u, e, p, r));
        public static string ValidateLogin(string u, string p) => RunSync(() => GetService().ValidateLoginAsync(u, p));
        public static string GetUserRole(string u) => RunSync(() => GetService().GetUserRoleAsync(u));
        public static bool ChangePassword(string u, string o, string n) => RunSync(() => GetService().ChangePasswordAsync(u, o, n));
        public static List<(int Id, string Username, string EmployeeName)> GetAllAccounts() => RunSync(() => GetService().GetAllAccountsAsync()).ToList();
        public static string GetEmployeeName(string u) => RunSync(() => GetService().GetEmployeeNameAsync(u));
        public static int GetEmployeeIdByUsername(string u) => RunSync(() => GetService().GetEmployeeIdByUsernameAsync(u));
        public static bool DeleteAccount(string u) => RunSync(() => GetService().DeleteAccountAsync(u));
        public static bool UpdateAccount(string u, string? p = null, string? r = null, string? e = null) => RunSync(() => GetService().UpdateAccountAsync(u, p, r, e));
        public static void MigratePasswordsToHashed(object? conn = null) => RunSync(() => GetService().MigratePasswordsToHashedAsync());
        public static UserRole GetUserRoleEnum(string u) => RunSync(() => GetService().GetUserRoleEnumAsync(u));
        public static bool DeleteAllAccountsExceptAdmin()
        {
            var accounts = GetAllAccounts();
            foreach (var acc in accounts)
            {
                if (acc.Username.ToLower() != "admin") DeleteAccount(acc.Username);
            }
            return true;
        }
    }
}
