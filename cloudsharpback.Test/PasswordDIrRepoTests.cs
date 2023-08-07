using Bogus;
using cloudsharpback.Repository;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Test.Records;
using Dapper;

namespace cloudsharpback.Test;

public class PasswordDIrRepoTests
{
    private List<Member> _members = new List<Member>();
    private List<PassDir> _passDirs = new List<PassDir>();
    private PasswordStoreDirectoryRepository _repository = null!;
    private Faker _faker = new Faker();
    private ulong FailMemberId =>
        (ulong)Random.Shared.Next(_members.Count +1, 100);
    
    [SetUp]
    public async Task SetUp()
    {
        _members = await MemberRepositoryTests.SetTable();
        _passDirs = await SetTable(5, _members);
        _repository = new PasswordStoreDirectoryRepository(DBConnectionFactoryMock.Mock.Object);
    }

    public static async Task<List<PassDir>> SetTable(int rowsSize, List<Member> members)
    {
        var list = new List<PassDir>();
        var faker = new Faker();
        await DeleteAllRows();
        for (int i = 0; i < rowsSize; i++)
        {
            var row = PassDir.GetFake(faker, (ulong)i+1, faker.Random.ULong(1, (ulong)members.Count));
            list.Add(row);
            await InsertRow(row);
        }
        return list;
    }

    private static async Task DeleteAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        await conn.ExecuteAsync("DELETE FROM password_store_directory;");
    }

    private static async Task InsertRow(PassDir passDir)
    {
        var sql = @"
INSERT INTO password_store_directory
VALUES (@password_directory_id, @name, @comment, @icon, @last_edited_time, @created_time, @member_id);
";
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        await conn.ExecuteAsync(sql, passDir);
    }
    
    private static async Task<List<PassDir>> GetAllRows() 
    {
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        var res = await conn.QueryAsync<PassDir>("SELECT * FROM password_store_directory;");
        return res.ToList();
    }

    [Test]
    public async Task GetDirListByMemberId()
    {
        foreach (var dir in _passDirs)
        {
            var names = _passDirs
                .Where(x => x.member_id == dir.member_id)
                .Select(x => x.name).ToList();

            var res = await _repository.GetDirListByMemberId(dir.member_id);
            var resNames = res.Select(x => x.Name).ToList();
            resNames.ForEach(x => Assert.That(names, Does.Contain(x)));
        }
    }

    [Test]
    public async Task GetDirById()
    {
        foreach (var passDir in _passDirs)
        {
            var res = await _repository.GetDirById(passDir.member_id, passDir.password_directory_id);
            Assert.That(res, Is.Not.Null);
            Assert.That(res!.Name, Is.EqualTo(passDir.name));
        }

        for (int i = 0; i < 5; i++)
        {
            var res = await _repository.GetDirById(FailMemberId, _faker.Random.ULong());
            Assert.That(res, Is.Null);
        }
    }
}