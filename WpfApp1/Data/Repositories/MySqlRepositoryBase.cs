using MySql.Data.MySqlClient;
using FashionStore.Core;

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
