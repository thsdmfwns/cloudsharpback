using Bogus;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using cloudsharpback.Utills;
using Dapper;

namespace cloudsharpback.Test.Tests.Repo;

public class ShareRepositoryTests
{
    private List<Member> _members = null!;
    private List<Share> _shares = null!;
    private Faker _faker = null!;
    private ShareRepository _shareRepository = null!;
    private ulong FailMemberId => Utils.GetFailId(_members);
    private ulong FailShareId => Utils.GetFailId(_shares);
    [SetUp]
    public async Task Setup()
    {
        _shareRepository = new ShareRepository(DBConnectionFactoryMock.Mock.Object);
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
            var item = Share.GetFake(faker, (ulong)i + 1, members.ElementAt(Random.Shared.Next(0, members.Count-1)).MemberId);
            list.Add(item);
            await InsertRow(item);
        }
        return list;
    }

    private static async Task DeleteAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        await conn.ExecuteAsync("DELETE FROM share");
    }
    
    private static async Task InsertRow(Share item)
    {
        var insertSQL = @"
INSERT INTO share
VALUES (@id, @memberId, @target, @password, @expireTime, @comment, @shareTime, @shareName, UUID_TO_BIN(@token), @fileSize)
";
        using var conn = DBConnectionFactoryMock.Mock.Object.Connection;
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
        var conn = DBConnectionFactoryMock.Mock.Object.Connection;
        return (await conn.QueryAsync<Share>(sql)).ToList();
    }


    [Test]
    public async Task GetShareByToken()
    {
        //success
        foreach (var item in _shares)
        {
            var dto = await _shareRepository.GetShareByToken(Guid.Parse(item.Token));
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.Target, Is.EqualTo(item.Target));
        }
        
        //fail
        for (int i = 0; i < _shares.Count; i++)
        {
            var dto = await _shareRepository.GetShareByToken(Guid.NewGuid());
            Assert.That(dto, Is.Null);
        }
    }

    [Test]
    public async Task GetSharesListByMemberId()
    {
        //Success
        foreach (var item in _members)
        {
            var dtos = 
                (await _shareRepository.GetSharesListByMemberId(item.MemberId))
                .Select(x => x.Target)
                .ToList();
            var shares = 
                _shares.Where(x => x.MemberId == item.MemberId)
                    .Select(x=> x.Target)
                    .ToList();
            dtos.ForEach(x => Assert.That(shares, Does.Contain(x)));
        }

        //fail
        for (int i = 0; i < _members.Count; i++)
        {
            var failList =
                await _shareRepository.GetSharesListByMemberId(FailMemberId);
            Assert.That(failList, Is.Empty);
        }
    }

    [Test]
    public async Task GetShareDownloadDtoByToken()
    {
        //suc
        foreach (var item in _shares)
        {
            var dto = await _shareRepository.GetShareDownloadDtoByToken(Guid.Parse(item.Token));
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.Target, Is.EqualTo(item.Target));
        }
        
        //fail
        for (int i = 0; i < _shares.Count; i++)
        {
            var dto = await _shareRepository.GetShareDownloadDtoByToken(Guid.NewGuid());
            Assert.That(dto, Is.Null);   
        }
    }

    [Test]
    public async Task GetSharesByTargetFilePath()
    {
        //suc
        foreach (var item in _shares)
        {
            var dtos = await _shareRepository.GetSharesByTargetFilePath(item.MemberId, item.Target);
            Assert.That(dtos, Is.Not.Empty);
            dtos.ForEach(x => Assert.That(x.Target, Is.EqualTo(item.Target)));
        }
        
        //fail
        for (int i = 0; i < _shares.Count; i++)
        {
            var dto = await _shareRepository.GetSharesByTargetFilePath(FailMemberId, _faker.Random.Words());
            Assert.That(dto, Is.Empty);   
        }
    }

    [Test]
    public async Task GetSharesInDirectory()
    {
        foreach (var share in _shares)
        {
            var dir = Path.GetDirectoryName(share.Target);
            var res = await _shareRepository.GetSharesInDirectory(share.MemberId, dir!);
            var tokens = res.Select(x => x.Token).ToList();
            Assert.That(res, Is.Not.Empty);
            Assert.That(tokens, Does.Contain(share.Token));
        }

        //fail
        for (int i = 0; i < _shares.Count; i++)
        {
            var res = await _shareRepository.GetSharesInDirectory(FailMemberId, _faker.Random.Word());
            Assert.That(res, Is.Empty);
        }
    }

    [Test]
    public async Task GetPasswordHashByToken()
    {
        foreach (var share in _shares)
        {
            var res = await _shareRepository.GetPasswordHashByToken(Guid.Parse(share.Token));
            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.EqualTo(share.Password));
        }

        //fail
        for (int i = 0; i < _shares.Count(); i++)
        {
            var res = await _shareRepository.GetPasswordHashByToken(_faker.Random.Uuid());
            Assert.That(res, Is.Null);
        }
    }

    [Test]
    public async Task TrySetShareExpireTimeToZero()
    {
        foreach (var share in _shares)
        {
            var res = await _shareRepository.TrySetShareExpireTimeToZero(share.MemberId, Guid.Parse(share.Token));
            Assert.That(res, Is.True);
            var targetRow = (await GetAllRows()).Single(x=> x.Token == share.Token);
            Assert.That(targetRow.ExpireTime, Is.EqualTo(0));
        }

        //fail
        for (int i = 0; i < _shares.Count(); i++)
        {
            var res = await _shareRepository.TrySetShareExpireTimeToZero(FailMemberId, Guid.NewGuid());
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task TryAddShare()
    {
        int addCount = 5;
        for (int i = 0; i < addCount; i++)
        {
            var share = Share.GetFake(_faker, _faker.Random.ULong(), _faker.Random.ULong(1, (ulong)_members.Count));
            var res = await _shareRepository.TryAddShare(share.MemberId, share.Target,
                share.Password, share.ExpireTime, share.Comment, share.ShareName,
                Guid.Parse(share.Token), share.FileSize);
            Assert.That(res, Is.True);
            var target = (await GetAllRows()).Single(x => x.Token == share.Token);
            Assert.That(target.ToCompareTestString(), Is.EqualTo(share.ToCompareTestString()));
        }
        
        //fail
        for (int i = 0; i < addCount; i++)
        {
            var share = Share.GetFake(_faker, _faker.Random.ULong(), FailMemberId);
            var res = await _shareRepository.TryAddShare(share.MemberId, share.Target,
                share.Password, share.ExpireTime, share.Comment, share.ShareName,
                Guid.Parse(share.Token), share.FileSize);
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task TryUpdateShare()
    {
        foreach (var share in _shares)
        {
            var update = Share.GetFake(_faker, share.Id, share.MemberId);
            var res = await _shareRepository.TryUpdateShare(
                update.MemberId, Guid.Parse(share.Token), update.Password, update.Comment,
                update.ExpireTime, update.ShareName);
            Assert.That(res, Is.True);
        }
        
        //fail
        foreach (var update in _shares.Select(share => Share.GetFake(_faker, share.Id, FailMemberId)).ToList())
        {
            var res = await _shareRepository.TryUpdateShare(
                update.MemberId, Guid.NewGuid(), update.Password, update.Comment,
                update.ExpireTime, update.ShareName);
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task TryDeleteShare()
    {
        foreach (var share in _shares)
        {
            var res = await _shareRepository.TryDeleteShare(share.MemberId, share.Target);
            Assert.That(res, Is.True);
        }

        //fail
        for (int i = 0; i < _shares.Count; i++)
        {
            var res = await _shareRepository.TryDeleteShare(FailMemberId, _faker.System.FilePath());
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task TryDeleteShareInDirectory()
    {
        foreach (var share in _shares)
        {
            var dir = Path.GetDirectoryName(share.Target);
            var res = await _shareRepository.TryDeleteShareInDirectory(share.MemberId, dir!);
            var dircount = _shares.Count(x =>  x.Target.StartsWith(dir!)
                                             && share.MemberId == x.MemberId);
            Assert.That(res, Is.EqualTo(dircount));
        }
    }

    [Test]
    public async Task TryDeleteShareInDIrectoryFail()
    {
        for (int i = 0; i < _shares.Count(); i++)
        {
            var res = await _shareRepository.TryDeleteShareInDirectory(FailMemberId, _faker.Random.Words());
            Assert.That(res, Is.EqualTo(0));
        }
    }
    

}