using Bogus;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using cloudsharpback.Utills;
using Dapper;

namespace cloudsharpback.Test;

public class ShareRepositoryTests
{
    private List<Member> Members = new();
    private List<Share> Shares = new();
    private ShareRepository _shareRepository;
    [SetUp]
    public async Task Setup()
    {
        _shareRepository = new ShareRepository(DBConnectionFactoryMock.Mock.Object);
        Members = await MemberRepositoryTests.SetTable();
        Shares = await SetTable(5, Members);
    }

    public static async Task<List<Share>> SetTable(int rowsCount, List<Member> members)
    {
        var faker = new Faker();
        var list = new List<Share>();
        var insertSQL = @"
INSERT INTO share
VALUES (@id, @memberId, @target, @password, @expireTime, @comment, @shareTime, @shareName, UUID_TO_BIN(@token), @fileSize)
";
        for (int i = 0; i < rowsCount; i++)
        {
            var item = Share.GetFake(faker, (ulong)i + 1, members.ElementAt(Random.Shared.Next(members.Count - 1)).MemberId);
            list.Add(item);
        }

        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        await conn.ExecuteAsync("DELETE FROM share");
        foreach (var item in list)
        {
            await conn.ExecuteAsync(insertSQL, new
            {
                id = item.Id,
                memberId = item.MemeberId,
                target = item.Target,
                password = PasswordEncrypt.EncryptPassword(item.Password),
                expireTime = item.ExpireTime,
                comment = item.Comment,
                shareTime = item.ShareTime,
                shareName = item.ShareName,
                token = item.Token,
                fileSize = item.FileSize,
            });
        }
        return list;
    }

    [Test]
    public async Task GetShareByToken()
    {
        //success
        foreach (var item in Shares)
        {
            var dto = await _shareRepository.GetShareByToken(item.Token);
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.Target, Is.EqualTo(item.Target));
        }
        
        //fail
        for (int i = 0; i < Shares.Count; i++)
        {
            var dto = await _shareRepository.GetShareByToken(Guid.NewGuid());
            Assert.That(dto, Is.Null);
        }
    }
}