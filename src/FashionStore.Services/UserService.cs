using FashionStore.Core.Common;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using FashionStore.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FashionStore.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> RegisterAccountAsync(string username, string employeeName, string password, string role = "Cashier")
        {
            try
            {
                string hashedPassword = PasswordHelper.HashPassword(password);
                var success = await _userRepository.AddAsync(username, employeeName, hashedPassword, role);
                
                if (success)
                {
                    Log.Information("User registered successfully: {Username} as {Role}", username, role);
                    return Result.Success();
                }
                
                return Result.Failure("Không thể đăng ký tài khoản. Tên đăng nhập có thể đã tồn tại.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error registering account for {Username}", username);
                return Result.Failure($"Lỗi hệ thống khi đăng ký: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ValidateLoginAsync(string username, string password)
        {
            try
            {
                var storedPassword = await _userRepository.GetPasswordHashAsync(username);

                if (string.IsNullOrEmpty(storedPassword))
                {
                    Log.Warning("Login failed: User {Username} not found", username);
                    return Result<bool>.Failure("Tên đăng nhập hoặc mật khẩu không chính xác.");
                }

                bool isValid = PasswordHelper.VerifyPassword(password, storedPassword);

                if (!isValid)
                {
                    Log.Warning("Login failed: Invalid password for user {Username}", username);
                    return Result<bool>.Failure("Tên đăng nhập hoặc mật khẩu không chính xác.");
                }

                // Auto-upgrade password hash if it's using the old SHA256 format
                if (PasswordHelper.NeedsUpgrade(storedPassword))
                {
                    try
                    {
                        string newHashedPassword = PasswordHelper.HashPassword(password);
                        await _userRepository.UpdatePasswordAsync(username, newHashedPassword);
                        Log.Information("Password hash upgraded to BCrypt for user {Username}", username);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to upgrade password hash for user {Username}", username);
                        // We still return success because login is valid
                    }
                }

                Log.Information("User {Username} logged in successfully", username);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error validating login for {Username}", username);
                return Result<bool>.Failure($"Lỗi hệ thống khi đăng nhập: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetUserRoleAsync(string username)
        {
            try
            {
                var userResult = await _userRepository.GetByUsernameAsync(username);
                if (!userResult.HasValue) return Result<string>.Failure("Người dùng không tồn tại.");
                var userFound = userResult.Value;
                return Result<string>.Success(userFound.Role ?? "Cashier");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting role for {Username}", username);
                return Result<string>.Failure("Lỗi khi lấy thông tin quyền hạn.");
            }
        }

        public async Task<Result<UserRole>> GetUserRoleEnumAsync(string username)
        {
            var result = await GetUserRoleAsync(username);
            if (result.IsFailure) return Result<UserRole>.Failure(result.ErrorMessage);

            UserRole role = result.Value.ToLower() switch
            {
                "admin" => UserRole.Admin,
                "manager" => UserRole.Manager,
                "cashier" => UserRole.Cashier,
                _ => UserRole.Cashier
            };
            return Result<UserRole>.Success(role);
        }

        public async Task<Result> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            try
            {
                var storedPassword = await _userRepository.GetPasswordHashAsync(username);

                if (string.IsNullOrEmpty(storedPassword))
                    return Result.Failure("Người dùng không tồn tại.");

                bool isOldPasswordValid = PasswordHelper.VerifyPassword(oldPassword, storedPassword);
                if (!isOldPasswordValid)
                    return Result.Failure("Mật khẩu cũ không chính xác.");

                string hashedNewPassword = PasswordHelper.HashPassword(newPassword);
                bool success = await _userRepository.UpdatePasswordAsync(username, hashedNewPassword);
                
                if (success)
                {
                    Log.Information("Password changed successfully for user {Username}", username);
                    return Result.Success();
                }
                return Result.Failure("Không thể cập nhật mật khẩu mới.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error changing password for {Username}", username);
                return Result.Failure("Lỗi hệ thống khi đổi mật khẩu.");
            }
        }

        public async Task<Result<IEnumerable<(int Id, string Username, string EmployeeName, string Role)>>> GetAllAccountsAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var list = users.Select(u => (u.Id, u.Username, u.EmployeeName, u.Role));
                return Result<IEnumerable<(int Id, string Username, string EmployeeName, string Role)>>.Success(list);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all accounts");
                return Result<IEnumerable<(int Id, string Username, string EmployeeName, string Role)>>.Failure("Lỗi khi tải danh sách tài khoản.");
            }
        }

        public async Task<Result<string>> GetEmployeeNameAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                return Result<string>.Success(user.HasValue ? user.Value.EmployeeName : username);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting employee name for {Username}", username);
                return Result<string>.Success(username); // Fallback
            }
        }

        public async Task<Result<int>> GetEmployeeIdByUsernameAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                return Result<int>.Success(user.HasValue ? user.Value.Id : 1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting employee ID for {Username}", username);
                return Result<int>.Success(1); // Fallback
            }
        }

        public async Task<Result> DeleteAccountAsync(string username)
        {
            try
            {
                bool success = await _userRepository.DeleteAsync(username);
                if (success)
                {
                    Log.Information("Account deleted: {Username}", username);
                    return Result.Success();
                }
                return Result.Failure("Không thể xóa tài khoản.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting account {Username}", username);
                return Result.Failure("Lỗi hệ thống khi xóa tài khoản.");
            }
        }

        public async Task<Result> UpdateAccountAsync(string username, string? newPassword = null, string? newRole = null, string? newEmployeeName = null)
        {
            try
            {
                string? hashedPassword = !string.IsNullOrWhiteSpace(newPassword)
                    ? PasswordHelper.HashPassword(newPassword)
                    : null;

                bool success = await _userRepository.UpdateAsync(username, hashedPassword, newRole, newEmployeeName);
                if (success)
                {
                    Log.Information("Account updated: {Username}", username);
                    return Result.Success();
                }
                return Result.Failure("Không thể cập nhật tài khoản.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating account {Username}", username);
                return Result.Failure("Lỗi hệ thống khi cập nhật tài khoản.");
            }
        }

        // Bridge for legacy static calls - MARKED OBSOLETE AND UNRELIABLE
        private static IUserService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<IUserService>() ?? throw new InvalidOperationException("DI not initialized");

        [Obsolete("Use IUserService via Dependency Injection instead. Static bridge may cause deadlocks in UI thread.")]
        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();
        
        [Obsolete("Use IUserService via Dependency Injection instead. Static bridge may cause deadlocks in UI thread.")]
        private static void RunSync(Func<Task> func) => Task.Run(func).GetAwaiter().GetResult();

        [Obsolete("Use Async version through DI")]
        public static bool RegisterAccount(string u, string e, string p, string r = "Cashier") => RunSync(() => GetService().RegisterAccountAsync(u, e, p, r)).IsSuccess;
        
        [Obsolete("Use Async version through DI")]
        public static string ValidateLogin(string u, string p) => RunSync(() => GetService().ValidateLoginAsync(u, p)).Value.ToString().ToLower();
        
        [Obsolete("Use Async version through DI")]
        public static string GetUserRole(string u) => RunSync(() => GetService().GetUserRoleAsync(u)).Value;
        
        [Obsolete("Use Async version through DI")]
        public static bool ChangePassword(string u, string o, string n) => RunSync(() => GetService().ChangePasswordAsync(u, o, n)).IsSuccess;
        
        [Obsolete("Use Async version through DI")]
        public static List<(int Id, string Username, string EmployeeName, string Role)> GetAllAccounts() => RunSync(() => GetService().GetAllAccountsAsync()).Value.ToList();
        
        [Obsolete("Use Async version through DI")]
        public static string GetEmployeeName(string u) => RunSync(() => GetService().GetEmployeeNameAsync(u)).Value;
        
        [Obsolete("Use Async version through DI")]
        public static int GetEmployeeIdByUsername(string u) => RunSync(() => GetService().GetEmployeeIdByUsernameAsync(u)).Value;
        
        [Obsolete("Use Async version through DI")]
        public static bool DeleteAccount(string u) => RunSync(() => GetService().DeleteAccountAsync(u)).IsSuccess;
        
        [Obsolete("Use Async version through DI")]
        public static bool UpdateAccount(string u, string? p = null, string? r = null, string? e = null) => RunSync(() => GetService().UpdateAccountAsync(u, p, r, e)).IsSuccess;
        
        [Obsolete("Use Async version through DI")]
        public static UserRole GetUserRoleEnum(string u) => RunSync(() => GetService().GetUserRoleEnumAsync(u)).Value;
        
        [Obsolete("Use Async version through DI")]
        public static bool DeleteAllAccountsExceptAdmin()
        {
            var accountsResult = RunSync(() => GetService().GetAllAccountsAsync());
            if (accountsResult.IsFailure) return false;
            
            foreach (var acc in accountsResult.Value)
            {
                if (acc.Username.ToLower() != "admin") RunSync(() => GetService().DeleteAccountAsync(acc.Username));
            }
            return true;
        }
    }
}
