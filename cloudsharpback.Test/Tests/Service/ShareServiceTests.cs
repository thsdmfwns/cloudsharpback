using Bogus;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Models.DTO.Share;
using cloudsharpback.Models.Ticket;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace cloudsharpback.Test.Tests.Service;

public class ShareServiceTests : TestsBase
{
    private ShareService _service = null!;
    private IShareRepository _repositoryStub = null!;
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
        _pathStore = new PathStore(env);
        _memberDto = Utils.GetFakeMemberDto(_faker);
        _repositoryStub = Substitute.For<IShareRepository>();
        _service = new ShareService(NullLogger<IShareService>.Instance, _pathStore, _repositoryStub);
    }

    [Test]
    public async Task Share()
    {
        var filepath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_memberDto.Directory), null);
        var fileFullPath = _pathStore.GetMemberTargetPath(_memberDto.Directory, filepath);
        var req = new ShareRequestDto()
        {
            Target = filepath
        };
        var token = Guid.NewGuid();
        var fileInfo = new FileInfo(fileFullPath);
        _repositoryStub
            .TryAddShare(
                _memberDto.Id,
                req.Target,
                null,
                req.ExpireTime ?? (ulong)DateTime.MaxValue.Ticks,
                req.Comment,
                req.ShareName,
                token,
                (ulong)fileInfo.Length
                )
            .Returns(true);
        var res = await _service.Share(_memberDto, req, token);
        Assert.That(res, Is.Null);
    }

    [Test]
    public async Task Share_fail()
    {
        var req = new ShareRequestDto()
        {
            Target = _faker.System.FileName()
        };
        var res = await _service.Share(_memberDto, req);
        Assert.That(res, Is.Not.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetDownloadTicketValue()
    {
        var filePath = Utils.MakeFakeFile(_faker, _pathStore.MemberDirectory(_memberDto.Directory), null);
        var fileFullPath = _pathStore.GetMemberTargetPath(_memberDto.Directory, filePath);
        var token = Guid.NewGuid();
        var password = _faker.Internet.Password();
        var req = new ShareDowonloadRequestDto()
        {
            Token = token.ToString(),
            Password = password
        };
        _repositoryStub.GetShareDownloadDtoByToken(token).Returns(new ShareDownloadDto()
        {
            Directory = _memberDto.Directory,
            ExpireTime = (ulong)DateTime.MaxValue.Ticks,
            Password = PasswordEncrypt.EncryptPassword(password),
            Target = filePath
        });
        var res = await _service.GetDownloadTicketValue(req, _memberDto);
        Assert.That(res.err, Is.Null);
        Assert.That(res.ticket, Is.Not.Null);
        Assert.That(res.ticket!.TargetFilePath, Is.EqualTo(fileFullPath));
        
        //fail
        
        _repositoryStub.GetShareDownloadDtoByToken(token).Returns(new ShareDownloadDto()
        {
            Directory = _memberDto.Directory,
            ExpireTime = (ulong)DateTime.MaxValue.Ticks,
            Password = PasswordEncrypt.EncryptPassword(_faker.Internet.Password()),
            Target = filePath
        });
        res = await _service.GetDownloadTicketValue(req, _memberDto);
        Assert.That(res.err, Is.Not.Null);
        Assert.That(res.ticket, Is.Null);
        Assert.That(res.err!.HttpCode, Is.EqualTo(403));
        
        _repositoryStub.GetShareDownloadDtoByToken(token).Returns(new ShareDownloadDto()
        {
            Directory = _memberDto.Directory,
            ExpireTime = (ulong)DateTime.UtcNow.Ticks,
            Password = PasswordEncrypt.EncryptPassword(password),
            Target = filePath
        });
        res = await _service.GetDownloadTicketValue(req, _memberDto);
        Assert.That(res.err, Is.Not.Null);
        Assert.That(res.ticket, Is.Null);
        Assert.That(res.err!.HttpCode, Is.EqualTo(410));
        
        _repositoryStub.GetShareDownloadDtoByToken(token).Returns(new ShareDownloadDto()
        {
            Directory = _memberDto.Directory,
            ExpireTime = (ulong)DateTime.MaxValue.Ticks,
            Password = PasswordEncrypt.EncryptPassword(password),
            Target = _faker.System.FileName()
        });
        res = await _service.GetDownloadTicketValue(req, _memberDto);
        Assert.That(res.err, Is.Not.Null);
        Assert.That(res.ticket, Is.Null);
        Assert.That(res.err!.HttpCode, Is.EqualTo(404));
    }
    
}