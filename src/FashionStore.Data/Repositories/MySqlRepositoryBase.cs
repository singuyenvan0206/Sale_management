using FashionStore.Core.Settings;
using MySql.Data.MySqlClient;

namespace FashionStore.Data.Repositories
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
