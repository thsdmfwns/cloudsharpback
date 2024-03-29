using Bogus;
using cloudsharpback.Models.DTO.PasswordStore;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using Dapper;
using Google.Protobuf.WellKnownTypes;
using K4os.Hash.xxHash;

namespace cloudsharpback.Test.Tests.Repo;

public class PassValueRepoTests : TestsBase
{
    private List<Member> _members = null!;
    private List<PassKey> _PassKeys = null!;
    private List<PassDir> _PassDirs = null!;
    private List<PassValue> _PassValues = null!;
    private PasswordStoreValueRepository _repository = null!;
    private Faker _faker = null!;
    
    private ulong FailPassKeyId => Utils.GetFailId(_PassKeys);
    private ulong FailPassDIrId => Utils.GetFailId(_PassDirs);
    private ulong FailPassValId => Utils.GetFailId(_PassValues);

    private PassValue RandomPassValue => Utils.GetRandomItem(_PassValues);
    [SetUp]
    public async Task SetUp()
    {
        _members = await MemberRepositoryTests.SetTable();
        _PassKeys = await PassKeyRepoTests.SetTable(_members);
        _PassDirs = await PasswordDIrRepoTests.SetTable(_members);
        _faker = new Faker();
        _PassValues = await SetTable(_PassKeys, _PassDirs);
        _repository = new PasswordStoreValueRepository(DBConnectionFactoryMock.Mock);
    }

    public static async Task<List<PassValue>> SetTable(List<PassKey> keys, List<PassDir> PassDirs, int fakeCount = 5)
    {
        var list = new List<PassValue>();
        var faker = new Faker();
        await DeleteAllRows();
        for (int i = 0; i < fakeCount; i++)
        {
            var fake = PassValue.GetFake(faker,
                faker.Random.ULong(1, (ulong)PassDirs.Count),
                faker.Random.ULong(1, (ulong)keys.Count),
                (ulong)i+1);
            await InsertRow(fake);
            list.Add(fake);
        }
        return list;
    }

    private static async Task InsertRow(PassValue passValue)
    {
        var sql = @"
INSERT INTO password_store_value
VALUES (
        @password_store_value_id,
        @directory_id,
        @encrypt_key_id,
        @value_id,
        @value_password,
        @created_time,
        @last_edited_time
);";

        using var conn = DBConnectionFactoryMock.Mock.MySqlConnection;
        await conn.ExecuteAsync(sql, passValue);
    }

    private static async Task DeleteAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.MySqlConnection;
        await conn.ExecuteAsync("DELETE FROM password_store_value");
    }

    private static async Task<List<PassValue>> GetAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.MySqlConnection;
        return (await conn.QueryAsync<PassValue>("SELECT * FROM password_store_value")).ToList();
    }

    [Test]
    public async Task GetValueById()
    {
        var value = RandomPassValue;
        var res = await _repository.GetValueById(value.password_store_value_id);
        Assert.That(res, Is.Not.Null);
        var resRecord = new PassValue(res!.Id, res.DirectoryId, res.KeyId, res.ValueId, res.ValuePassword,
            res.CreatedTime, res.LastEditedTime);
        Assert.That(resRecord.ToCompareTestString(), Is.EqualTo(value.ToCompareTestString()));

        //fail
        res = await _repository.GetValueById(FailPassValId);
        Assert.That(res, Is.Null);
    }

    [Test]
    public async Task GetValuesByDirectoryId()
    {
        foreach (var passDir in _PassDirs)
        {
            var res = 
                (await _repository.GetValuesByDirectoryId(passDir.password_directory_id))
                .OrderBy(x => x.Id)
                .ToList();
            var items = _PassValues
                .Where(x => x.directory_id == passDir.password_directory_id)
                .Select(x => new PasswordStoreValueListItemDto()
                {
                    CreatedTime = x.created_time,
                    DirectoryId = x.directory_id,
                    Id = x.password_store_value_id,
                    KeyId = x.encrypt_key_id,
                    LastEditedTime = x.last_edited_time
                })
                .OrderBy(x => x.Id)
                .ToList();
            Assert.That(Utils.ToJson(res), Is.EqualTo(Utils.ToJson(items)));
        }

        //fail
        var result = await _repository.GetValuesByDirectoryId(FailPassDIrId);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetValuesByKeyId()
    {
        foreach (var passKey in _PassKeys)
        {
            var res =
                (await _repository.GetValuesByKeyId(passKey.password_store_key_id))
                .OrderBy(x => x.Id)
                .ToList();
            var items = _PassValues
                .Where(x => x.encrypt_key_id == passKey.password_store_key_id)
                .Select(x => new PasswordStoreValueListItemDto()
                {
                    CreatedTime = x.created_time,
                    DirectoryId = x.directory_id,
                    Id = x.password_store_value_id,
                    KeyId = x.encrypt_key_id,
                    LastEditedTime = x.last_edited_time
                })
                .OrderBy(x => x.Id)
                .ToList();
            Assert.That(Utils.ToJson(res), Is.EqualTo(Utils.ToJson(items)));
        }

        for (int i = 0; i < _PassKeys.Count; i++)
        {
            var res = await _repository.GetValuesByKeyId(FailPassKeyId);
            Assert.That(res, Is.Empty);
        }   
    }

    [Test]
    public async Task GetValuesByKeyIdAndDirId()
    {
        var value = RandomPassValue;
        var res = 
            (await _repository.GetValuesByKeyIdAndDirId(value.directory_id, value.encrypt_key_id))
            .OrderBy(x => x.Id)
            .ToList();
        var items = _PassValues
            .Where(x => x.directory_id == value.directory_id &&
                        x.encrypt_key_id == value.encrypt_key_id)
            .Select(x => new PasswordStoreValueListItemDto()
            {
                CreatedTime = x.created_time,
                DirectoryId = x.directory_id,
                Id = x.password_store_value_id,
                KeyId = x.encrypt_key_id,
                LastEditedTime = x.last_edited_time
            })
            .OrderBy(x => x.Id)
            .ToList();
        Assert.That(Utils.ToJson(res), Is.EqualTo(Utils.ToJson(items)));


        //fail
        res = await _repository.GetValuesByKeyIdAndDirId(value.directory_id, FailPassKeyId);
        Assert.That(res, Is.Empty);
        res = await _repository.GetValuesByKeyIdAndDirId(FailPassDIrId, value.encrypt_key_id);
        Assert.That(res, Is.Empty);
        res = await _repository.GetValuesByKeyIdAndDirId(FailPassDIrId, FailPassKeyId);
        Assert.That(res, Is.Empty);

    }

    [Test]
    public async Task InsertValue()
    {
        var val = PassValue.GetFake(_faker,
            _faker.Random.ULong(1, (ulong)_PassDirs.Count),
            _faker.Random.ULong(1, (ulong)_PassKeys.Count),
            0
        );
        var res = await _repository.TryInsertValue(val.directory_id, val.encrypt_key_id, val.value_id,
            val.value_password);
        Assert.That(res, Is.True);

        var rows = (await GetAllRows())
            .Select(x => x.ToCompareTestString())
            .ToList();

        Assert.That(rows, Does.Contain(val.ToCompareTestString()));

        //fail
        val = PassValue.GetFake(_faker,
            FailPassDIrId,
            _faker.Random.ULong(1, (ulong)_PassKeys.Count),
            0
        );
        res = await _repository.TryInsertValue(val.directory_id, val.encrypt_key_id, val.value_id, val.value_password);
        Assert.That(res, Is.False);

        val = PassValue.GetFake(_faker,
            FailPassDIrId,
            FailPassKeyId,
            0
        );
        res = await _repository.TryInsertValue(val.directory_id, val.encrypt_key_id, val.value_id, val.value_password);
        Assert.That(res, Is.False);

        val = PassValue.GetFake(_faker,
            _faker.Random.ULong(1, (ulong)_PassDirs.Count),
            FailPassKeyId,
            0
        );
        res = await _repository.TryInsertValue(val.directory_id, val.encrypt_key_id, val.value_id, val.value_password);
        Assert.That(res, Is.False);

    }

    [Test]
    public async Task UpdateValue()
    {
        var value = RandomPassValue;
        var update = PassValue.GetFake(
            _faker,
            value.directory_id,
            value.encrypt_key_id,
            value.password_store_value_id);
        var res = await _repository.UpdateValue(update.password_store_value_id, update.value_id, update.value_password);
        Assert.That(res, Is.True);
        var target = (await GetAllRows()).Single(x => x.password_store_value_id == update.password_store_value_id);
        Assert.That(target.ToCompareTestString(), Is.EqualTo(update.ToCompareTestString()));

        //fail
        res = await _repository.UpdateValue(FailPassValId, update.value_id, update.value_password);
        Assert.That(res, Is.False);

    }

    [Test]
    public async Task DeleteValue()
    {
        var value = RandomPassValue;
        var res = await _repository.DeleteValue(value.password_store_value_id);
        Assert.That(res, Is.True);
        var target =
            (await GetAllRows()).SingleOrDefault(x => x.password_store_value_id == value.password_store_value_id);
        Assert.That(target, Is.Null);
    }

    [Test]
    public async Task DeleteValue_Fail()
    {
        var res = await _repository.DeleteValue(FailPassValId);
        Assert.That(res, Is.False);
    }

}