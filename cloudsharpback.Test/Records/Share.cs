using Bogus;
using Bogus.DataSets;
using cloudsharpback.Utills;

namespace cloudsharpback.Test.Records;

public record Share(
    ulong Id,
    ulong MemberId,
    string Target,
    string Password,
    ulong ExpireTime,
    string Comment,
    ulong ShareTime,
    string ShareName,
    string Token,
    ulong FileSize
    )
{
    public static Share GetFake(Faker faker, ulong id, ulong memberId)
    {
        return new Share(
            id,
            memberId,
            faker.System.FilePath(),
            PasswordEncrypt.EncryptPassword(faker.Internet.Password()),
            (ulong)DateTime.MaxValue.Ticks,
            faker.Random.Words(),
            (ulong)DateTime.UtcNow.Ticks,
            faker.Random.Word(),
            Guid.NewGuid().ToString(),
            faker.Random.ULong()
        );
    }

    public string ToCompareTestString()
        => Utils.ClassToJson(new
        {
            MemeberId = MemberId,
            Target,
            Password,
            ExpireTime,
            Comment,
            ShareName,
            Token,
            FileSize
        });
    
    public override string ToString()
        => Utils.ClassToJson(this);
};