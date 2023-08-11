using Bogus;
using cloudsharpback.Models.DTO.Share;
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
            var item = Share.GetFake(faker, (ulong)i + 1, members.ElementAt(Random.Shared.Next(0, members.Count-1)).MemberId);
            list.Add(item);
            await InsertRow(item);
        }
        return list;
    }

    private static async Task DeleteAllRows()
    {
        using var conn = DBConnectionFactoryMock.Mock.Connection;
        await conn.ExecuteAsync("DELETE FROM share");
    }
    
    private static async Task InsertRow(Share item)
    {
        var insertSQL = @"
INSERT INTO share
VALUES (@id, @memberId, @target, @password, @expireTime, @comment, @shareTime, @shareName, UUID_TO_BIN(@token), @fileSize)
";
        using var conn = DBConnectionFactoryMock.Mock.Connection;
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
        var conn = DBConnectionFactoryMock.Mock.Connection;
        return (await conn.QueryAsync<Share>(sql)).ToList();
    }


    [Test]
    public async Task GetShareByToken()
    {
        //success
        foreach (var item in _shares)
        {
            var res = await _shareRepository.GetShareByToken(Guid.Parse(item.Token));
            Assert.That(res, Is.Not.Null);
            var mem = _members.Single(x => x.MemberId == item.MemberId);
            var dto = new ShareResponseDto()
            {
                Comment = item.Comment,
                ExpireTime = item.ExpireTime,
                FileSize = item.FileSize,
                HasPassword = true,
                OwnerId = item.MemberId,
                OwnerNick = mem.Nick,
                ShareName = item.ShareName,
                ShareTime = item.ShareTime,
                Target = item.Target,
                Token = item.Token
            };
            Assert.That(Utils.ToJson(res!), Is.EqualTo(Utils.ToJson(dto)));
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
            var res = 
                (await _shareRepository.GetSharesListByMemberId(item.MemberId))
                .OrderBy(x => x.Token)
                .ToList();
            var shares = 
                _shares.Where(x => x.MemberId == item.MemberId)
                    .Select( x => new ShareResponseDto()
                    {
                        Comment = x.Comment,
                        ExpireTime = x.ExpireTime,
                        FileSize = x.FileSize,
                        HasPassword = true,
                        OwnerId = x.MemberId,
                        OwnerNick = item.Nick,
                        ShareName = x.ShareName,
                        ShareTime = x.ShareTime,
                        Target = x.Target,
                        Token = x.Token
                    })
                    .OrderBy(x => x.Token)
                    .ToList();
            Assert.That(Utils.ToJson(res), Is.EqualTo(Utils.ToJson(shares)));
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
            var res = await _shareRepository.GetShareDownloadDtoByToken(Guid.Parse(item.Token));
            Assert.That(res, Is.Not.Null);
            var mem = _members.Single(x => x.MemberId == item.MemberId);
            var dto = new ShareDownloadDto()
            {
                Directory = mem.Dir,
                ExpireTime = item.ExpireTime,
                Password = item.Password,
                Target = item.Target
            };
            Assert.That(Test.Utils.ClassToJson(res!), Is.EqualTo(Test.Utils.ClassToJson(dto)));
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
            var res = (await _shareRepository.GetSharesByTargetFilePath(item.MemberId, item.Target))
                .OrderBy(x => x.Token)
                .ToList();
            Assert.That(res, Is.Not.Empty);
            var mem = _members.Single(x => x.MemberId == item.MemberId);
            var shares = 
                _shares.Where(x => x.MemberId == item.MemberId
                    && x.Target == item.Target)
                    .Select( x => new ShareResponseDto()
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
        foreach (var item in _shares)
        {
            var dir = Path.GetDirectoryName(item.Target);
            var res = await _shareRepository.GetSharesInDirectory(item.MemberId, dir!);
            Assert.That(res, Is.Not.Empty);
            var mem = _members.Single(x => x.MemberId == item.MemberId);
            var shares = 
                _shares.Where(x => x.MemberId == item.MemberId
                                   && x.Target.StartsWith(dir!))
                    .Select( x => new ShareResponseDto()
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
            var target = (await GetAllRows()).FirstOrDefault(x => x.MemberId == share.MemberId && x.Target == share.Target);
            Assert.That(target, Is.Null);
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
            var targets = (await GetAllRows()).Where(x => x.Target.StartsWith(dir!)
                                                          && x.MemberId == share.MemberId);
            Assert.That(targets, Is.Empty);
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