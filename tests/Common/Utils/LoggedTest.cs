using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ModelContextProtocol.Tests.Utils;

[CancelAfter(60_000)]
public class LoggedTest : IDisposable
{
    private readonly DelegatingTestOutputHelper _delegatingTestOutputHelper = new();

    [SetUp]
    public virtual void SetUp()
    {
        MockLoggerProvider.Clear();
        _delegatingTestOutputHelper.CurrentTestOutputHelper = new NUnitTestOutputHelper();
        XunitLoggerProvider = new XunitLoggerProvider(_delegatingTestOutputHelper);
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddProvider(XunitLoggerProvider);
            builder.AddProvider(MockLoggerProvider);
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }

    [TearDown]
    public virtual void TearDown()
    {
        _delegatingTestOutputHelper.CurrentTestOutputHelper = null;
        LoggerFactory.Dispose();
    }

    public ITestOutputHelper TestOutputHelper => _delegatingTestOutputHelper;
    public ILoggerFactory LoggerFactory { get; set; } = null!;
    public ILoggerProvider XunitLoggerProvider { get; private set; } = null!;
    public MockLoggerProvider MockLoggerProvider { get; } = new();

    public virtual void Dispose()
    {
        _delegatingTestOutputHelper.CurrentTestOutputHelper = null;
    }
}
