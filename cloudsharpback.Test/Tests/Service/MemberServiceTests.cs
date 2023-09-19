using Bogus;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Repository;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace cloudsharpback.Test.Tests.Service;

public class MemberServiceTests : TestsBase
{
    private MemberService _service = null!;
    private IMemberRepository _repositoryStub = null!;
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
        _repositoryStub = Substitute.For<IMemberRepository>();
        _service = new MemberService(NullLogger<IMemberService>.Instance, _pathStore, _repositoryStub);
    }

    [Test]
    public async Task ProfileImage()
    {
        //upload
        var filePath = Utils.MakeFakeFile(_faker, _memberDto.Directory, null, "png", true);
        var filename = Path.GetFileName(filePath);
        using var stream = File.OpenRead(filePath);
        var formfile = new FormFile(stream, 0, stream.Length, filename, filename);
        var profileId = Guid.NewGuid();
        var profileName = $"{profileId}.png";
        _repositoryStub.TryUpdateMemberProfileImage(_memberDto.Id, profileName).Returns(true);
        var uploadRes = await _service.UploadProfileImage(formfile, _memberDto, profileId);
        Assert.That(uploadRes, Is.Null);
        var savedPath = Path.Combine(_pathStore.ProfilePath, profileName);
        Assert.That(File.Exists(savedPath), Is.True);
        
        //dl
        var dlRes = _service.DownloadProfileImage(profileName, out var fileStream, out var contentType);
        Assert.That(dlRes, Is.Null);
        Assert.That(fileStream, Is.Not.Null);
        Assert.That(contentType, Is.Not.Null);
        Assert.That(fileStream!.Name, Is.EqualTo(savedPath));
    }
    
    [Test]
    public async Task UploadProfileImage_fail()
    {
        var filePath = Utils.MakeFakeFile(_faker, _memberDto.Directory, null, "abcdefg", true);
        var filename = Path.GetFileName(filePath);
        using var stream = File.OpenRead(filePath);
        var formfile = new FormFile(stream, 0, stream.Length, filename, filename);
        var profileId = Guid.NewGuid();
        var profileName = $"{profileId}.png";
        _repositoryStub.TryUpdateMemberProfileImage(_memberDto.Id, profileName).Returns(true);
        var res = await _service.UploadProfileImage(formfile, _memberDto, profileId);
        Assert.That(res, Is.Not.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(415));
    }

    [Test]
    public void DlProfileImage_fail()
    {
        var res = _service.DownloadProfileImage(_faker.System.FileName(), out var stream, out var contentType);
        Assert.That(res, Is.Not.Null);
        Assert.That(stream, Is.Null);
        Assert.That(contentType, Is.Null);
        Assert.That(res!.HttpCode, Is.EqualTo(404));
    }
    
}