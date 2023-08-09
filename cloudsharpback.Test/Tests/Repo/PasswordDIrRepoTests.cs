using Bogus;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using Dapper;

namespace cloudsharpback.Test.Tests.Repo;

public class PasswordDIrRepoTests
{
    private List<Member> _members = null!;
    private List<PassDir> _passDirs = null!;
    private PasswordStoreDirectoryRepository _repository = null!;
    private Faker _faker = null!;
    private ulong FailMemberId => Utils.GetFailId(_members);
    private ulong FailRowId => Utils.GetFailId(_passDirs);

    [SetUp]
    public async Task SetUp()
    {
        _members = await MemberRepositoryTests.SetTable();
        _passDirs = await SetTable(_members, 5);
        _repository = new PasswordStoreDirectoryRepository(DBConnectionFactoryMock.Mock.Object);
        _faker = new Faker();
    }

    public static async Task<List<PassDir>> SetTable(List<Member> members, int rowsSize = 5)
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

    [Test]
    public async Task TryInsertDir()
    {
        var insertCount = 5;
        var insertlist = new List<PassDir>();
        for (int i = 0; i < insertCount; i++)
        {
            var data = PassDir.GetFake(_faker, 0, _faker.Random.ULong(1, (ulong)_members.Count));
            var res = await _repository.TryInsertDir(data.member_id, data.name, data.comment, data.icon);
            Assert.That(res, Is.True);
            var rows = await GetAllRows();
            Assert.That(rows.Select(x => x.ToCompareTestString()).ToList(), Does.Contain(data.ToCompareTestString()));
        }

        //fail
        for (int i = 0; i < insertCount; i++)
        {
            var data = PassDir.GetFake(_faker, 0, FailMemberId);
            var res = await _repository.TryInsertDir(data.member_id, data.name, data.comment, data.icon);
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task DeleteDir()
    {
        foreach (var passDir in _passDirs)
        {
            var res = await _repository.DeleteDir(passDir.member_id, passDir.password_directory_id);
            Assert.That(res, Is.True);
            var rows = await GetAllRows();
            Assert.That(rows.Select(Utils.ToJson).ToList(), Does.Not.Contain(Utils.ToJson(passDir)));
        }
    }

    [Test]
    public async Task DeleteDirFail()
    {
        foreach (var passDir in _passDirs)
        {
            var res = await _repository.DeleteDir(FailMemberId, passDir.password_directory_id);
            Assert.That(res, Is.False);
            res = await _repository.DeleteDir(FailMemberId, FailRowId);
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task UpdateDir()
    {
        foreach (var update in _passDirs.Select(passDir => PassDir.GetFake(_faker, passDir.password_directory_id, passDir.member_id)))
        {
            var res = await _repository.UpdateDir(update.member_id, update.password_directory_id, update.name,
                update.comment, update.icon);
            Assert.That(res, Is.True);
            var rows = await GetAllRows();
            Assert.That(rows.Select(x => x.ToCompareTestString()).ToList(), Does.Contain(update.ToCompareTestString()));
        }

        //fail
        foreach (var update in _passDirs.Select(passDir => PassDir.GetFake(_faker, FailRowId, passDir.member_id)))
        {
            var res = await _repository.UpdateDir(update.member_id, update.password_directory_id, update.name,
                update.comment, update.icon);
            Assert.That(res, Is.False);
        }
        
        foreach (var update in _passDirs.Select(passDir => PassDir.GetFake(_faker, FailRowId, FailMemberId)))
        {
            var res = await _repository.UpdateDir(update.member_id, update.password_directory_id, update.name,
                update.comment, update.icon);
            Assert.That(res, Is.False);
        }
    }
}