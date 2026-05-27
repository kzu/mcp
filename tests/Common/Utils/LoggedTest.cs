using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ModelContextProtocol.Tests.Utils;

public class LoggedTest : IDisposable
{
    private readonly DelegatingTextWriter _outputWriter;

    public LoggedTest()
    {
        _outputWriter = new DelegatingTextWriter(() => TestContext.Out);
        LoggerProvider = new TestLoggerProvider(_outputWriter);
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddProvider(LoggerProvider);
            builder.AddProvider(MockLoggerProvider);
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }

    /// <summary>
    /// Provides a TextWriter for test output (writes to TestContext.Out).
    /// </summary>
    public TextWriter TestOutput => _outputWriter;

    public ILoggerFactory LoggerFactory { get; }

    public ILoggerProvider LoggerProvider { get; }

    public MockLoggerProvider MockLoggerProvider { get; } = new();

    public virtual void Dispose()
    {
        // Nothing to clear; delegating to TestContext.Out which is managed by NUnit
    }
}
