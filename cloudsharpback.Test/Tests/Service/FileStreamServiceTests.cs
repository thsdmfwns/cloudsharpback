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
        Directory.Delete(env[RequiredEnvironmentValueKey.CS_VOLUME_PATH]);
    }

    [Test]
    public void GetFileStream()
    {
        var memberDir = Guid.NewGuid().ToString();
        var fileName = _faker.System.CommonFileName();
        var fileContent = _faker.Lorem.Sentences();
        var fileDir = _pathStore.MemberDirectory(memberDir);
        var filePath = Path.Combine(fileDir, fileName);
        Directory.CreateDirectory(fileDir);
        
        File.WriteAllText(filePath, fileContent);

        var ticketValue = new FileDownloadTicketValue()
        {
            FileDownloadType = FileDownloadType.Download,
            TargetFilePath = filePath
        };

        var ticket = new Ticket(_faker.Internet.Ip(), null, TicketType.Download, ticketValue);

        var res = _service.GetFileStream(ticket, out var fileStream);
        Assert.That(res, Is.Null);
        Assert.That(fileStream, Is.Not.Null);
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