using NUnit.Framework;

namespace ModelContextProtocol.Tests.Utils;

#pragma warning disable NUnit1033 // Intentional ITestOutputHelper wrapper for xUnit migration compatibility
public sealed class NUnitTestOutputHelper : ITestOutputHelper
#pragma warning restore NUnit1033
{
    public string Output => string.Empty;

    public void Write(string message) => TestContext.Write(message);

    public void Write(string format, params object[] args) => TestContext.Write(format, args);

    public void WriteLine(string message) => TestContext.WriteLine(message);

    public void WriteLine(string format, params object[] args) => TestContext.WriteLine(format, args);
}
