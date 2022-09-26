using MySql.Data.MySqlClient;

namespace cloudsharpback.Services
{
    public interface IDBConnService
    {
        public MySqlConnection Connection { get; }
    }
}
