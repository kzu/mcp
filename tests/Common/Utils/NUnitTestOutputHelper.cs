namespace ModelContextProtocol.Tests.Utils;

public class NUnitTestOutputHelper : ITestOutputHelper
{
    public string Output => NUnit.Framework.TestContext.Out.ToString() ?? string.Empty;
    public void Write(string message) => NUnit.Framework.TestContext.Out.Write(message);
    public void Write(string format, params object[] args) => NUnit.Framework.TestContext.Out.Write(format, args);
    public void WriteLine(string message) => NUnit.Framework.TestContext.Out.WriteLine(message);
    public void WriteLine(string format, params object[] args) => NUnit.Framework.TestContext.Out.WriteLine(format, args);
}
