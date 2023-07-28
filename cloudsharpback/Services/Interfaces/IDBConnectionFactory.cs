using MySql.Data.MySqlClient;

namespace cloudsharpback.Services.Interfaces
{
    public interface IDBConnectionFactory
    {
        public MySqlConnection Connection { get; }
    }
}
