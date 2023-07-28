using cloudsharpback.Repository;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Utills;

namespace cloudsharpback.Test;

public class MemberRepositoryTests
{
    private MemberRepository _repository => new MemberRepository(DBConnectionFactoryMock.Mock.Object);
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public async Task GetMemberById()
    {
        var idsucs = new List<ulong>();
        idsucs.Add(33);
        idsucs.Add(34);
        var idfails = new List<ulong>();
        idfails.Add(32);
        idfails.Add(35);

        var repo = _repository;
        foreach (var x in idsucs)
        {
            var res = await repo.GetMemberById(x);
            Assert.That(res, Is.Not.Null);
            Assert.That(res!.Id, Is.EqualTo(x));
        }
        foreach (var x in idfails)
        {
            Assert.That(await repo.GetMemberById(x), Is.Null);
        }
    }
    
    [Test]
    public async Task GetMemberByLoginId()
    {
        var idsucs = new List<(string, int)>();
        idsucs.Add(("test", 33));
        idsucs.Add(("string", 34));
        var idfails = new List<string>();
        idfails.Add("asd");
        idfails.Add("qwe");

        var repo = _repository;
        foreach (var x in idsucs)
        {
            var res = await repo.GetMemberByLoginId(x.Item1);
            Assert.That(res, Is.Not.Null);
            Assert.That(res!.Id, Is.EqualTo(x.Item2));
        }
        foreach (var x in idfails)
        {
            Assert.That(await repo.GetMemberByLoginId(x), Is.Null);
        }
    }
    
    [Test]
    public async Task TryUpdateMemberProfileImage()
    {
        var idsucs = new List<(ulong , string)>();
        idsucs.Add((33, "test1"));
        idsucs.Add((34, "test2"));
        var idfails = new List<(ulong , string)>();
        idfails.Add((32, "test3"));
        idfails.Add((35, "test4"));

        var repo = _repository;
        foreach (var x in idsucs)
        {
            Assert.That(await repo.TryUpdateMemberProfileImage(x.Item1, x.Item2), Is.True);
            var res = await repo.GetMemberById(x.Item1);
            Assert.That(res!.ProfileImage, Is.EqualTo(x.Item2));
        }
        foreach (var x in idfails)
        {
            Assert.That(await repo.TryUpdateMemberProfileImage(x.Item1, x.Item2), Is.False);
        }
    }
    
    [Test]
    public async Task TryUpdateMemberNickname()
    {
        var idsucs = new List<(ulong , string)>();
        idsucs.Add((33, "test1"));
        idsucs.Add((34, "test2"));
        var idfails = new List<(ulong , string)>();
        idfails.Add((32, "test3"));
        idfails.Add((35, "test4"));

        var repo = _repository;
        foreach (var x in idsucs)
        {
            Assert.That(await repo.TryUpdateMemberNickname(x.Item1, x.Item2), Is.True);
            var res = await repo.GetMemberById(x.Item1);
            Assert.That(res!.Nickname, Is.EqualTo(x.Item2));

        }
        foreach (var x in idfails)
        {
            Assert.That(await repo.TryUpdateMemberNickname(x.Item1, x.Item2), Is.False);
        }
    }
    
    [Test]
    public async Task TryUpdateMemberEmail()
    {
        var idsucs = new List<(ulong , string)>();
        idsucs.Add((33, "test1"));
        idsucs.Add((34, "test2"));
        var idfails = new List<(ulong , string)>();
        idfails.Add((32, "test3"));
        idfails.Add((35, "test4"));

        var repo = _repository;
        foreach (var x in idsucs)
        {
            Assert.That(await repo.TryUpdateMemberEmail(x.Item1, x.Item2), Is.True);
            var res = await repo.GetMemberById(x.Item1);
            Assert.That(res!.Email, Is.EqualTo(x.Item2));
        }
        foreach (var x in idfails)
        {
            Assert.That(await repo.TryUpdateMemberEmail(x.Item1, x.Item2), Is.False);
        }
    }
    
    [Test]
    public async Task GetMemberPasswordHashById()
    {
        var idsucs = new List<(ulong, string)>();
        idsucs.Add((33, "test"));
        idsucs.Add((34, "string"));
        var idfails = new List<ulong>();
        idfails.Add(32);
        idfails.Add(35);

        var repo = _repository;
        foreach (var x in idsucs)
        {
            var res = await repo.GetMemberPasswordHashById(x.Item1);
            Assert.That(res, Is.Not.Null);
            Assert.That(PasswordEncrypt.VerifyPassword(x.Item2, res!), Is.True);
        }
        foreach (var x in idfails)
        {
            Assert.That(await repo.GetMemberPasswordHashById(x), Is.Null);
        }
    }
    
    [Test]
    public async Task GetMemberPasswordHashByLoginId()
    {
        var idsucs = new List<(string, string)>();
        idsucs.Add(("test", "test"));
        idsucs.Add(("string", "string"));
        var idfails = new List<string>();
        idfails.Add("asd");
        idfails.Add("qwe");

        var repo = _repository;
        foreach (var x in idsucs)
        {
            var res = await repo.GetMemberPasswordHashByLoginId(x.Item1);
            Assert.That(res, Is.Not.Null);
            Assert.That(PasswordEncrypt.VerifyPassword(x.Item2, res!), Is.True);
        }
        foreach (var x in idfails)
        {
            Assert.That(await repo.GetMemberPasswordHashByLoginId(x), Is.Null);
        }
    }
    
    [Test]
    public async Task TryLoginIdDuplicate()
    {
        var idsucs = new List<string>();
        idsucs.Add("test");
        idsucs.Add("string");
        var idfails = new List<string>();
        idfails.Add("asd");
        idfails.Add("qwe");

        var repo = _repository;
        foreach (var x in idsucs)
        {
            Assert.That(await repo.TryLoginIdDuplicate(x), Is.True);
        }
        foreach (var x in idfails)
        {
            Assert.That(await repo.TryLoginIdDuplicate(x), Is.False);
        }
    }
    
    [Test]
    public async Task TryUpdateMemberPassword()
    {
        var idsucs = new List<(string, ulong)>();
        idsucs.Add(("test", 33));
        idsucs.Add(("string", 34));
        var idfails = new List<(string, ulong)>();
        idfails.Add(("asd", 35));
        idfails.Add(("qwe", 32));

        var repo = _repository;
        foreach (var x in idsucs)
        {
            Assert.That(await repo.TryUpdateMemberPassword(x.Item2, PasswordEncrypt.EncryptPassword(x.Item1)), Is.True);
            var res = await repo.GetMemberPasswordHashById(x.Item2);
            Assert.That(PasswordEncrypt.VerifyPassword(x.Item1, res!), Is.True);

        }
        foreach (var x in idfails)
        {
            Assert.That(await repo.TryUpdateMemberPassword(x.Item2, x.Item1), Is.False);
        }
    }
    
}