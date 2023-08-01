using Bogus;

namespace cloudsharpback.Test.Records;

public record Member(ulong MemberId, string Id, string Password, string Nick, ulong Role, string Email,
    string Dir)
{
    
    public static Member GetFake(ulong memberid, Faker faker)
    {
        return new Member(
            memberid,
            faker.Internet.UserName(),
            faker.Internet.Password(),
            faker.Internet.UserName(),
            2,
            faker.Internet.Email(),
            Guid.NewGuid().ToString()
        );    
    }
};