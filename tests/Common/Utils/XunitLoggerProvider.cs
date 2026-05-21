using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace ModelContextProtocol.Tests.Utils;

public class XunitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(output, categoryName);
    }

    public void Dispose()
    {
    }

    private class XunitLogger(ITestOutputHelper output, string category) : ILogger
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
                // Ignore exceptions when the active test output context has already completed.
            }
            catch (NullReferenceException)
            {
                // Some test output implementations may tear down internal state during shutdown.
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => new NoopDisposable();

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
