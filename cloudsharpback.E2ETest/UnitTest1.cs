namespace cloudsharpback.E2ETest;

public class UserApiTest : TestBase
{
    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}