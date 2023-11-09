using System.Net;
using Newtonsoft.Json.Linq;

namespace cloudsharpback.E2ETest;

public class MemberApiTest : TestBase
{
    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
    }

    [Test]
    public async Task GetAccessToken()
    {
        //login master
        var loginDto = new
        {
            id = MasterId,
            password = MasterPw
        };
        var loginRes = await PostAsync("/api/User/login", loginDto);
        Assert.That(loginRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var jobject = JObject.Parse(await loginRes.Content.ReadAsStringAsync());
        var refreshToken = jobject.GetValue("refreshToken");
        Assert.That(string.IsNullOrEmpty(refreshToken?.ToString()), Is.False);
        Console.WriteLine(refreshToken);
        //get token
        var header = new
        {
            auth = refreshToken?.ToString()
        };
        var res = await PostAsync("/api/Member/token", header: header);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(string.IsNullOrEmpty(await res.Content.ReadAsStringAsync()), Is.False);
    }
    
}