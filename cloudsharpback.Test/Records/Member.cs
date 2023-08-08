using Bogus;

namespace cloudsharpback.Test.Records;

public record Member(ulong MemberId, string Id, string Password, string Nick, string Email,
    string Dir, ulong Role, string ProfileImage)
{
    
    public static Member GetFake(ulong memberid, Faker faker)
    {
        return new Member(
            memberid,
            faker.Internet.UserName(),
            faker.Internet.Password(),
            faker.Internet.UserName(),
            faker.Internet.Email(),
            Guid.NewGuid().ToString(),
            2,
            faker.Image.PicsumUrl()
        );    
    }

    public override string ToString()
        => Utils.ToJson(this);
};