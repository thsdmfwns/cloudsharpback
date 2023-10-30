using Bogus;
using cloudsharpback.Models.DTO.Share;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using Dapper;

namespace cloudsharpback.Test.Tests.Repo;

public class ShareRepositoryTests : TestsBase
{
    private List<Member> _members = null!;
    private List<Share> _shares = null!;
    private Faker _faker = null!;
    private ShareRepository _shareRepository = null!;
    private ulong FailMemberId => Utils.GetFailId(_members);
    private Share RandomShare => Utils.GetRandomItem(_shares);

    [SetUp]
    public async Task Setup()
    {
        _shareRepository = new ShareRepository(DBConnectionFactoryMock.Mock);
        _faker = new();
        _members = await MemberRepositoryTests.SetTable();
        _shares = await SetTable(5, _members);
    }

    public static async Task<List<Share>> SetTable(int rowsCount, List<Member> members)
    {
        var faker = new Faker();
        var list = new List<Share>();
        await DeleteAllRows();
        for (int i = 0; i < rowsCount; i++)
        {
            var item = Share.GetFake(faker, (ulong)i + 1,
                members.ElementAt(Random.Shared.Next(0, members.Count - 1)).MemberId);
            list.Add(item);
            await InsertRow(item);
        }

        return list;
    }

    private static async Task DeleteAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.MySqlConnection;
        await conn.ExecuteAsync("DELETE FROM share");
    }

    private static async Task InsertRow(Share item)
    {
        var insertSQL = @"
INSERT INTO share
VALUES (@id, @memberId, @target, @password, @expireTime, @comment, @shareTime, @shareName, UUID_TO_BIN(@token), @fileSize)
";
        using var conn = DBConnectionFactoryMock.Mock.MySqlConnection;
        await conn.ExecuteAsync(insertSQL, new
        {
            id = item.Id,
            memberId = item.MemberId,
            target = item.Target,
            password = item.Password,
            expireTime = item.ExpireTime,
            comment = item.Comment,
            shareTime = item.ShareTime,
            shareName = item.ShareName,
            token = item.Token,
            fileSize = item.FileSize,
        });
    }

    private static async Task<List<Share>> GetAllRows()
    {
        var sql = @"
SELECT share_id Id,
       member_id MemberId,
       target as Target,
       password as Password,
       expire_time ExpireTime,
       comment as Comment,
       share_time ShareTime,
       share_name ShareName,
       BIN_TO_UUID(token) as Token,
       file_size FileSize
FROM share; 
";
        var conn = DBConnectionFactoryMock.Mock.MySqlConnection;
        return (await conn.QueryAsync<Share>(sql)).ToList();
    }


    [Test]
    public async Task GetShareByToken()
    {
        //success
        var share = RandomShare;
        var res = await _shareRepository.GetShareByToken(Guid.Parse(share.Token));
        Assert.That(res, Is.Not.Null);
        var mem = _members.Single(x => x.MemberId == share.MemberId);
        var dto = new ShareResponseDto()
        {
            Comment = share.Comment,
            ExpireTime = share.ExpireTime,
            FileSize = share.FileSize,
            HasPassword = true,
            OwnerId = share.MemberId,
            OwnerNick = mem.Nick,
            ShareName = share.ShareName,
            ShareTime = share.ShareTime,
            Target = share.Target,
            Token = share.Token
        };
        Assert.That(Utils.ToJson(res!), Is.EqualTo(Utils.ToJson(dto)));

        //fail
        dto = await _shareRepository.GetShareByToken(Guid.NewGuid());
        Assert.That(dto, Is.Null);
    }

    [Test]
    public async Task GetSharesListByMemberId()
    {
        var share = RandomShare;
        //Success
        var res =
            (await _shareRepository.GetSharesListByMemberId(share.MemberId))
            .OrderBy(x => x.Token)
            .ToList();
        var member = _members.Single(x => x.MemberId == share.MemberId);
        var shares =
            _shares.Where(x => x.MemberId == share.MemberId)
                .Select(x => new ShareResponseDto()
                {
                    Comment = x.Comment,
                    ExpireTime = x.ExpireTime,
                    FileSize = x.FileSize,
                    HasPassword = true,
                    OwnerId = x.MemberId,
                    OwnerNick = member.Nick,
                    ShareName = x.ShareName,
                    ShareTime = x.ShareTime,
                    Target = x.Target,
                    Token = x.Token
                })
                .OrderBy(x => x.Token)
                .ToList();
        Assert.That(Utils.ToJson(res), Is.EqualTo(Utils.ToJson(shares)));

        //fail
        var failList =
            await _shareRepository.GetSharesListByMemberId(FailMemberId);
        Assert.That(failList, Is.Empty);
    }

    [Test]
    public async Task GetShareDownloadDtoByToken()
    {
        var share = RandomShare;
        //suc
        var res = await _shareRepository.GetShareDownloadDtoByToken(Guid.Parse(share.Token));
        Assert.That(res, Is.Not.Null);
        var mem = _members.Single(x => x.MemberId == share.MemberId);
        var dto = new ShareDownloadDto()
        {
            Directory = mem.Dir,
            ExpireTime = share.ExpireTime,
            Password = share.Password,
            Target = share.Target
        };
        Assert.That(Test.Utils.ClassToJson(res!), Is.EqualTo(Test.Utils.ClassToJson(dto)));

        //fail
        dto = await _shareRepository.GetShareDownloadDtoByToken(Guid.NewGuid());
        Assert.That(dto, Is.Null);
    }

    [Test]
    public async Task GetSharesByTargetFilePath()
    {
        var share = RandomShare;
        //suc
        var res = (await _shareRepository.GetSharesByTargetFilePath(share.MemberId, share.Target))
            .OrderBy(x => x.Token)
            .ToList();
        Assert.That(res, Is.Not.Empty);
        var mem = _members.Single(x => x.MemberId == share.MemberId);
        var shares =
            _shares.Where(x => x.MemberId == share.MemberId
                               && x.Target == share.Target)
                .Select(x => new ShareResponseDto()
                {
                    Comment = x.Comment,
                    ExpireTime = x.ExpireTime,
                    FileSize = x.FileSize,
                    HasPassword = true,
                    OwnerId = x.MemberId,
                    OwnerNick = mem.Nick,
                    ShareName = x.ShareName,
                    ShareTime = x.ShareTime,
                    Target = x.Target,
                    Token = x.Token
                })
                .OrderBy(x => x.Token)
                .ToList();
        Assert.That(Utils.ToJson(res), Is.EqualTo(Test.Utils.ToJson(shares)));

        //fail
        var dto = await _shareRepository.GetSharesByTargetFilePath(FailMemberId, _faker.Random.Words());
        Assert.That(dto, Is.Empty);
    }

    [Test]
    public async Task GetSharesInDirectory()
    {
        var share = RandomShare;
        var dir = Path.GetDirectoryName(share.Target);
        var res = await _shareRepository.GetSharesInDirectory(share.MemberId, dir!);
        Assert.That(res, Is.Not.Empty);
        var mem = _members.Single(x => x.MemberId == share.MemberId);
        var shares =
            _shares.Where(x => x.MemberId == share.MemberId
                               && x.Target.StartsWith(dir!))
                .Select(x => new ShareResponseDto()
                {
                    Comment = x.Comment,
                    ExpireTime = x.ExpireTime,
                    FileSize = x.FileSize,
                    HasPassword = true,
                    OwnerId = x.MemberId,
                    OwnerNick = mem.Nick,
                    ShareName = x.ShareName,
                    ShareTime = x.ShareTime,
                    Target = x.Target,
                    Token = x.Token
                })
                .OrderBy(x => x.Token)
                .ToList();
        Assert.That(Utils.ToJson(res), Is.EqualTo(Test.Utils.ToJson(shares)));

        //fail
        res = await _shareRepository.GetSharesInDirectory(FailMemberId, _faker.Random.Word());
        Assert.That(res, Is.Empty);
    }

    [Test]
    public async Task GetPasswordHashByToken()
    {
        var share = RandomShare;

        var res = await _shareRepository.GetPasswordHashByToken(Guid.Parse(share.Token));
        Assert.That(res, Is.Not.Null);
        Assert.That(res, Is.EqualTo(share.Password));

        //fail
        res = await _shareRepository.GetPasswordHashByToken(_faker.Random.Uuid());
        Assert.That(res, Is.Null);
    }

    [Test]
    public async Task TrySetShareExpireTimeToZero()
    {
        var share = RandomShare;
        var res = await _shareRepository.TrySetShareExpireTimeToZero(share.MemberId, Guid.Parse(share.Token));
        Assert.That(res, Is.True);
        var targetRow = (await GetAllRows()).Single(x => x.Token == share.Token);
        Assert.That(targetRow.ExpireTime, Is.EqualTo(0));

        //fail
        res = await _shareRepository.TrySetShareExpireTimeToZero(FailMemberId, Guid.NewGuid());
        Assert.That(res, Is.False);
    }

    [Test]
    public async Task TryAddShare()
    {
        var share = Share.GetFake(_faker, _faker.Random.ULong(), _faker.Random.ULong(1, (ulong)_members.Count));
        var res = await _shareRepository.TryAddShare(share.MemberId, share.Target,
            share.Password, share.ExpireTime, share.Comment, share.ShareName,
            Guid.Parse(share.Token), share.FileSize);
        Assert.That(res, Is.True);
        var target = (await GetAllRows()).Single(x => x.Token == share.Token);
        Assert.That(target.ToCompareTestString(), Is.EqualTo(share.ToCompareTestString()));

        //fail
        share = Share.GetFake(_faker, _faker.Random.ULong(), FailMemberId);
        res = await _shareRepository.TryAddShare(share.MemberId, share.Target,
            share.Password, share.ExpireTime, share.Comment, share.ShareName,
            Guid.Parse(share.Token), share.FileSize);
        Assert.That(res, Is.False);
    }

    [Test]
    public async Task TryUpdateShare()
    {
        var share = RandomShare;
        var update = Share.GetFake(_faker, share.Id, share.MemberId);
        var res = await _shareRepository.TryUpdateShare(
            share.MemberId, Guid.Parse(share.Token), update.Password, update.Comment,
            update.ExpireTime, update.ShareName);

        string GetCompareString(Share s)
        {
            return Utils.ClassToJson(new
            {
                s.Password,
                s.ExpireTime,
                s.Comment,
                s.ShareName,
            });
        }

        Assert.That(res, Is.True);
        var target = (await GetAllRows()).ToList().Single(x => x.Token == share.Token);
        Assert.That(GetCompareString(target), Is.EqualTo(GetCompareString(update)));
        //fail
        res = await _shareRepository.TryUpdateShare(
            update.MemberId, Guid.NewGuid(), update.Password, update.Comment,
            update.ExpireTime, update.ShareName);
        Assert.That(res, Is.False);
    }

    [Test]
    public async Task TryDeleteShare()
    {
        var share = RandomShare;
        var res = await _shareRepository.TryDeleteShare(share.MemberId, share.Target);
        Assert.That(res, Is.True);
        var target = (await GetAllRows()).FirstOrDefault(x => x.MemberId == share.MemberId && x.Target == share.Target);
        Assert.That(target, Is.Null);

        //fail
        res = await _shareRepository.TryDeleteShare(FailMemberId, _faker.System.FilePath());
        Assert.That(res, Is.False);
    }

    [Test]
    public async Task TryDeleteShareInDirectory()
    {
        var share = RandomShare;

        var dir = Path.GetDirectoryName(share.Target);
        var res = await _shareRepository.TryDeleteShareInDirectory(share.MemberId, dir!);
        var dircount = _shares.Count(x => x.Target.StartsWith(dir!)
                                          && share.MemberId == x.MemberId);
        Assert.That(res, Is.EqualTo(dircount));
        var targets = (await GetAllRows()).Where(x => x.Target.StartsWith(dir!)
                                                      && x.MemberId == share.MemberId);
        Assert.That(targets, Is.Empty);
    }

    [Test]
    public async Task TryDeleteShareInDIrectoryFail()
    {
        var res = await _shareRepository.TryDeleteShareInDirectory(FailMemberId, _faker.Random.Words());
        Assert.That(res, Is.EqualTo(0));
    }


}