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
        var token = await LoginMaster();
        //get token
        var header = new
        {
            auth = token.rf
        };
        using var res = await PostAsync("/api/Member/token", header: header);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(string.IsNullOrEmpty(await res.Content.ReadAsStringAsync()), Is.False);
    }

    [Test]
    public async Task GetMember()
    {
        var token = await LoginMaster();
        var header = new
        {
            auth = token.ac
        };
        using var res = await GetAsync("/api/Member/get", header: header);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(string.IsNullOrEmpty(await res.Content.ReadAsStringAsync()), Is.False);
    }

    [Test]
    public async Task UpdateProfileAndGet()
    {
        //UpdateProfile
        var filePath = "/TestData/testImage1.png";
        var fileInfo = new FileInfo(filePath);
        Assert.That(fileInfo.Exists, Is.True);
        var token = await LoginMaster();
        await using var uploadStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
        using var updateContent = new MultipartFormDataContent();
        updateContent.Add(new StreamContent(uploadStream), "image", Path.GetFileName(fileInfo.Name));
        using var updateRes = await PostAsync("/api/Member/updateProfile", updateContent, header: new { auth = token.ac });
        Assert.That(updateRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        //getProfile
        token = await LoginMaster();
        var header = new
        {
            auth = token.ac
        };
        var getMemberRes = await (await GetAsync("/api/Member/get", header: header)).Content.ReadAsStringAsync();
        var profile = JObject.Parse(getMemberRes).GetValue("profileImage");
        Assert.That(profile, Is.Not.Null);
        using var res = await GetAsync($"/api/Member/profile/{profile}", header: header);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var bytes = await res.Content.ReadAsByteArrayAsync();
        var updateHash = GetHash(await File.ReadAllBytesAsync(fileInfo.FullName));
        var hash = GetHash(bytes);
        Assert.That(hash, Is.EqualTo(updateHash));
    }

    [Test]
    public async Task UpdateNick()
    {
        var nickname = _faker.Internet.UserName();
        var token = await LoginMaster();
        var updateRes = await PostAsync("/api/Member/UpdateNick", qurey: new { nickname }, header: Header());
        Assert.That(updateRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        token = await LoginMaster();
        var memberRes = await GetAsync("/api/Member/Get", header: Header());
        Assert.That(memberRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var memberJson = JObject.Parse(await memberRes.Content.ReadAsStringAsync());
        var updatedNick = memberJson.GetValue("nickname")?.ToString();
        Assert.That(updatedNick, Is.Not.Null);
        Assert.That(updatedNick, Is.EqualTo(nickname));
        return;

        object Header() => new
        {
            auth = token.ac
        };
    }
    
    [Test]
    public async Task UpdateEmail()
    {
        var email = _faker.Internet.Email();
        var token = await LoginMaster();
        var updateRes = await PostAsync("/api/Member/UpdateEmail", qurey: new { email }, header: Header());
        Assert.That(updateRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        token = await LoginMaster();
        var memberRes = await GetAsync("/api/Member/Get", header: Header());
        Assert.That(memberRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var memberJson = JObject.Parse(await memberRes.Content.ReadAsStringAsync());
        var updatedNick = memberJson.GetValue("email")?.ToString();
        Assert.That(updatedNick, Is.Not.Null);
        Assert.That(updatedNick, Is.EqualTo(email));
        return;

        object Header() => new
        {
            auth = token.ac
        };
    }
    
    [Test]
    public async Task UpdatePassword()
    {
        var password = _faker.Internet.Password();
        var dto = new
        {
            original = MasterPw,
            changeTo = password
        };
        var token = await LoginMaster();
        var updateRes = await PostAsync("/api/Member/UpdatePw", dto, header: Header());
        Assert.That(updateRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await LoginMaster(pw: password);
        return;
        
        object Header() => new
        {
            auth = token.ac
        };
    }
    
    

}