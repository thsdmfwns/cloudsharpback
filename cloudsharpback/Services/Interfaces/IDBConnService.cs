using MySql.Data.MySqlClient;

namespace cloudsharpback.Services.Interfaces
{
    public interface IDBConnService
    {
        public MySqlConnection Connection { get; }
    }
}
