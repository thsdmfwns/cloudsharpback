using cloudsharpback.Services.Interfaces;
using Moq;
using MySql.Data.MySqlClient;

namespace cloudsharpback.Test;

public static class DBConnectionFactoryMock
{
    public static Mock<IDBConnectionFactory> Mock => GetIdbConnectionFactoryMock();
    private const string connString = "Server=cs_db;Port=3306;Uid=root;Pwd=3279;Database=cloud_sharp;";
    private static Mock<IDBConnectionFactory> GetIdbConnectionFactoryMock()
    {
        var mock = new Mock<IDBConnectionFactory>();
        mock.Setup(x => x.Connection).Returns(new MySqlConnection(connString));
        return mock;
    }
}