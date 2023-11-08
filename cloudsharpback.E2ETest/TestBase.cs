using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Dapper;

namespace cloudsharpback.E2ETest;

public abstract class TestBase
{
    protected const string BackEndServerPath = "cs_backend";
    protected const int BackEndServerPort = 80;
    protected IDBConnectionFactory _dbConnectionFactory;
    protected IPathStore _path;
    protected IEnvironmentValueStore _environmentValue;
    public virtual async Task SetUp()
    {
        _environmentValue = new EnvironmentValueStore();
        _dbConnectionFactory = new DBConnectionFactory(_environmentValue);
        _path = new PathStore(_environmentValue);
        await InitDb();
        InitVolume();
    }

    private void InitVolume()
    {
        var volPath = _environmentValue[RequiredEnvironmentValueKey.CS_VOLUME_PATH];
        var volume = new DirectoryInfo(volPath);
        if (!volume.Exists)
        {
            Directory.CreateDirectory(volPath);
        }
        volume.GetDirectories().ToList().ForEach(x => x.Delete(true));
        volume.GetFiles().ToList().ForEach(x => x.Delete());
    }

    private async Task InitDb()
    {
        await DeleteMemberTable();
        await DeleteShareTable();
    }

    private async Task DeleteMemberTable()
    {
        const string sql = "DELETE FROM member";
        await using var conn = _dbConnectionFactory.MySqlConnection;
        await conn.ExecuteAsync(sql);
    }
    
    private async Task DeleteShareTable()
    {
        const string sql = "DELETE FROM share";
        await using var conn = _dbConnectionFactory.MySqlConnection;
        await conn.ExecuteAsync(sql);
    }
}