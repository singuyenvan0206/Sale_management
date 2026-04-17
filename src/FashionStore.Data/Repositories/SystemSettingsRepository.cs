using Dapper;
using FashionStore.Core.Interfaces;

namespace FashionStore.Data.Repositories
{
    public class SystemSettingsRepository : MySqlRepositoryBase, ISystemSettingsRepository
    {
        public async Task<string?> GetValueAsync(string key)
        {
            using var connection = GetConnection();
            string sql = "SELECT SettingValue FROM SystemSettings WHERE SettingKey = @Key;";
            return await connection.ExecuteScalarAsync<string>(sql, new { Key = key });
        }

        public async Task<bool> SetValueAsync(string key, string value, string? description = null)
        {
            using var connection = GetConnection();
            string sql = @"
                INSERT INTO SystemSettings (SettingKey, SettingValue, Description) 
                VALUES (@Key, @Value, @Description)
                ON DUPLICATE KEY UPDATE 
                    SettingValue = @Value, 
                    Description = COALESCE(@Description, Description);";
            
            return await connection.ExecuteAsync(sql, new { Key = key, Value = value, Description = description }) > 0;
        }
    }
}
