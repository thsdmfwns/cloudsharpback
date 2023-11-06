using System.Security.Cryptography;
using Bogus;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Models.Ticket;
using cloudsharpback.Repository;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Tusd.Protos;
using Microsoft.Extensions.Logging.Abstractions;

namespace cloudsharpback.Test.Tests.Service;

public class TusdHookServiceTest : TestsBase
{
    private IPathStore _pathStore;
    private TusdHookService _tusdHookService;
    private ITicketStore _ticketStore;
    private Faker _faker = null!;
    private MemberDto _member = null!;
    private DirectoryInfo _tusData = null!;

    [SetUp]
    public async Task Setup()
    {
        var env = new EnvironmentValueStore();
        _pathStore = new PathStore(env);
        _ticketStore = new TicketStore(new DBConnectionFactory(env));
        _tusdHookService = new TusdHookService(NullLogger<TusdHookService>.Instance, _pathStore, _ticketStore);
        _faker = new Faker();
        _member = Utils.GetFakeMemberDto(_faker);
        _tusData = new DirectoryInfo(_pathStore.TusStorePath);
        _tusData.Delete();
        var memberDir = new DirectoryInfo(_pathStore.DirectoryPath);
        memberDir.Delete(true);
    }

    private string Hash(byte[] fileBytes)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(fileBytes);
        return Convert.ToBase64String(hash);
    }

    private string Hash(string filePath)
        => Hash(File.ReadAllBytes(filePath));

    private HookRequest GetHookRequest(string type, string ticketToken, bool overwirte, out string id)
    {
        id = _faker.Random.Number().ToString();
        return new HookRequest()
        {
            Type = type,
            Event = new Event()
            {
                Upload = new Tusd.Protos.FileInfo()
                {
                    MetaData = { { "token", ticketToken }, { "overwrite", overwirte.ToString() } },
                    Id =id
                }
            }
        };
    }

    [Test]
    public async Task OnBeforeCreate()
    {
        var dirName = _faker.Internet.UserName();
        var dir = _pathStore.GetMemberTargetPath(_member.Directory, dirName);
        Directory.CreateDirectory(dir);
        var ticket = new UploadTicket
        {
            Owner = _member,
            UploadDirectoryPath = dirName,
            FileName = _faker.System.CommonFileName()
        };
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var req = GetHookRequest("pre-create", ticket.Token.ToString(), false, out _);
        var hookRes = await _tusdHookService.InvokeHook(req, default);
        Assert.That(hookRes.HttpResponse.StatusCode, Is.EqualTo(200));
    }
    
    [Test]
    public async Task OnBeforeCreate_Overwrite()
    {
        var dirName = _faker.Internet.UserName();
        var dir = _pathStore.GetMemberTargetPath(_member.Directory, dirName);
        var filename = _faker.System.CommonFileName();
        Directory.CreateDirectory(dir);
        Utils.MakeFakeFileAtDirectory(_faker, dir, filename);
        var ticket = new UploadTicket
        {
            Owner = _member,
            UploadDirectoryPath = dirName,
            FileName = filename
        };
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var req = GetHookRequest("pre-create", ticket.Token.ToString(), true, out _);
        var hookRes = await _tusdHookService.InvokeHook(req, default);
        Assert.That(hookRes.HttpResponse.StatusCode, Is.EqualTo(200));
    }
    
    [Test]
    public async Task OnBeforeCreate_Fail400()
    {
        // no token
        var dirName = _faker.Internet.UserName();
        var dir = _pathStore.GetMemberTargetPath(_member.Directory, dirName);
        Directory.CreateDirectory(dir);
        var ticket = new UploadTicket
        {
            Owner = _member,
            UploadDirectoryPath = dirName,
            FileName = _faker.System.CommonFileName()
        };
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var req = GetHookRequest("pre-create", string.Empty, false, out _);
        var hookRes = await _tusdHookService.InvokeHook(req, default);
        Assert.That(hookRes.HttpResponse.StatusCode, Is.EqualTo(400));
    }
    
    [Test]
    public async Task OnBeforeCreate_Fail404_NoTicket()
    {
        // not found ticket
        var dirName = _faker.Internet.UserName();
        var dir = _pathStore.GetMemberTargetPath(_member.Directory, dirName);
        Directory.CreateDirectory(dir);
        var ticket = new UploadTicket
        {
            Owner = _member,
            UploadDirectoryPath = dirName,
            FileName = _faker.System.CommonFileName()
        };
        // var res = await _ticketStore.AddTicket(ticket);
        // Assert.That(res, Is.True);
        var req = GetHookRequest("pre-create", ticket.Token.ToString(), false, out _);
        var hookRes = await _tusdHookService.InvokeHook(req, default);
        Assert.That(hookRes.HttpResponse.StatusCode, Is.EqualTo(404));
    }
    
    [Test]
    public async Task OnBeforeCreate_Fail404_NoDIr()
    {
        // not found ticket
        var dirName = _faker.Internet.UserName();
        var dir = _pathStore.GetMemberTargetPath(_member.Directory, dirName);
        var ticket = new UploadTicket
        {
            Owner = _member,
            UploadDirectoryPath = dirName,
            FileName = _faker.System.CommonFileName()
        };
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var req = GetHookRequest("pre-create", ticket.Token.ToString(), false, out _);
        var hookRes = await _tusdHookService.InvokeHook(req, default);
        Assert.That(hookRes.HttpResponse.StatusCode, Is.EqualTo(404));
    }
    
    [Test]
    public async Task OnBeforeCreate_Fail401()
    {
        // no member
        var dirName = _faker.Internet.UserName();
        var dir = _pathStore.GetMemberTargetPath(_member.Directory, dirName);
        Directory.CreateDirectory(dir);
        var ticket = new UploadTicket
        {
            UploadDirectoryPath = dirName,
            FileName = _faker.System.CommonFileName()
        };
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var req = GetHookRequest("pre-create", ticket.Token.ToString(), false, out _);
        var hookRes = await _tusdHookService.InvokeHook(req, default);
        Assert.That(hookRes.HttpResponse.StatusCode, Is.EqualTo(401));
    }
    
    [Test]
    public async Task OnBeforeCreate_Fail409()
    {
        // not found ticket
        var dirName = _faker.Internet.UserName();
        var dir = _pathStore.GetMemberTargetPath(_member.Directory, dirName);
        var filename = _faker.System.CommonFileName();
        Directory.CreateDirectory(dir);
        Utils.MakeFakeFileAtDirectory(_faker, dir, filename);
        var ticket = new UploadTicket
        {
            Owner = _member,
            UploadDirectoryPath = dirName,
            FileName = filename
        };
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var req = GetHookRequest("pre-create", ticket.Token.ToString(), false, out _);
        var hookRes = await _tusdHookService.InvokeHook(req, default);
        Assert.That(hookRes.HttpResponse.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task OnBeforeFinish()
    {
        var dirName = _faker.Internet.UserName();
        var filename = _faker.System.CommonFileName();
        var dir = _pathStore.GetMemberTargetPath(_member.Directory, dirName);
        Directory.CreateDirectory(dir);
        var ticket = new UploadTicket
        {
            Owner = _member,
            UploadDirectoryPath = dirName,
            FileName = filename
        };
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var req = GetHookRequest("pre-finish", ticket.Token.ToString(), false, out var id);
        var file = Utils.MakeFakeFileAtDirectory(_faker, _tusData.FullName, id);
        var originalFileHash = Hash(file);
        var hookRes = await _tusdHookService.InvokeHook(req, default);
        var movedFileHash = Hash(Path.Combine(dir, filename));
        Assert.That(hookRes.HttpResponse.StatusCode, Is.EqualTo(200));
        Assert.That(movedFileHash, Is.EqualTo(originalFileHash));
        Assert.That(File.Exists(file), Is.False);
    }
    
    [Test]
    public async Task OnBeforeFinish_Overwrite()
    {
        var dirName = _faker.Internet.UserName();
        var filename = _faker.System.CommonFileName();
        var dir = _pathStore.GetMemberTargetPath(_member.Directory, dirName);
        Directory.CreateDirectory(dir);
        Utils.MakeFakeFileAtDirectory(_faker, dir, filename);
        var ticket = new UploadTicket
        {
            Owner = _member,
            UploadDirectoryPath = dirName,
            FileName = filename
        };
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var req = GetHookRequest("pre-finish", ticket.Token.ToString(), true, out var id);
        var file = Utils.MakeFakeFileAtDirectory(_faker, _tusData.FullName, id);
        var originalFileHash = Hash(file);
        var hookRes = await _tusdHookService.InvokeHook(req, default);
        var movedFileHash = Hash(Path.Combine(dir, filename));
        Assert.That(hookRes.HttpResponse.StatusCode, Is.EqualTo(200));
        Assert.That(movedFileHash, Is.EqualTo(originalFileHash));
        Assert.That(File.Exists(file), Is.False);
    }
}