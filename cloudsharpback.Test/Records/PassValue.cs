using Bogus;

namespace cloudsharpback.Test.Records;

public record PassValue(
    ulong password_store_value_id,
    ulong directory_id,
    ulong encrypt_key_id,
    string value_id,
    string value_password,
    ulong created_time,
    ulong last_edited_time
)
{
    public static PassValue GetFake(Faker faker, ulong dirId, ulong keyId, ulong id)
    {
        return new PassValue(
            id,
            dirId,
            keyId,
            faker.Internet.UserName(),
            faker.Internet.Password(),
            (ulong)faker.Date.Past().Ticks,
            (ulong)faker.Date.Recent().Ticks
        );
    }

    public string ToCompareTestString()
        => Utils.ClassToJson(new
        {
            directory_id,
            encrypt_key_id,
            value_id,
            value_password
        });

    public override string ToString()
        => Utils.ClassToJson(this);
}