using Bogus;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using cloudsharpback.Utills;
using Dapper;

namespace cloudsharpback.Test.Tests.Repo;

public class MemberRepositoryTests
{

    private MemberRepository _repository = null!;
    private List<Member> _members = null!;
    private Faker _faker = null!;
    private ulong FailMemberId => Utils.GetFailId(_members);
    
    [SetUp]
    public async Task Setup()
    {
        _repository = new MemberRepository(DBConnectionFactoryMock.Mock);
        _members = await SetTable();
        _faker = new Faker();
    }

    [TearDown]
    public void TearDown()
    {
        var test = TestContext.CurrentContext.Test;
        var testResult = TestContext.CurrentContext.Result;
        
        Console.WriteLine($"|-------------------------------------------------------------------------------------------|");
        Console.WriteLine($"Test '{test.Name}' of '{test.ClassName}' finished with outcome: {testResult.Outcome}");
        Console.WriteLine($"PASS : {testResult.PassCount} | FAIL : {testResult.FailCount} | ALL : {testResult.InconclusiveCount}");
        Console.WriteLine($"|-------------------------------------------------------------------------------------------|");
    }

    private static async Task InsertMember(Member mem)
    {
        var insertSql = @"
INSERT INTO member
VALUES (@memberId, @id, @password, @nick, @role, @email, UUID_TO_BIN(@dir), @profileImage);
";
        using var conn = DBConnectionFactoryMock.Mock.Connection;
        await conn.ExecuteAsync(insertSql, new
        {
            memberId = mem.MemberId,
            id = mem.Id,
            password = mem.Password,
            nick = mem.Nick,
            email = mem.Email,
            dir = mem.Dir.ToString(),
            role = mem.Role,
            profileImage = mem.ProfileImage
        });
        
    }

    private static async Task<List<Member>> GetAllRows()
    {
        var sql = @"
SELECT member_id MemberId, 
       id AS Id,
       password AS Password,
       nickname Nick,
       email As Email,
       BIN_TO_UUID(directory) Dir,
       role_id Role,
       profile_image ProfileImage 
FROM member;
";
        var conn = DBConnectionFactoryMock.Mock.Connection;
        return (await conn.QueryAsync<Member>(sql)).ToList();
    }

    private static async Task DeleteMembers()
    {
        using var conn = DBConnectionFactoryMock.Mock.Connection;
        await conn.ExecuteAsync("DELETE FROM member");   
    }

    public static async Task<List<Member>> SetTable(int maxCount = 5)
    {
        ulong memberid = 1;
        var members = new List<Member>();
        var faker  = new Faker();
        await DeleteMembers();
        for (int i = 0; i < maxCount; i++)
        {
            var mem = Member.GetFake(memberid++, faker);
            members.Add(mem);
            await InsertMember(mem);
        }
        return members;
    }

    [Test]
    public async Task GetMemberById()
    {
        foreach (var member in _members)
        {
            var res = await _repository.GetMemberById(member.MemberId);
            Assert.That(res, Is.Not.Null);
            var memberDto = new MemberDto()
            {
                Directory = member.Dir,
                Email = member.Email,
                Id = member.MemberId,
                Nickname = member.Nick,
                ProfileImage = member.ProfileImage,
                Role = member.Role
            };
            Assert.That(Test.Utils.ClassToJson(res!), Is.EqualTo(Test.Utils.ClassToJson(memberDto)));
        }
        Assert.That(await _repository.GetMemberById(FailMemberId), Is.Null);
    }

    [Test]
    public async Task GetMemberByLoginId()
    {
        foreach (var member in _members)
        {
            var res = await _repository.GetMemberByLoginId(member.Id);
            Assert.That(res, Is.Not.Null);
            var memberDto = new MemberDto()
            {
                Directory = member.Dir,
                Email = member.Email,
                Id = member.MemberId,
                Nickname = member.Nick,
                ProfileImage = member.ProfileImage,
                Role = member.Role
            };
            Assert.That(Test.Utils.ClassToJson(res!), Is.EqualTo(Test.Utils.ClassToJson(memberDto)));
        }
        Assert.That(await _repository.GetMemberByLoginId(_faker.Random.String()), Is.Null);
    }

    [Test]
    public async Task TryUpdateMemberProfileImage()
    {
        
        foreach (var member in _members)
        {
            var filename = _faker.System.FileName();
            var res = await _repository.TryUpdateMemberProfileImage(member.MemberId, filename);
            Assert.That(res, Is.True);
            var target = (await GetAllRows()).Single(x => x.MemberId == member.MemberId);
            Assert.That(target.ProfileImage, Is.EqualTo(filename));
        }
        //fail
        for (int i = 0; i < _members.Count; i++)
        {
            var filename = _faker.System.FileName();
            var res = await _repository.TryUpdateMemberProfileImage(FailMemberId, filename);
            Assert.That(res, Is.False);
        }
    }
    
    [Test]
    public async Task TryUpdateMemberNickname()
    {
        foreach (var member in _members)
        {
            var nickename = _faker.Internet.UserName();
            var res = await _repository.TryUpdateMemberNickname(member.MemberId, nickename);
            Assert.That(res, Is.True);
            var target = (await GetAllRows()).Single(x => x.MemberId == member.MemberId);
            Assert.That(target.Nick, Is.EqualTo(nickename));
        }
        
        //fail
        for (int i = 0; i < _members.Count; i++)
        {
            var nickename = _faker.Internet.UserName();
            var res = await _repository.TryUpdateMemberNickname(FailMemberId, nickename);
            Assert.That(res, Is.False);
        }
    }
    
    [Test]
    public async Task TryUpdateMemberEmail()
    {
        foreach (var member in _members)
        {
            var email = _faker.Internet.Email();
            var res = await _repository.TryUpdateMemberEmail(member.MemberId, email);
            Assert.That(res, Is.True);
            var target = (await GetAllRows()).Single(x => x.MemberId == member.MemberId);
            Assert.That(target.Email, Is.EqualTo(email));
        }
        
        //fail
        for (int i = 0; i < _members.Count; i++)
        {
            var email = _faker.Internet.Email();
            var res = await _repository.TryUpdateMemberEmail(FailMemberId, email);
            Assert.That(res, Is.False);
        }
    }
    
    [Test]
    public async Task GetMemberPasswordHashById()
    {
        foreach (var member in _members)
        {
            var hash = await _repository.GetMemberPasswordHashById(member.MemberId);
            Assert.That(hash, Is.Not.Null);
            Assert.That(hash, Is.EqualTo(member.Password));
        }

        Assert.That(await _repository.GetMemberPasswordHashById(FailMemberId), Is.Null);
    }
    
    [Test]
    public async Task GetMemberPasswordHashByLoginId()
    {
        foreach (var member in _members)
        {
            var hash = await _repository.GetMemberPasswordHashByLoginId(member.Id);
            Assert.That(hash, Is.Not.Null);
            Assert.That(hash, Is.EqualTo(member.Password));
        }
        
        Assert.That(await _repository.GetMemberPasswordHashByLoginId(_faker.Random.String()), Is.Null);
    }
    
    [Test]
    public async Task TryUpdateMemberPassword()
    {
        foreach (var member in _members)
        {
            var password = PasswordEncrypt.EncryptPassword(_faker.Internet.Password());
            var res = await _repository.TryUpdateMemberPassword(member.MemberId, password);
            Assert.That(res, Is.True);
            var target = (await GetAllRows()).Single(x => x.MemberId == member.MemberId);
            Assert.That(target.Password, Is.EqualTo(password));
        }
    }

    [Test]
    public async Task TryAddMember()
    {
        var addMembersCount = 5;
        for (int i = 0; i < addMembersCount; i++)
        {
            var mem = Member.GetFake(0, _faker);
            var res = await _repository.TryAddMember(mem.Id, mem.Password, mem.Nick, mem.Email, Guid.Parse(mem.Dir),
                mem.Role, mem.ProfileImage);
            Assert.That(res, Is.True);
            var rows = await GetAllRows();
            Assert.That(rows.Select(x => x.ToCompareTestString()).ToList(), Does.Contain(mem.ToCompareTestString()));
        }

        //fail

        for (int i = 0; i < addMembersCount; i++)
        {
            var mem = Member.GetFake(0, _faker);
            var res = await _repository.TryAddMember(mem.Id, mem.Password, mem.Nick, mem.Email, Guid.Parse(mem.Dir),
                _faker.Random.ULong(3), mem.ProfileImage);
            Assert.That(res, Is.False);
        }

        foreach (var member in _members)
        {
            var res = await _repository.TryAddMember(member.Id, member.Password, member.Nick, member.Email, Guid.Parse(member.Dir),
                member.Role, member.ProfileImage);
            Assert.That(res, Is.False);
        }
    }
}