namespace cloudsharpback.E2ETest;

public class UserApiTest : TestBase
{
    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
    }

    [Test]
    public async Task RegisterAndLogin()
    {
        var regDto = new
        {
            id = "test1",
            pw = "test1",
            email = "test1@test.com",
            nick = "test1"
        };
        var regRes = await PostAsync("/api/User/register", regDto);
        Assert.That(regRes.IsSuccessStatusCode, Is.True);
        var loginDto = new
        {
            id = "test1",
            password = "test1"
        };
        var loginRes = await PostAsync("/api/User/login", loginDto);
        Assert.That(loginRes.IsSuccessStatusCode, Is.True);
    }
}