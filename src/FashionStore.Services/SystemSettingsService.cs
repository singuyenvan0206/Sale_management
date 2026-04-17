using FashionStore.Core.Interfaces;
using FashionStore.Core.Utils;
using System;
using System.Threading.Tasks;

namespace FashionStore.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly ISystemSettingsRepository _repository;

        public SystemSettingsService(ISystemSettingsRepository repository)
        {
            _repository = repository;
        }

        public async Task<string?> GetSettingAsync(string key, string? defaultValue = null)
        {
            var value = await _repository.GetValueAsync(key);
            return value ?? defaultValue;
        }

        public async Task<bool> SaveSettingAsync(string key, string value, string? description = null)
        {
            return await _repository.SetValueAsync(key, value, description);
        }

        public async Task<string?> GetSePayTokenAsync()
        {
            // 1. Try Database
            var cipherText = await _repository.GetValueAsync("SePayToken");
            if (!string.IsNullOrEmpty(cipherText))
            {
                return EncryptionHelper.Decrypt(cipherText);
            }

            // 2. Try Environment Variable (Fallback/Bootstrap)
            return Environment.GetEnvironmentVariable("SEPAY_TOKEN");
        }

        public async Task<bool> SaveSePayTokenAsync(string token)
        {
            var cipherText = EncryptionHelper.Encrypt(token);
            return await _repository.SetValueAsync("SePayToken", cipherText, "SePay API Token for bank transaction verification (Encrypted)");
        }
    }
}
