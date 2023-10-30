using MySql.Data.MySqlClient;
using StackExchange.Redis;

namespace cloudsharpback.Services.Interfaces
{
    public interface IDBConnectionFactory
    {
        public MySqlConnection MySqlConnection { get; }
        public IDatabase Redis { get; }
    }
}
