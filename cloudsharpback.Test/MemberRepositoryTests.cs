using Bogus;
using cloudsharpback.Repository;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Utills;
using Dapper;

namespace cloudsharpback.Test;

public class MemberRepositoryTests
{
    protected record Member(ulong MemberId, string Id, string Password, string Nick, ulong Role, string Email,
        string Dir);

    private MemberRepository _repository;
    private List<Member> _members = new List<Member>();
    [SetUp]
    public async Task Setup()
    {
        _repository = new MemberRepository(DBConnectionFactoryMock.Mock.Object);
        _members = new List<Member>();
        var insertSql = @"
INSERT INTO member
VALUES (@memberId, @id, @password, @nick, @role, @email, @dir, null);
";
        ulong memberid = 1;
        var faker = new Faker();
        for (int i = 0; i < 2; i++)
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
                dir = mem.Dir
            });  
        }
    }
    
    
    
}