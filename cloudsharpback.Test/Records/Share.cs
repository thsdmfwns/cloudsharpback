using Bogus;
using Bogus.DataSets;

namespace cloudsharpback.Test.Records;

public record Share(
    ulong Id,
    ulong MemeberId,
    string Target,
    string Password,
    ulong ExpireTime,
    string Comment,
    ulong ShareTime,
    string ShareName,
    Guid Token,
    ulong FileSize
    )
{
    public static Share GetFake(Faker faker, ulong id, ulong memberId)
    {
        return new Share(
            id,
            memberId,
            faker.System.FilePath(),
            faker.Internet.Password(),
            (ulong)DateTime.MaxValue.Ticks,
            faker.Random.String(),
            (ulong)DateTime.UtcNow.Ticks,
            faker.Random.Word(),
            Guid.NewGuid(),
            faker.Random.ULong()
        );
    }
};