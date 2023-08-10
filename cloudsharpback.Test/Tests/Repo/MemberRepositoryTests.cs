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
            password = PasswordEncrypt.EncryptPassword(mem.Password),
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
SELECT member_id MemberId, id AS Id, password AS Password, nickname Nick, email As Email, BIN_TO_UUID(directory) Dir, role_id Role, profile_image ProfileImage 
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
        var sucs = _members;
        foreach (var suc in sucs)
        {
            var mem = await _repository.GetMemberById(suc.MemberId);
            Assert.That(mem, Is.Not.Null);
            Assert.That(mem!.Directory, Is.EqualTo(suc.Dir.ToString()));
        }
        Assert.That(await _repository.GetMemberById(FailMemberId), Is.Null);
    }

    [Test]
    public async Task GetMemberByLoginId()
    {
        var sucs = _members;
        foreach (var suc in sucs)
        {
            var mem = await _repository.GetMemberByLoginId(suc.Id);
            Assert.That(mem, Is.Not.Null);
            Assert.That(mem!.Directory, Is.EqualTo(suc.Dir.ToString()));
        }
        Assert.That(await _repository.GetMemberByLoginId(_faker.Random.String()), Is.Null);
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
            var rows = await GetAllRows();
            Assert.That(rows.Select(x => x.ProfileImage).ToList(), Does.Contain(filename));
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
            var rows = await GetAllRows();
            Assert.That(rows.Select(x => x.Nick).ToList(), Does.Contain(nickename));
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
            var rows = await GetAllRows();
            Assert.That(rows.Select(x => x.Email).ToList(), Does.Contain(email));
        }
    }
    
    [Test]
    public async Task GetMemberPasswordHashById()
    {
        var sucs = _members;
        foreach (var suc in sucs)
        {
            var hash = await _repository.GetMemberPasswordHashById(suc.MemberId);
            Assert.That(hash, Is.Not.Null);
            Assert.That(PasswordEncrypt.VerifyPassword(suc.Password, hash!), Is.True);
        }

        Assert.That(await _repository.GetMemberPasswordHashById(FailMemberId), Is.Null);
    }
    
    [Test]
    public async Task GetMemberPasswordHashByLoginId()
    {
        var sucs = _members;
        foreach (var suc in sucs)
        {
            var hash = await _repository.GetMemberPasswordHashByLoginId(suc.Id);
            Assert.That(hash, Is.Not.Null);
            Assert.That(PasswordEncrypt.VerifyPassword(suc.Password, hash!), Is.True);
        }

        Assert.That(await _repository.GetMemberPasswordHashByLoginId(_faker.Random.String()), Is.Null);
    }
    
    [Test]
    public async Task TryUpdateMemberPassword()
    {
        var sucs = _members;
        foreach (var suc in sucs)
        {
            var password = PasswordEncrypt.EncryptPassword(_faker.Internet.Password());
            var res = await _repository.TryUpdateMemberPassword(suc.MemberId, password);
            Assert.That(res, Is.True);
            var rows = await GetAllRows();
            Assert.That(rows.Select(x => x.Password).ToList(), Does.Contain(password));
        }
    }

    [Test]
    public async Task TryAddMember()
    {
        var addMembersCount = 5;
        var sucs = new List<Member>();
        for (int i = 0; i < addMembersCount; i++)
        {
            var mem = Member.GetFake(0, _faker);
            sucs.Add(mem);
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

        foreach (var suc in sucs)
        {
            var dto = new RegisterDto()
            {
                Email = _faker.Internet.Email(),
                Id = suc.Id,
                Nick = _faker.Internet.UserName(),
                Pw = PasswordEncrypt.EncryptPassword(_faker.Internet.Password())
            };
            var res = await _repository.TryAddMember(dto, 2);
            Assert.That(res, Is.False);
        }
    }
}