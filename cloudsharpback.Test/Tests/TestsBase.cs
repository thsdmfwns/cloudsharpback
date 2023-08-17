using NUnit.Framework.Interfaces;

namespace cloudsharpback.Test.Tests;

public abstract class TestsBase
{

    [TearDown]
    public void TearDown()
    {
        var test = TestContext.CurrentContext.Test;
        var testResult = TestContext.CurrentContext.Result;
        if (testResult.Outcome == ResultState.Success)
        {
            Utils.PassCount++;
        }
        if (testResult.Outcome == ResultState.Failure)
        {
            Utils.FailCount++;
        }


        Console.WriteLine($"|-------------------------------------------------------------------------------------------|");
        Console.WriteLine($"Test Case '{test.Name}' {testResult.Outcome} ");
        Console.WriteLine($"From {test.ClassName}");
        Console.WriteLine($"PASS : {Utils.PassCount} | FAIL : {Utils.FailCount}");
        Console.WriteLine($"|-------------------------------------------------------------------------------------------|");
    }
}