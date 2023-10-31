using System.Text.RegularExpressions;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;

namespace cloudsharpback.Services;

// ReSharper disable InconsistentNaming
public enum RequiredEnvironmentValueKey
{
    CS_VOLUME_PATH,
    MYSQL_SERVER,
    MYSQL_PORT,
    MYSQL_USER,
    MYSQL_PASSWORD,
    REDIS_SERVER,
    REDIS_PORT,
    REDIS_PASSWORD,
}

public class EnvironmentValueStore : IEnvironmentValueStore
{
    public string this[RequiredEnvironmentValueKey key] => GeValueByKey(key);

    private string GeValueByKey(RequiredEnvironmentValueKey key)
    {
        return Environment.GetEnvironmentVariable(key.ToString())!;
    }
    
}