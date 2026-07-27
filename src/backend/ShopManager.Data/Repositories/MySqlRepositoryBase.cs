using ShopManager.Core.Settings;
using MySql.Data.MySqlClient;

namespace ShopManager.Data.Repositories
{
    public abstract class MySqlRepositoryBase
    {
        protected string ConnectionString => SettingsManager.BuildConnectionString();

        protected MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
