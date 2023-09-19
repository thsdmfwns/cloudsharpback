using Bogus;
using cloudsharpback.Models;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace cloudsharpback.Test.Tests.Service;

public class FileStreamServiceTests : TestsBase
{
    private FileStreamService _service = null!;
    private PathStore _pathStore = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        var env = new EnvironmentValueStore();
        _pathStore = new PathStore(env);
        _service = new FileStreamService(_pathStore, new NullLogger<IFileStreamService>());
        _faker = new Faker();
        var volPath = env[RequiredEnvironmentValueKey.CS_VOLUME_PATH];
        var volDir = new DirectoryInfo(volPath);
        if (volDir.Exists)
        {
            volDir.Delete(true);
        }
    }

    [Test]
    public void GetFileStream()
    {
        var memberDir = _pathStore.MemberDirectory(Guid.NewGuid().ToString());
        var filePath = Utils.MakeFakeFile(_faker, memberDir, null, null, true);
        var ticketValue = new FileDownloadTicketValue()
        {
            FileDownloadType = FileDownloadType.Download,
            TargetFilePath = filePath
        };

        var ticket = new Ticket(_faker.Internet.Ip(), null, TicketType.Download, ticketValue);

        var res = _service.GetFileStream(ticket, out var fileStream);
        Assert.That(res, Is.Null);
        Assert.That(fileStream, Is.Not.Null);
        Assert.That(fileStream!.CanRead, Is.True);
        Assert.That(fileStream!.Name, Is.EqualTo(filePath));
        
        //fail
        var fakeTicket = new Ticket(_faker.Internet.Ip(), null, TicketType.Download, null);
        res = _service.GetFileStream(fakeTicket, out fileStream);
        Assert.That(res, Is.Not.Null);
        Assert.That(fileStream, Is.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
        
        File.Delete(filePath);
        res = _service.GetFileStream(ticket, out fileStream);
        Assert.That(res, Is.Not.Null);
        Assert.That(fileStream, Is.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
    }
}