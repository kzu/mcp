using Microsoft.Extensions.Logging;

namespace ModelContextProtocol.Tests.Utils;

public class LoggedTest : IDisposable
{
    private readonly DelegatingTestOutputHelper _delegatingTestOutputHelper;

    protected LoggedTest()
        : this(new NUnitTestOutputHelper())
    {
    }

    protected LoggedTest(ITestOutputHelper testOutputHelper)
    {
        _delegatingTestOutputHelper = new()
        {
            CurrentTestOutputHelper = testOutputHelper,
        };
        TestLoggerProvider = new TestLoggerProvider(_delegatingTestOutputHelper);
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddProvider(TestLoggerProvider);
            builder.AddProvider(MockLoggerProvider);
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }

    public ITestOutputHelper TestOutputHelper => _delegatingTestOutputHelper;
    public ILoggerFactory LoggerFactory { get; set; }
    public TestLoggerProvider TestLoggerProvider { get; }
    public MockLoggerProvider MockLoggerProvider { get; } = new();

    public virtual void Dispose()
    {
        _delegatingTestOutputHelper.CurrentTestOutputHelper = null;
    }
}
