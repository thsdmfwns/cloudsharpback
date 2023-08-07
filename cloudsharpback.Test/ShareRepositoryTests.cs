using Bogus;
using cloudsharpback.Repository;
using cloudsharpback.Test.Records;
using cloudsharpback.Utills;
using Dapper;
using MySql.Data.MySqlClient;

namespace cloudsharpback.Test;

public class ShareRepositoryTests
{
    private List<Member> Members = new();
    private List<Share> Shares = new();
    private Faker _faker;
    private ShareRepository _shareRepository;
    private ulong FailMemberId =>
        (ulong)Random.Shared.Next(Members.Count +1, 100);
    [SetUp]
    public async Task Setup()
    {
        _shareRepository = new ShareRepository(DBConnectionFactoryMock.Mock.Object);
        _faker = new();
        Members = await MemberRepositoryTests.SetTable();
        Shares = await SetTable(5, Members);
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

    [Test]
    public async Task GetSharesListByMemberId()
    {
        //Success
        foreach (var item in Members)
        {
            var dtos = 
                (await _shareRepository.GetSharesListByMemberId(item.MemberId))
                .Select(x => x.Target)
                .ToList();
            var shares = 
                Shares.Where(x => x.MemeberId == item.MemberId)
                    .Select(x=> x.Target)
                    .ToList();
            dtos.ForEach(x => Assert.That(shares, Does.Contain(x)));
        }

        //fail
        for (int i = 0; i < Members.Count; i++)
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
        foreach (var item in Shares)
        {
            var dto = await _shareRepository.GetShareDownloadDtoByToken(item.Token);
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.Target, Is.EqualTo(item.Target));
        }
        
        //fail
        for (int i = 0; i < Shares.Count; i++)
        {
            var dto = await _shareRepository.GetShareDownloadDtoByToken(Guid.NewGuid());
            Assert.That(dto, Is.Null);   
        }
    }

    [Test]
    public async Task GetSharesByTargetFilePath()
    {
        //suc
        foreach (var item in Shares)
        {
            var dtos = await _shareRepository.GetSharesByTargetFilePath(item.MemeberId, item.Target);
            Assert.That(dtos, Is.Not.Empty);
            dtos.ForEach(x => Assert.That(x.Target, Is.EqualTo(item.Target)));
        }
        
        //fail
        for (int i = 0; i < Shares.Count; i++)
        {
            var dto = await _shareRepository.GetSharesByTargetFilePath(FailMemberId, _faker.Random.Words());
            Assert.That(dto, Is.Empty);   
        }
    }

    [Test]
    public async Task GetSharesInDirectory()
    {
        foreach (var share in Shares)
        {
            var dir = Path.GetDirectoryName(share.Target);
            var res = await _shareRepository.GetSharesInDirectory(share.MemeberId, dir!);
            var tokens = res.Select(x => x.Token).ToList();
            Assert.That(res, Is.Not.Empty);
            Assert.That(tokens, Does.Contain(share.Token.ToString()));
        }

        
        for (int i = 0; i < Shares.Count; i++)
        {
            var res = await _shareRepository.GetSharesInDirectory(FailMemberId, _faker.Random.Word());
            Assert.That(res, Is.Empty);
        }
    }

    [Test]
    public async Task GetPasswordHashByToken()
    {
        foreach (var share in Shares)
        {
            var res = await _shareRepository.GetPasswordHashByToken(share.Token);
            Assert.That(res, Is.Not.Null);
            Assert.That(PasswordEncrypt.VerifyPassword(share.Password, res!), Is.True);
        }

        for (int i = 0; i < Shares.Count(); i++)
        {
            var res = await _shareRepository.GetPasswordHashByToken(_faker.Random.Uuid());
            Assert.That(res, Is.Null);
        }
    }

    [Test]
    public async Task TrySetShareExpireTimeToZero()
    {
        foreach (var share in Shares)
        {
            var res = await _shareRepository.TrySetShareExpireTimeToZero(share.MemeberId, share.Token);
            Assert.That(res, Is.True);
            
        }

        for (int i = 0; i < Shares.Count(); i++)
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
            var share = Share.GetFake(_faker, _faker.Random.ULong(), _faker.Random.ULong(1, (ulong)Members.Count));
            var res = await _shareRepository.TryAddShare(share.MemeberId, share.Target,
                PasswordEncrypt.EncryptPassword(share.Password), share.ExpireTime, share.Comment, share.ShareName,
                share.Token, share.FileSize);
            Assert.That(res, Is.True);
        }

        for (int i = 0; i < addCount; i++)
        {
            var share = Share.GetFake(_faker, _faker.Random.ULong(), FailMemberId);
            var res = await _shareRepository.TryAddShare(share.MemeberId, share.Target,
                PasswordEncrypt.EncryptPassword(share.Password), share.ExpireTime, share.Comment, share.ShareName,
                share.Token, share.FileSize);
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task TryUpdateShare()
    {
        foreach (var share in Shares)
        {
            var update = Share.GetFake(_faker, share.Id, share.MemeberId);
            var res = await _shareRepository.TryUpdateShare(
                update.MemeberId, share.Token, PasswordEncrypt.EncryptPassword(update.Password), update.Comment,
                update.ExpireTime, update.ShareName);
            Assert.That(res, Is.True);
        }

        foreach (var update in Shares.Select(share => Share.GetFake(_faker, share.Id, FailMemberId)).ToList())
        {
            var res = await _shareRepository.TryUpdateShare(
                update.MemeberId, Guid.NewGuid(), PasswordEncrypt.EncryptPassword(update.Password), update.Comment,
                update.ExpireTime, update.ShareName);
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task TryDeleteShare()
    {
        foreach (var share in Shares)
        {
            var res = await _shareRepository.TryDeleteShare(share.MemeberId, share.Target);
            Assert.That(res, Is.True);
        }

        for (int i = 0; i < Shares.Count; i++)
        {
            var res = await _shareRepository.TryDeleteShare(FailMemberId, _faker.System.FilePath());
            Assert.That(res, Is.False);
        }
    }

    [Test]
    public async Task TryDeleteShareInDirectory()
    {
        foreach (var share in Shares)
        {
            var dir = Path.GetDirectoryName(share.Target);
            var res = await _shareRepository.TryDeleteShareInDirectory(share.MemeberId, dir!);
            var dircount = Shares.Count(x => Path.GetDirectoryName(x.Target) == dir 
                                             && share.MemeberId == x.MemeberId);
            Assert.That(res, Is.EqualTo(dircount));
        }
    }

    [Test]
    public async Task TryDeleteShareInDIrectoryFail()
    {
        for (int i = 0; i < Shares.Count(); i++)
        {
            var res = await _shareRepository.TryDeleteShareInDirectory(FailMemberId, _faker.Random.Words());
            Assert.That(res, Is.EqualTo(0));
        }
    }
    

}