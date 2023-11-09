using System.Net;
using System.Text;
using System.Web;
using Bogus;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace cloudsharpback.E2ETest;

public abstract class TestBase
{
    protected const string BackEndServerPath = "cs_backend";
    protected const int BackEndServerPort = 80;
    protected IDBConnectionFactory _dbConnectionFactory;
    protected IPathStore _path;
    protected IEnvironmentValueStore _environmentValue;
    protected Faker _faker;

    public virtual async Task SetUp()
    {
        _environmentValue = new EnvironmentValueStore();
        _dbConnectionFactory = new DBConnectionFactory(_environmentValue);
        _faker = new Faker();
        _path = new PathStore(_environmentValue);
        await InitDb();
        InitVolume();
        await RegisterMaster();
    }

    private void InitVolume()
    {
        var volPath = _environmentValue[RequiredEnvironmentValueKey.CS_VOLUME_PATH];
        var volume = new DirectoryInfo(volPath);
        if (!volume.Exists)
        {
            Directory.CreateDirectory(volPath);
            return;
        }
        volume.GetDirectories().ToList().ForEach(x => x.Delete(true));
        volume.GetFiles().ToList().ForEach(x => x.Delete());
    }

    private async Task InitDb()
    {
        await DeleteMemberTable();
        await DeleteShareTable();
    }
    
    protected const string MasterId = "master";
    protected const string MasterPw = "master";

    private async Task RegisterMaster()
    {
        var regDto = new
        {
            id = MasterId,
            pw = MasterPw,
            email = _faker.Internet.Email(),
            nick = _faker.Internet.UserName()
        };
        var regRes = await PostAsync("/api/User/register", regDto);
        Assert.That(regRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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

    private HttpContent EmptyContent => new StringContent(string.Empty);

    private enum HttpRequestType
    {
        Get,
        Post
    }

    private HttpContent JsonContent(object content)
    {
        var serializeOption = new JsonSerializerSettings();
        serializeOption.ContractResolver = new CamelCasePropertyNamesContractResolver();
        var json = JsonConvert.SerializeObject(content, serializeOption);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
    

    public async Task<HttpResponseMessage> GetAsync(
        string path,
        object? qurey = null,
        object? header = null)
        => await SendRequest(path, HttpRequestType.Get, null, qurey, header);

    public async Task<HttpResponseMessage> PostAsync(
        string path,
        object? content = null,
        object? qurey = null,
        object? header = null)
        => await PostAsync(path, content is null ? EmptyContent : JsonContent(content), qurey, header);

    public async Task<HttpResponseMessage> PostAsync(
        string path,
        HttpContent body,
        object? qurey = null,
        object? header = null)
        => await SendRequest(path, HttpRequestType.Post, body, qurey, header);

    private async Task<HttpResponseMessage> SendRequest(
        string path,
        HttpRequestType requestType,
        HttpContent? body,
        object? qurey = null,
        object? header = null)
    {
        var uriBuilder = new UriBuilder(new Uri(new Uri($"http://{BackEndServerPath}:{BackEndServerPort}"), path));
        var q = HttpUtility.ParseQueryString(uriBuilder.Query);
        qurey?.GetType().GetProperties().ToList().ForEach(x => q[x.Name] = x.GetValue(qurey)?.ToString());
        uriBuilder.Query = q.ToString();
        using var httpClient = new HttpClient();
        header?.GetType().GetProperties().ToList().ForEach(x
            => httpClient.DefaultRequestHeaders.Add(x.Name, x.GetValue(header)?.ToString()));
        var cancellationToken = new CancellationTokenSource();
        return requestType switch
        {
            HttpRequestType.Post => await httpClient.PostAsync(uriBuilder.ToString(), body ?? EmptyContent,
                cancellationToken.Token),
            HttpRequestType.Get => await httpClient.GetAsync(uriBuilder.ToString(), cancellationToken.Token),
            _ => throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null)
        };
    }
    
    public string MakeFakeFileAtDirectory(string dirPath, string? filename = null)
    {
        var fileName = filename ??_faker.System.CommonFileName();
        var fileContent = _faker.Lorem.Paragraphs();
        Directory.CreateDirectory(dirPath);
        var path = Path.Combine(dirPath, fileName);
        File.WriteAllText(path, fileContent);
        return path;
    }
}