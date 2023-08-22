using Bogus;
using cloudsharpback.Models;
using cloudsharpback.Models.DTO.FIle;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace cloudsharpback.Test.Tests.Service;

public class MemberFileServiceTests : TestsBase
{
    private MemberFileService _service = null!;
    private Faker _faker = null!;
    private PathStore _pathStore = null!;
    private MemberDto _memberDto = null!;

    [SetUp]
    public void SetUp()
    {
        var env = new EnvironmentValueStore();
        var volPath = env[RequiredEnvironmentValueKey.CS_VOLUME_PATH];
        var volDir = new DirectoryInfo(volPath);
        if (volDir.Exists)
        {
            volDir.Delete(true);
        }
        _faker = new Faker();
        _memberDto = Utils.GetFakeMemberDto(_faker);
        _pathStore = new PathStore(env);
        _service = new MemberFileService(NullLogger<IMemberFileService>.Instance, _pathStore);
    }

    [Test]
    public void MakeBaseDirectory()
    {
        var res = _service.GetFiles(_memberDto, null, out var files);
        Assert.That(res, Is.Null);
        var names = files
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToList();
        var targets = new List<string>() 
            { "Download", "Music", "Video", "Document" }
            .OrderBy(x => x)
            .ToList();
        
        Assert.That(Utils.ToJson(names), Is.EqualTo(Utils.ToJson(targets)));
    }

    [Test]
    public void GerFiles()
    {
        var filePath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_memberDto.Directory), null);
        var res = _service.GetFiles(_memberDto, null, out var files);
        Assert.That(res, Is.Null);
        var names = files
            .Select(x => x.Name)
            .ToList();
        Assert.That(names.Single(), Is.EqualTo(Path.GetFileName(filePath)));
        
        //fail
        res = _service.GetFiles(_memberDto, _faker.System.DirectoryPath().TrimStart('/'), out files);
        Assert.That(res, Is.Not.Null);
        Assert.That(files, Is.Empty);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
    }

    [Test]
    public void DeleteFile()
    {
        var filePath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_memberDto.Directory), null);
        var res = _service.DeleteFile(_memberDto, filePath, out var files);
        Assert.That(res, Is.Null);
        Assert.That(files, Is.Empty);
        
        //fail
        res = _service.DeleteFile(_memberDto, _faker.System.FilePath().TrimStart('/'), out files);
        Assert.That(res, Is.Not.Null);
        Assert.That(files, Is.Empty);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
    }

    [Test]
    public void GetDownloadTicketValue_DL()
    {
        var filePath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_memberDto.Directory), null);
        var res = _service.GetDownloadTicketValue(_memberDto, filePath, out var ticketValue);
        Assert.That(res, Is.Null);
        Assert.That(ticketValue, Is.Not.Null);
        var targetTicket = new FileDownloadTicketValue()
        {
            FileDownloadType = FileDownloadType.Download,
            TargetFilePath = Path.Combine(_pathStore.MemberDirectory(_memberDto.Directory), filePath),
        };
        Assert.That(Utils.ClassToJson(ticketValue!), Is.EqualTo(Utils.ClassToJson(targetTicket)));
        
        //fail
        res = _service.GetDownloadTicketValue(_memberDto, _faker.System.FilePath().TrimStart('/'), out ticketValue);
        Assert.That(res, Is.Not.Null);
        Assert.That(ticketValue, Is.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
    }
    
    [Test]
    public void GetDownloadTicketValue_View()
    {
        var filePath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_memberDto.Directory), null, "png");
        var res = _service.GetDownloadTicketValue(_memberDto, filePath, out var ticketValue, true);
        Assert.That(res, Is.Null);
        Assert.That(ticketValue, Is.Not.Null);
        var targetTicket = new FileDownloadTicketValue()
        {
            FileDownloadType = FileDownloadType.View,
            TargetFilePath = Path.Combine(_pathStore.MemberDirectory(_memberDto.Directory), filePath),
        };
        Assert.That(Utils.ClassToJson(ticketValue!), Is.EqualTo(Utils.ClassToJson(targetTicket)));
        
        //fail
        res = _service.GetDownloadTicketValue(_memberDto, _faker.System.FilePath().TrimStart('/'), out ticketValue);
        Assert.That(res, Is.Not.Null);
        Assert.That(ticketValue, Is.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(404));

        filePath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_memberDto.Directory), null, ext: "aabbcc");
        res = _service.GetDownloadTicketValue(_memberDto, filePath, out ticketValue, true);
        Assert.That(res, Is.Not.Null);
        Assert.That(ticketValue, Is.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(415));
    }

    [Test]
    public void GetUploadTicketValue()
    {
        var fileName = _faker.System.CommonFileName();
        var filePath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_memberDto.Directory), null);
        var uploadreq = new FileUploadRequestDto()
        {
            FileName = fileName,
            UploadDirectory = null
        };
        var res = _service.GetUploadTicketValue(_memberDto, uploadreq, out var ticketValue);
        var target =  new FileUploadTicketValue()
        {
            FileName = fileName,
            UploadDirectoryPath = _pathStore.MemberDirectory(_memberDto.Directory)
        };
        Assert.That(res, Is.Null);
        Assert.That(ticketValue, Is.Not.Null);
        Assert.That(Utils.ClassToJson(ticketValue!), Is.EqualTo(Utils.ClassToJson(target)));
        
        //fail
        uploadreq = new FileUploadRequestDto()
        {
            FileName = Path.GetFileName(filePath),
            UploadDirectory = null
        };
        res = _service.GetUploadTicketValue(_memberDto, uploadreq, out ticketValue);
        Assert.That(res, Is.Not.Null);
        Assert.That(ticketValue, Is.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(409));
        
        uploadreq = new FileUploadRequestDto()
        {
            FileName = _faker.System.CommonFileName(),
            UploadDirectory = _faker.System.DirectoryPath().TrimStart('/')
        };
        res = _service.GetUploadTicketValue(_memberDto, uploadreq, out ticketValue);
        Assert.That(res, Is.Not.Null);
        Assert.That(ticketValue, Is.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
    }

    [Test]
    public void MakeDirectory()
    {
        var dirName = _faker.Random.Word();
        var res = _service.MakeDirectory(_memberDto, null, dirName, out var files);
        var names = files
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToList();
        var targets = new List<string>() 
                { "Download", "Music", "Video", "Document", dirName }
            .OrderBy(x => x)
            .ToList();
        Assert.That(Utils.ToJson(names), Is.EqualTo(Utils.ToJson(targets)));
        
        //fails
        res = _service.MakeDirectory(_memberDto, null, _faker.Random.String(), out files);
        Assert.That(res, Is.Not.Null);
        Assert.That(files, Is.Empty);
        Assert.That(res!.HttpCode, Is.EqualTo(400));
        
        res = _service.MakeDirectory(_memberDto, null, dirName, out files);
        Assert.That(res, Is.Not.Null);
        Assert.That(files, Is.Empty);
        Assert.That(res!.HttpCode, Is.EqualTo(409));
        
        res = _service.MakeDirectory(_memberDto, Guid.NewGuid().ToString(), dirName, out files);
        Assert.That(res, Is.Not.Null);
        Assert.That(files, Is.Empty);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
    }

    [Test]
    public void RemoveDirectory()
    {
        var res = _service.RemoveDirectory(_memberDto, "Download", out var files);
        var targets = new List<string>() 
                { "Music", "Video", "Document" }
            .OrderBy(x => x)
            .ToList();
        var names = files
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToList();
        Assert.That(Utils.ToJson(names), Is.EqualTo(Utils.ToJson(targets)));
        
        //fails
        
        res = _service.RemoveDirectory(_memberDto, "/", out files);
        Assert.That(res, Is.Not.Null);
        Assert.That(files, Is.Empty);
        Assert.That(res!.HttpCode, Is.EqualTo(400));
        
        res = _service.RemoveDirectory(_memberDto, "Download", out files);
        Assert.That(res, Is.Not.Null);
        Assert.That(files, Is.Empty);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
    }
    
}