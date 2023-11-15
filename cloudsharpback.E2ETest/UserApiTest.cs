using System.Net;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

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
            email = _faker.Internet.Email(),
            nick = _faker.Internet.UserName()
        };
        var regRes = await PostAsync("/api/User/register", regDto);
        Assert.That(regRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var loginDto = new
        {
            id = "test1",
            password = "test1"
        };
        var loginRes = await PostAsync("/api/User/login", loginDto);
        Assert.That(loginRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    }

    [Test]
    public async Task Login()
    {
        var dto = new
        {
            id = MasterId,
            password = MasterPw
        };
        var loginRes = await PostAsync("/api/User/login", dto);
        Assert.That(loginRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await loginRes.Content.ReadAsStringAsync();
        var obj = JObject.Parse(await loginRes.Content.ReadAsStringAsync());
        Assert.That(obj.GetValue("accessToken"), Is.Not.Null);
        Assert.That(obj.GetValue("refreshToken"), Is.Not.Null);
    }

    [Test]
    public async Task IdCheck()
    {
        var checkDto = new
        {
            id = "master123"
        };
        var checkRes = await GetAsync("/api/User/idCheck", qurey: checkDto);
        Assert.That(checkRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = bool.TryParse(await checkRes.Content.ReadAsStringAsync(), out var result); 
        Assert.That(content, Is.True);
        Assert.That(result, Is.False);

        checkDto = new
        {
            id = MasterId
        };
        checkRes = await GetAsync("/api/User/idCheck", qurey: checkDto);
        Assert.That(checkRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        content = bool.TryParse(await checkRes.Content.ReadAsStringAsync(), out result); 
        Assert.That(content, Is.True);
        Assert.That(result, Is.True);
    }
}