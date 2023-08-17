using Bogus;
using cloudsharpback.Models.DTO.PasswordStore;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using Dapper;

namespace cloudsharpback.Test.Tests.Repo;

public class PasswordDIrRepoTests : TestsBase
{
    private List<Member> _members = null!;
    private List<PassDir> _passDirs = null!;
    private PasswordStoreDirectoryRepository _repository = null!;
    private Faker _faker = null!;
    private ulong FailMemberId => Utils.GetFailId(_members);
    private ulong FailRowId => Utils.GetFailId(_passDirs);
    private PassDir RandomPassDir => Utils.GetRandomItem(_passDirs);

    [SetUp]
    public async Task SetUp()
    {
        _members = await MemberRepositoryTests.SetTable();
        _passDirs = await SetTable(_members, 5);
        _repository = new PasswordStoreDirectoryRepository(DBConnectionFactoryMock.Mock);
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
        using var conn = DBConnectionFactoryMock.Mock.Connection;
        await conn.ExecuteAsync("DELETE FROM password_store_directory;");
    }

    private static async Task InsertRow(PassDir passDir)
    {
        var sql = @"
INSERT INTO password_store_directory
VALUES (@password_directory_id, @name, @comment, @icon, @last_edited_time, @created_time, @member_id);
";
        using var conn = DBConnectionFactoryMock.Mock.Connection;
        await conn.ExecuteAsync(sql, passDir);
    }
    
    private static async Task<List<PassDir>> GetAllRows() 
    {
        using var conn = DBConnectionFactoryMock.Mock.Connection;
        var res = await conn.QueryAsync<PassDir>("SELECT * FROM password_store_directory;");
        return res.ToList();
    }

    [Test]
    public async Task GetDirListByMemberId()
    {
        var dir = RandomPassDir;
        var datas = _passDirs
            .Where(x => x.member_id == dir.member_id)
            .Select(x => new PasswordStoreDirDto()
            {
                Comment = x.comment,
                CreatedTime = x.created_time,
                Id = x.password_directory_id,
                Icon = x.icon,
                LastEditTime = x.last_edited_time,
                Name = x.name,
                OwnerId = x.member_id
            })
            .OrderBy(x => x.Id)
            .ToList();
        var res = (await _repository.GetDirListByMemberId(dir.member_id))
            .OrderBy(x => x.Id)
            .ToList();
        Assert.That(Utils.ToJson(res), Is.EqualTo(Utils.ToJson(datas)));


        //fail
        res = await _repository.GetDirListByMemberId(FailMemberId);
        Assert.That(res, Is.Empty);

    }

    [Test]
    public async Task GetDirById()
    {
        var passDir = RandomPassDir;
        var res = await _repository.GetDirById(passDir.member_id, passDir.password_directory_id);
        Assert.That(res, Is.Not.Null);
        var dto = new PasswordStoreDirDto()
        {
            Comment = passDir.comment,
            CreatedTime = passDir.created_time,
            Id = passDir.password_directory_id,
            Icon = passDir.icon,
            LastEditTime = passDir.last_edited_time,
            Name = passDir.name,
            OwnerId = passDir.member_id
        };
        Assert.That(res, Is.EqualTo(dto));
        
        //fail
        res = await _repository.GetDirById(FailMemberId, _faker.Random.ULong());
        Assert.That(res, Is.Null);
    }

    [Test]
    public async Task TryInsertDir()
    {
        var data = PassDir.GetFake(_faker, 0, _faker.Random.ULong(1, (ulong)_members.Count));
        var res = await _repository.TryInsertDir(data.member_id, data.name, data.comment, data.icon);
        Assert.That(res, Is.True);
        var rows = await GetAllRows();
        Assert.That(rows.Select(x => x.ToCompareTestString()).ToList(), Does.Contain(data.ToCompareTestString()));

        //fail
        data = PassDir.GetFake(_faker, 0, FailMemberId);
        res = await _repository.TryInsertDir(data.member_id, data.name, data.comment, data.icon);
        Assert.That(res, Is.False);
    }

    [Test]
    public async Task DeleteDir()
    {
        var passDir = RandomPassDir;
        var res = await _repository.DeleteDir(passDir.member_id, passDir.password_directory_id);
        Assert.That(res, Is.True);
        var target =
            (await GetAllRows()).SingleOrDefault(x => x.password_directory_id == passDir.password_directory_id);
        Assert.That(target, Is.Null);
    }


    [Test]
    public async Task DeleteDir_Fail()
    {
        var passDir = RandomPassDir;
        var res = await _repository.DeleteDir(FailMemberId, passDir.password_directory_id);
        Assert.That(res, Is.False);
        res = await _repository.DeleteDir(FailMemberId, FailRowId);
        Assert.That(res, Is.False);
    }

    [Test]
    public async Task UpdateDir()
    {
        var passDir = RandomPassDir;
        var update = PassDir.GetFake(_faker, passDir.password_directory_id, passDir.member_id);
        var res = await _repository.UpdateDir(update.member_id, update.password_directory_id, update.name,
            update.comment, update.icon);
        Assert.That(res, Is.True);
        var target = (await GetAllRows()).Single(x => x.password_directory_id == update.password_directory_id);
        Assert.That(target.ToCompareTestString(), Is.EqualTo(update.ToCompareTestString()));


        //fail
        res = await _repository.UpdateDir(FailMemberId, update.password_directory_id, update.name,
            update.comment, update.icon);
        Assert.That(res, Is.False);

        res = await _repository.UpdateDir(update.member_id, FailRowId, update.name,
            update.comment, update.icon);
        Assert.That(res, Is.False);
        
        res = await _repository.UpdateDir(FailMemberId, FailRowId, update.name,
            update.comment, update.icon);
        Assert.That(res, Is.False);
    }
}