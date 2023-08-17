using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using MySql.Data.MySqlClient;
using NSubstitute;

namespace cloudsharpback.Test.Tests.Repo;

public static class DBConnectionFactoryMock
{
    public static IDBConnectionFactory Mock = GetIdbConnectionFactoryMock();
    private static IDBConnectionFactory GetIdbConnectionFactoryMock()
    {
        var env = new EnvironmentValueStore();
        return new DBConnectionFactory(env);
    }
}