using NUnit.Framework;

namespace ModelContextProtocol.Tests.Utils;

public static class TestExecutionContext
{
    public static TestExecutionContextCurrent Current { get; } = new();

    public sealed class TestExecutionContextCurrent
    {
        public CancellationToken CancellationToken => TestContext.CurrentContext.CancellationToken;
    }
}
