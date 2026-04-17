using System.Threading.Tasks;

namespace FashionStore.Core.Interfaces
{
    public interface ISystemSettingsRepository
    {
        Task<string?> GetValueAsync(string key);
        Task<bool> SetValueAsync(string key, string value, string? description = null);
    }
}
