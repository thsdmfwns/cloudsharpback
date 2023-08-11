using cloudsharpback.Services.Interfaces;
using MySql.Data.MySqlClient;
using NSubstitute;

namespace cloudsharpback.Test;

public static class DBConnectionFactoryMock
{
    public static IDBConnectionFactory Mock => GetIdbConnectionFactoryMock();
    private const string connString = "Server=cs_db;Port=3306;Uid=root;Pwd=3279;Database=cloud_sharp;";
    private static IDBConnectionFactory GetIdbConnectionFactoryMock()
    {
        var mock = Substitute.For<IDBConnectionFactory>();
        mock.Connection.Returns(new MySqlConnection(connString));
        return mock;
    }
}