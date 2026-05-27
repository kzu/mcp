using System.Text;

namespace ModelContextProtocol.Tests.Utils;

/// <summary>
/// Delegates test output writes to the current TextWriter (NUnit uses TestContext.Out).
/// </summary>
public sealed class DelegatingTextWriter : TextWriter
{
    private readonly Func<TextWriter> _writerFactory;

    public DelegatingTextWriter(Func<TextWriter> writerFactory)
    {
        _writerFactory = writerFactory;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value) => _writerFactory().Write(value);
    public override void Write(string? value) => _writerFactory().Write(value);
    public override void WriteLine(string? value) => _writerFactory().WriteLine(value);
    public override void WriteLine() => _writerFactory().WriteLine();
}