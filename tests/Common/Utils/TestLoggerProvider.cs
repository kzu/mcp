using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace ModelContextProtocol.Tests.Utils;

/// <summary>
/// Logger provider that writes to a TextWriter (e.g. TestContext.Out under NUnit).
/// </summary>
public sealed class TestLoggerProvider(TextWriter output) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(output, categoryName);
    }

    public void Dispose()
    {
    }

    private sealed class TestLogger(TextWriter output, string category) : ILogger
    {
        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var sb = new StringBuilder();

            var timestamp = DateTimeOffset.UtcNow.ToString("s", CultureInfo.InvariantCulture);
            var prefix = $"| [{timestamp}] {category} {logLevel}: ";
            var lines = formatter(state, exception);
            sb.Append(prefix);
            sb.Append(lines);

            if (exception is not null)
            {
                sb.AppendLine();
                sb.Append(exception.ToString());
            }

            try
            {
                output.WriteLine(sb.ToString());
            }
            catch (InvalidOperationException)
            {
                // Ignore when the test output writer has been closed (e.g. after test completion).
            }
            catch (ObjectDisposedException)
            {
                // Writer may be disposed after test.
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => new NoopDisposable();

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}