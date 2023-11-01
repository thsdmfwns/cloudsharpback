using Bogus;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Models.Ticket;
using cloudsharpback.Repository;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Newtonsoft.Json;

namespace cloudsharpback.Test.Tests.Repo;

public class TIcketStoreTest : TestsBase
{
    private Faker _faker = null!;
    private MemberDto _member = null!;
    private ITicketStore _ticketStore = null!;
    private IPathStore _pathStore = null!;

    [SetUp]
    public void Setup()
    {
        _faker = new Faker();
        _member = Utils.GetFakeMemberDto(_faker);
        _ticketStore = new TicketStore(DBConnectionFactoryMock.Mock);
        _pathStore = new PathStore(new EnvironmentValueStore());
    }

    private async Task AddAndGet<T> (ITicket<T> ticket) where T : ITicket<T>
    {
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var redisTicket = await _ticketStore.GetTicket<T>(ticket.Token);
        Assert.That(redisTicket, Is.Not.Null);
        Assert.That(JsonConvert.SerializeObject(redisTicket), Is.EqualTo(JsonConvert.SerializeObject(ticket)));
    }

    private async Task AddAndExist<T> (ITicket<T> ticket) where T : ITicket<T>
    {
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        var redisTicket = await _ticketStore.ExistTicket<T>(ticket.Token);
        Assert.That(redisTicket, Is.True);
    }

    private async Task AddAndRemoveAndGet<T> (ITicket<T> ticket) where T : ITicket<T>
    {
        var res = await _ticketStore.AddTicket(ticket);
        Assert.That(res, Is.True);
        res = await _ticketStore.RemoveTicket(ticket);
        Assert.That(res, Is.True);
        var redisTicket = await _ticketStore.GetTicket<T>(ticket.Token);
        Assert.That(redisTicket, Is.Null);
    }
    
    private DownloadTicket GetDownloadTicket()
    {
        var filePath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_member.Directory), null);
        var ticket = new DownloadTicket
        {
            TargetFilePath = filePath,
            FileDownloadType = FileDownloadType.Download,
            ExpireTime = default,
            Token = default,
            Owner = _member
        };
        return ticket;
    }
    
    private UploadTicket GetUploadTicket()
    {
        var filePath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_member.Directory), null);
        var ticket = new UploadTicket
        {
            ExpireTime = default,
            Token = default,
            Owner = _member,
            UploadDirectoryPath = filePath,
            FileName = Path.GetFileName(filePath)
        };
        return ticket;
    }
    
    private SignalrTicket GetSignalrTicket()
    {
        var ticket = new SignalrTicket()
        {
            ExpireTime = default,
            Token = default,
            Owner = _member,
        };
        return ticket;
    }

    [Test]
    public async Task AddAndGet_Download()
    {
        var ticket = GetDownloadTicket();
        await AddAndGet(ticket);
    }
    
    [Test]
    public async Task AddAndGet_Upload()
    {
        var ticket = GetUploadTicket();
        await AddAndGet(ticket);
    }

    [Test]
    public async Task AddAndGet_Signalr()
    {
        var ticket = GetSignalrTicket();
        await AddAndGet(ticket);
    }

    [Test]
    public async Task AddAndRemoveAndGet_Download()
    {
        var ticket = GetDownloadTicket();
        await AddAndRemoveAndGet(ticket);
    }
    
    [Test]
    public async Task AddAndRemoveAndGet_Upload()
    {
        var ticket = GetUploadTicket();
        await AddAndRemoveAndGet(ticket);
    }
    
    [Test]
    public async Task AddAndRemoveAndGet_Signalr()
    {
        var ticket = GetSignalrTicket();
        await AddAndRemoveAndGet(ticket);
    }
    
    [Test]
    public async Task AddAndExist_Download()
    {
        var ticket = GetDownloadTicket();
        await AddAndExist(ticket);
    }
    
    [Test]
    public async Task AddAndExist_Upload()
    {
        var ticket = GetUploadTicket();
        await AddAndExist(ticket);
    }
    
    [Test]
    public async Task AddAndExist_Signalr()
    {
        var ticket = GetSignalrTicket();
        await AddAndExist(ticket);
    }

}