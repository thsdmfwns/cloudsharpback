using cloudsharpback.Services.Interfaces;
using MySql.Data.MySqlClient;

namespace cloudsharpback.Services
{
    public class DBConnectionFactory : IDBConnectionFactory
    {
        private readonly string _connStr;
        public DBConnectionFactory(IConfiguration configuration)
        {
            _connStr = configuration["DBConnectionString"];
        }

        public MySqlConnection Connection => new MySqlConnection(_connStr);
    }
}
