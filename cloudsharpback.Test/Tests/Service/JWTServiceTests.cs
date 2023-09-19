using Bogus;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace cloudsharpback.Test.Tests.Service;

public class JWTServiceTests : TestsBase
{
    private JWTService _service = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _faker = new Faker();
        _service = new JWTService(NullLogger<IJWTService>.Instance);
    }

    [Test]
    public void AccessToken()
    {
        //write
        var member = Utils.GetFakeMemberDto(_faker);
        var token = _service.WriteAccessToken(member);
        Assert.That(string.IsNullOrEmpty(token), Is.False);
        
        //validate
        var res = _service.TryValidateAccessToken(token, out var dto);
        Assert.That(res, Is.True);
        Assert.That(dto, Is.Not.Null);
        Assert.That(Utils.ClassToJson(dto!), Is.EqualTo(Utils.ClassToJson(member)));
        
        //key validate
        var service = new JWTService(NullLogger<IJWTService>.Instance);
        res = service.TryValidateAccessToken(token, out dto);
        Assert.That(res, Is.False);
        Assert.That(dto, Is.Null);
        
        var newToken = service.WriteAccessToken(member);
        res = _service.TryValidateAccessToken(newToken, out dto);
        Assert.That(res, Is.False);
        Assert.That(dto, Is.Null);
    }
    
    [Test]
    public void RefreshToken()
    {
        //write
        var member = Utils.GetFakeMemberDto(_faker);
        var token = _service.WriteRefreshToken(member);
        Assert.That(string.IsNullOrEmpty(token), Is.False);
        
        //validate
        var res = _service.TryValidateRefreshToken(token, out var id);
        Assert.That(res, Is.True);
        Assert.That(id, Is.Not.Null);
        Assert.That(id, Is.EqualTo(member.Id));
    }
}