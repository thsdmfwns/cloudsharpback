using Bogus;
using cloudsharpback.Models.DTO.PasswordStore;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using Dapper;

namespace cloudsharpback.Test.Tests.Repo;

public class PassKeyRepoTests
{
    private List<Member> _members = null!;
    private List<PassKey> _passKeys = null!;
    private PasswordStoreKeyRepository _repository = null!;
    private Faker _faker = null!;
    private ulong FailMemberId => Utils.GetFailId(_members);
    private ulong FailPassKeyId => Utils.GetFailId(_passKeys);

    [SetUp]
    public async Task Setup()
    {
        _faker = new Faker();
        _repository = new PasswordStoreKeyRepository(DBConnectionFactoryMock.Mock.Object);
        _members = await MemberRepositoryTests.SetTable();
        _passKeys = await SetTable(_members);
    }

    public static async Task<List<PassKey>> SetTable(List<Member> members, int maxCount = 5)
    {
        await DeleteAllRows();
        var faker = new Faker();
        var list = new List<PassKey>();
        await DeleteAllRows();
        for (int i = 0; i < maxCount; i++)
        {
            var row = PassKey.GetFake(faker, (ulong)i + 1, faker.Random.ULong(1, (ulong)members.Count));
            list.Add(row);
            await InsertRow(row);
        }

        return list;
    }

    private static async Task InsertRow(PassKey passKey)
    {
        var sql = @"
INSERT INTO password_store_keys
VALUES (@password_store_key_id, @owner_id, @public_key, @private_key, @encrypt_algorithm, @created_time, @name, @comment)
";
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        await conn.ExecuteAsync(sql, passKey);
    }

    private static async Task DeleteAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        await conn.ExecuteAsync("DELETE FROM password_store_keys");
    }

    private static async Task<List<PassKey>> GetAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        var res = await conn.QueryAsync<PassKey>("SELECT * FROM password_store_keys");
        return res.ToList();
    }

    [Test]
    public async Task GetKeyById()
    {
        foreach (var passKey in _passKeys)
        {
            var res = await _repository.GetKeyById(passKey.owner_id, passKey.password_store_key_id);
            var data = new PasswordStoreKeyDto
            {
                Id = passKey.password_store_key_id,
                OwnerId = passKey.owner_id,
                PublicKey = passKey.public_key,
                PrivateKey = passKey.private_key,
                EncryptAlgorithmValue = (ulong)passKey.encrypt_algorithm,
                Name = passKey.name,
                Comment = passKey.comment,
                CreatedTime = passKey.created_time,
            };
            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.EqualTo(data));
            
            //fail
            res = await _repository.GetKeyById(FailMemberId, passKey.password_store_key_id);
            Assert.That(res, Is.Null);
            res = await _repository.GetKeyById(passKey.owner_id, FailPassKeyId);
            Assert.That(res, Is.Null);
        }
    }

    [Test]
    public async Task GetKeyListByMemberId()
    {
        foreach (var mem in _members)
        {
            var res = (await _repository.GetKeyListByMemberId(mem.MemberId))
                .Select(x => x.Id)
                .ToList();
            var keys = _passKeys
                .Where(x => x.owner_id == mem.MemberId)
                .Select(x => x.password_store_key_id)
                .ToList();
            keys.ForEach(x => Assert.That(res, Does.Contain(x)));
        }
        
        //fail
        for (int i = 0; i < _members.Count; i++)
        {
            var res = await _repository.GetKeyListByMemberId(FailMemberId);
            Assert.That(res, Is.Empty);
        }
    }

    [Test]
    public async Task TryInsertKey()
    {
        for (int i = 0; i < _passKeys.Count; i++)
        {
            var row = PassKey.GetFake(_faker, 0, _faker.Random.ULong(1, (ulong)_members.Count));
            var res = await _repository.TryInsertKey(row.owner_id, row.encrypt_algorithm, row.public_key, row.private_key,
                row.name, row.comment);
            Assert.That(res, Is.True);
            var rows = await GetAllRows();
            Assert.That(rows.Select(x => x.ToCompareTestString()).ToList(), Does.Contain(row.ToCompareTestString()));

            //fail
            row = PassKey.GetFake(_faker, 0, FailMemberId);
            res = await _repository.TryInsertKey(row.owner_id, row.encrypt_algorithm, row.public_key, row.private_key,
                row.name, row.comment);
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task DeleteKeyById()
    {
        foreach (var passKey in _passKeys)
        {
            var res = await _repository.DeleteKeyById(passKey.owner_id, passKey.password_store_key_id);
            Assert.That(res, Is.True);
            var rows = await GetAllRows();
            Assert.That(rows.Select(x => x.ToCompareTestString()).ToList(), Does.Not.Contain(passKey.ToCompareTestString()));
        }
    }

    [Test]
    public async Task DeleteKeyByIdFail()
    {
        foreach (var passKey in _passKeys)
        {
            var res = await _repository.DeleteKeyById(FailMemberId, passKey.password_store_key_id);
            Assert.That(res, Is.False);
            res = await _repository.DeleteKeyById(FailMemberId, FailPassKeyId);
            Assert.That(res, Is.False);
            res = await _repository.DeleteKeyById(passKey.owner_id, FailPassKeyId);
            Assert.That(res, Is.False);
        }
    }
}