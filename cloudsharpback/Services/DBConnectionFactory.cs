using cloudsharpback.Services.Interfaces;
using MySql.Data.MySqlClient;

namespace cloudsharpback.Services
{
    public class DBConnectionFactory : IDBConnectionFactory
    {
        private readonly string _connStr;
        public DBConnectionFactory(IEnvironmentValueStore environmentValueStore)
        {
            var database = "cloud_sharp";
            var port = environmentValueStore[RequiredEnvironmentValueKey.MYSQL_PORT];
            var server = environmentValueStore[RequiredEnvironmentValueKey.MYSQL_SERVER];
            var password = environmentValueStore[RequiredEnvironmentValueKey.MYSQL_PASSWORD];
            var user = environmentValueStore[RequiredEnvironmentValueKey.MYSQL_USER];
            //Server=<server_address>;Port=<port_number>;Database=<database_name>;Uid=<username>;Pwd=<password>;

            _connStr = $"Server={server};Port={port};Database={database};Uid={user};Pwd={password};";
        }

        public MySqlConnection Connection => new MySqlConnection(_connStr);
    }
}
