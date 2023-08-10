using Bogus;

namespace cloudsharpback.Test.Records;

public record PassKey(
    ulong password_store_key_id,
    ulong owner_id,
    string public_key,
    string private_key,
    int encrypt_algorithm,
    ulong created_time,
    string name,
    string comment
)
{
    public static PassKey GetFake(Faker faker, ulong id, ulong memberId)
    {
        return new PassKey(
            id,
            memberId,
            faker.Random.Hash(),
            faker.Random.Hash(),
            faker.Random.Int(1, 2),
            (ulong)faker.Date.Past().Ticks,
            faker.Random.Words(),
            faker.Random.Words()
        );
    }

    public string ToCompareTestString()
        => Utils.ClassToJson(new
        {
            owner_id,
            private_key,
            public_key,
            encrypt_algorithm,
            name,
            comment
        });

    public override string ToString()
        => Utils.ClassToJson(this);
}