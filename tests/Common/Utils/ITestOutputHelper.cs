namespace ModelContextProtocol.Tests.Utils;

public interface ITestOutputHelper
{
    string Output { get; }

    void Write(string message);

    void Write(string format, params object[] args);

    void WriteLine(string message);

    void WriteLine(string format, params object[] args);
}
