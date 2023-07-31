using Bogus;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Repository;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Utills;
using Dapper;
using MySql.Data.MySqlClient;

namespace cloudsharpback.Test;

public class MemberRepositoryTests
{
    protected record Member(ulong MemberId, string Id, string Password, string Nick, ulong Role, string Email,
        string Dir);

    private MemberRepository _repository;
    private List<Member> _members = new List<Member>();
    private Faker _faker = new Faker();

    [SetUp]
    public async Task Setup()
    {
        _repository = new MemberRepository(DBConnectionFactoryMock.Mock.Object);
        _members = new List<Member>();
        var insertSql = @"
INSERT INTO member
VALUES (@memberId, @id, @password, @nick, @role, @email, UUID_TO_BIN(@dir), null);
";
        ulong memberid = 1;
        var faker = _faker = new Faker();
        for (int i = 0; i < 5; i++)
        {
            var mem = new Member(
                memberid++,
                faker.Internet.UserName(),
                faker.Internet.Password(),
                faker.Internet.UserName(),
                2,
                faker.Internet.Email(),
                Guid.NewGuid().ToString()
            );
            _members.Add(mem);
            await Task.Delay(1);
        }

        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        await conn.ExecuteAsync("DELETE FROM member");
        foreach (var mem in _members)
        {
            await conn.ExecuteAsync(insertSql, new
            {
                memberId = mem.MemberId,
                id = mem.Id,
                password = PasswordEncrypt.EncryptPassword(mem.Password),
                nick = mem.Nick,
                email = mem.Email,
                dir = mem.Dir,
                role = mem.Role
            });
        }
    }

    [Test]
    public async Task GetMemberById()
    {
        var sucs = _members;
        var fails = (ulong)Random.Shared.Next(_members.Count+1, int.MaxValue);
        foreach (var suc in sucs)
        {
            var mem = await _repository.GetMemberById(suc.MemberId);
            Assert.That(mem, Is.Not.Null);
            Assert.That(mem!.Directory, Is.EqualTo(suc.Dir));
        }

        Assert.That(await _repository.GetMemberById(fails), Is.Null);
    }

    [Test]
    public async Task GetMemberByLoginId()
    {
        var sucs = _members;
        var fails = "fails";
        foreach (var suc in sucs)
        {
            var mem = await _repository.GetMemberByLoginId(suc.Id);
            Assert.That(mem, Is.Not.Null);
            Assert.That(mem!.Directory, Is.EqualTo(suc.Dir));
        }

        Assert.That(await _repository.GetMemberByLoginId(fails), Is.Null);
    }

    [Test]
    public async Task TryUpdateMemberProfileImage()
    {
        var sucs = _members;
        foreach (var suc in sucs)
        {
            var filename = _faker.System.FileName();
            var res = await _repository.TryUpdateMemberProfileImage(suc.MemberId, filename);
            Assert.That(res, Is.True);
            Assert.That((await _repository.GetMemberById(suc.MemberId))!.ProfileImage, Is.EqualTo(filename));
        }
    }
    
    [Test]
    public async Task TryUpdateMemberNickname()
    {
        var sucs = _members;
        foreach (var suc in sucs)
        {
            var nickename = _faker.Internet.UserName();
            var res = await _repository.TryUpdateMemberNickname(suc.MemberId, nickename);
            Assert.That(res, Is.True);
            Assert.That((await _repository.GetMemberById(suc.MemberId))!.Nickname, Is.EqualTo(nickename));
        }
    }
    
    [Test]
    public async Task TryUpdateMemberEmail()
    {
        var sucs = _members;
        foreach (var suc in sucs)
        {
            var email = _faker.Internet.Email();
            var res = await _repository.TryUpdateMemberEmail(suc.MemberId, email);
            Assert.That(res, Is.True);
            Assert.That((await _repository.GetMemberById(suc.MemberId))!.Email, Is.EqualTo(email));
        }
    }
    
    [Test]
    public async Task GetMemberPasswordHashById()
    {
        var sucs = _members;
        var fails = (ulong)Random.Shared.Next(_members.Count+1, int.MaxValue);
        foreach (var suc in sucs)
        {
            var hash = await _repository.GetMemberPasswordHashById(suc.MemberId);
            Assert.That(hash, Is.Not.Null);
            Assert.That(PasswordEncrypt.VerifyPassword(suc.Password, hash!), Is.True);
        }

        Assert.That(await _repository.GetMemberPasswordHashById(fails), Is.Null);
    }
    
    [Test]
    public async Task GetMemberPasswordHashByLoginId()
    {
        var sucs = _members;
        var fails = "fails";
        foreach (var suc in sucs)
        {
            var hash = await _repository.GetMemberPasswordHashByLoginId(suc.Id);
            Assert.That(hash, Is.Not.Null);
            Assert.That(PasswordEncrypt.VerifyPassword(suc.Password, hash!), Is.True);
        }

        Assert.That(await _repository.GetMemberPasswordHashByLoginId(fails), Is.Null);
    }
    
    [Test]
    public async Task TryUpdateMemberPassword()
    {
        var sucs = _members;
        foreach (var suc in sucs)
        {
            var password = _faker.Internet.Password();
            var res = await _repository.TryUpdateMemberPassword(suc.MemberId, PasswordEncrypt.EncryptPassword(password));
            Assert.That(res, Is.True);
            Assert.That(PasswordEncrypt.VerifyPassword(password, (await _repository.GetMemberPasswordHashById(suc.MemberId))!) , Is.True);
        }
    }

    [Test]
    public async Task TryAddMember()
    {
        var sucs = new List<RegisterDto>();
        for (int i = 0; i < 5; i++)
        {
            var dto = new RegisterDto()
            {
                Email = _faker.Internet.Email(),
                Id = _faker.Internet.UserName(),
                Nick = _faker.Internet.UserName(),
                Pw = PasswordEncrypt.EncryptPassword(_faker.Internet.Password())
            };
            sucs.Add(dto);
            var res = await _repository.TryAddMember(dto, 2);
            Assert.That(res, Is.True);
        }

        foreach (var suc in sucs)
        {
            var res = await _repository.GetMemberByLoginId(suc.Id);
            Assert.That(res, Is.Not.Null);
            Assert.That(res!.Email, Is.EqualTo(suc.Email));
        }
        
        //fail
        
        var fails = new List<RegisterDto>();
        for (int i = 0; i < 5; i++)
        {
            var dto = new RegisterDto()
            {
                Email = _faker.Internet.Email(),
                Id = _faker.Internet.UserName(),
                Nick = _faker.Internet.UserName(),
                Pw = PasswordEncrypt.EncryptPassword(_faker.Internet.Password())
            };
            fails.Add(dto);
            var res = await _repository.TryAddMember(dto, (ulong)Random.Shared.Next(3, 100));
            Assert.That(res, Is.False);
        }

        foreach (var suc in sucs)
        {
            var dto = new RegisterDto()
            {
                Email = _faker.Internet.Email(),
                Id = suc.Id,
                Nick = _faker.Internet.UserName(),
                Pw = PasswordEncrypt.EncryptPassword(_faker.Internet.Password())
            };
            fails.Add(dto);
            var res = await _repository.TryAddMember(dto, (ulong)Random.Shared.Next(3, 100));
            Assert.That(res, Is.False);
        }
    }
}