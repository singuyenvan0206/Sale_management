using System.Threading.Tasks;

namespace FashionStore.Core.Interfaces
{
    public interface ISystemSettingsService
    {
        Task<string?> GetSettingAsync(string key, string? defaultValue = null);
        Task<bool> SaveSettingAsync(string key, string value, string? description = null);
        
        // Convenience methods for SePay
        Task<string?> GetSePayTokenAsync();
        Task<bool> SaveSePayTokenAsync(string token);
    }
}
