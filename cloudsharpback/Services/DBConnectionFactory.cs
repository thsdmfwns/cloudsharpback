using cloudsharpback.Services.Interfaces;
using MySql.Data.MySqlClient;
using StackExchange.Redis;

namespace cloudsharpback.Services
{
    public class DBConnectionFactory : IDBConnectionFactory
    {
        private readonly string _mysqlConnStr;
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        public DBConnectionFactory(IEnvironmentValueStore environmentValueStore)
        {
            var mysqlDatabase = "cloud_sharp";
            var mysqlPort = environmentValueStore[RequiredEnvironmentValueKey.MYSQL_PORT];
            var mysqlServer = environmentValueStore[RequiredEnvironmentValueKey.MYSQL_SERVER];
            var mysqlPassword = environmentValueStore[RequiredEnvironmentValueKey.MYSQL_PASSWORD];
            var mysqlUser = environmentValueStore[RequiredEnvironmentValueKey.MYSQL_USER];
            var redisServer = environmentValueStore[RequiredEnvironmentValueKey.REDIS_SERVER];
            var redisPort = environmentValueStore[RequiredEnvironmentValueKey.REDIS_PORT];
            var redisPassword = environmentValueStore[RequiredEnvironmentValueKey.REDIS_PASSWORD];

            //Server=<server_address>;Port=<port_number>;Database=<database_name>;Uid=<username>;Pwd=<password>;

            _mysqlConnStr = $"Server={mysqlServer};Port={mysqlPort};Database={mysqlDatabase};Uid={mysqlUser};Pwd={mysqlPassword};";
            var redisOption = ConfigurationOptions.Parse($"{redisServer}:{redisPort}");
            redisOption.Password = redisPassword;
            _connectionMultiplexer = ConnectionMultiplexer.Connect(redisOption);
        }

        public MySqlConnection MySqlConnection => new MySqlConnection(_mysqlConnStr);
        public IDatabase Redis =>  _connectionMultiplexer.GetDatabase();
    }
}
