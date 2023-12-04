using System.Net;
using cloudsharpback.Models.DTO.FIle;
using cloudsharpback.Models.DTO.Member;
using Grpc.Net.Client.Balancer;
using Newtonsoft.Json;
using TusSharp;

namespace cloudsharpback.E2ETest;

public class FileApiTest : TestBase
{
    private List<string> _files = null!;
    private (MemberDto member, string ac, string rf) _master;
    private string makeDirName = null!;
    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        makeDirName = Guid.NewGuid().ToString();
        _files = new();
        _master = await GetMaster();
        for (int i = 0; i < 5; i++)
        {
            var file = MakeFakeFileInMemberDir(_master.member.Directory, "test");
            _files.Add(file);
        }
    }
    
    [Test]
    public async Task FilesList()
    {
        var header = new { auth = _master.ac };
        var res = await GetAsync("/api/File/ls", header: header, qurey: new { path = "test" });
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = JsonConvert.DeserializeObject<List<FileInfoDto>>(await res.Content.ReadAsStringAsync());
        var results = content!.Select(x => x.Path).ToList();
        results.Sort();
        _files.Sort();
        Assert.That(JsonConvert.SerializeObject(results), Is.EqualTo(JsonConvert.SerializeObject(_files)));
    }

    [Test]
    public async Task DeleteFile()
    {
        var header = new { auth = _master.ac };
        var res = await PostAsync("api/File/rm", header: header, qurey: new { path = _files.First() });
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = JsonConvert.DeserializeObject<List<FileInfoDto>>(await res.Content.ReadAsStringAsync());
        var results = content!.Select(x => x.Path).ToList();
        results.Sort();
        _files.RemoveAt(0);
        _files.Sort();
        Assert.That(JsonConvert.SerializeObject(results), Is.EqualTo(JsonConvert.SerializeObject(_files)));
    }
    
    [Test]    
    public async Task MakeDir()
    {
        var header = new { auth = _master.ac };
        var res = await PostAsync("api/File/mkdir", header: header, qurey: new
        {
            rootDir = "test",
            dirName = makeDirName
        });
        var content = JsonConvert.DeserializeObject<List<FileInfoDto>>(await res.Content.ReadAsStringAsync());
        var results = content!.Select(x => x.Path).ToList();
        results.Sort();
        _files.Add($"test/{makeDirName}");
        _files.Sort();
        Assert.That(JsonConvert.SerializeObject(results), Is.EqualTo(JsonConvert.SerializeObject(_files)));
    }

    [Test]
    public async Task MakeDirAndRemoveDir()
    {
        await MakeDir();
        var header = new { auth = _master.ac };
        var res = await PostAsync("api/File/rmdir", header: header, qurey: new { path = $"test/{makeDirName}" });
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = JsonConvert.DeserializeObject<List<FileInfoDto>>(await res.Content.ReadAsStringAsync());
        var results = content!.Select(x => x.Path).ToList();
        results.Sort();
        _files.Remove($"test/{makeDirName}");
        _files.Sort();
        Assert.That(JsonConvert.SerializeObject(results), Is.EqualTo(JsonConvert.SerializeObject(_files)));
    }

    [Test]
    public async Task GetTicketAndDownload()
    {
        var header = new { auth = _master.ac };
        var file = MakeFakeFileInMemberDir(_master.member.Directory, null);
        var fullpath = _path.GetMemberTargetPath(_master.member.Directory, file);
        var ticketRes = await GetAsync("api/File/dlTicket", header:header, qurey: new {path = file});
        Assert.That(ticketRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var ticketToken = await ticketRes.Content.ReadAsStringAsync();
        var dlRes = await GetAsync($"dl/{ticketToken}");
        Assert.That(dlRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var dlHash = GetHash(await dlRes.Content.ReadAsByteArrayAsync());
        var ogHash = GetHash(await File.ReadAllBytesAsync(fullpath));
        Assert.That(dlHash, Is.EqualTo(ogHash));
    }

    [Test]
    public async Task GetTIcketAndUpload()
    {
        var header = new { auth = _master.ac };
        var file = new FileInfo(TestImage1Path);
        Assert.That(file.Exists, Is.True);
        var requestContent = new
        {
            fileName = file.Name,
            uploadDirectory = ""
        };
        var ticketRes = await PostAsync("api/File/ulTicket", requestContent, header: header);
        Assert.That(ticketRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var ticketToken = await ticketRes.Content.ReadAsStringAsync();
        await using var fs = File.OpenRead(file.FullName);
        var client = new TusClient();
        var err = string.Empty;
        var opt = new TusUploadOption()
        {
            EndPoint = new Uri("http://cs_tusd:8080/files"),
            ChunkSize = 1 * 1024 * 1024, //1MB
            RetryDelays = new List<int>(),
            MetaData = new Dictionary<string, string>()
            {
                {"token", ticketToken}
            },
            OnFailed = (originalResponseMsg, originalRequestMsg, errMsg, exception) =>
            {
                Console.WriteLine($"upload failed beacuse : {errMsg} \n");
                err += errMsg;
            },
        };
        using var upload = client.Upload(opt, fs);
        await upload.Start();
        Assert.That(err, Is.EqualTo(""));
        var target = new FileInfo(_path.GetMemberTargetPath(_master.member.Directory, Path.Combine(requestContent.uploadDirectory, requestContent.fileName)));
        Assert.That(target.Exists, Is.True);
        var targetHash = GetHash(await File.ReadAllBytesAsync(target.FullName));
        var testImageHash = GetHash(await File.ReadAllBytesAsync(TestImage1Path));
        Assert.That(targetHash, Is.EqualTo(testImageHash));
    }
}