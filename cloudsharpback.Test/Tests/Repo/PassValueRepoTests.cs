using Bogus;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using Dapper;

namespace cloudsharpback.Test.Tests.Repo;

public class PassValueRepoTests
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
    
    [SetUp]
    public async Task SetUp()
    {
        _members = await MemberRepositoryTests.SetTable();
        _PassKeys = await PassKeyRepoTests.SetTable(_members);
        _PassDirs = await PasswordDIrRepoTests.SetTable(_members);
        _faker = new Faker();
        _PassValues = await SetTable(_PassKeys, _PassDirs);
        _repository = new PasswordStoreValueRepository(DBConnectionFactoryMock.Mock.Object);
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

        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        await conn.ExecuteAsync(sql, passValue);
    }

    private static async Task DeleteAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        await conn.ExecuteAsync("DELETE FROM password_store_value");
    }

    private static async Task<List<PassValue>> GetAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        return (await conn.QueryAsync<PassValue>("SELECT * FROM password_store_value")).ToList();
    }
    
    
}