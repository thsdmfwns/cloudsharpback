using JsonWebToken;
using MySql.Data.MySqlClient;

namespace cloudsharpback.Services
{
    public class DBConnService : IDBConnService
    {
        private readonly string _connStr;
        public DBConnService(IConfiguration configuration)
        {
            _connStr = configuration["DBConnectionString"];
        }

        public MySqlConnection Connection => new MySqlConnection(_connStr);
    }
}
