using Bogus;

namespace cloudsharpback.Test.Records;

public record PassDir(
    ulong password_directory_id,
    string name,
    string? comment,
    string? icon,
    ulong last_edited_time,
    ulong created_time,
    ulong member_id)
{
    public static PassDir GetFake(Faker faker, ulong id, ulong memberId)
    {
        return new PassDir(
            id,
            faker.Name.FullName(),
            faker.Random.Words(),
            faker.Image.PicsumUrl(),
            (ulong)faker.Date.Recent().Ticks,
            (ulong)faker.Date.Past().Ticks,
            memberId);
    }

    public string ToCompareTestString()
        => Utils.ClassToJson(new
        {
            name,
            comment,
            icon,
            member_id
        });

    public override string ToString()
        => Utils.ClassToJson(this);

}