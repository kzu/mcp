using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace ModelContextProtocol.Tests.Server;

public class McpServerToolNameValidationTests
{
    [Test]
    public void WithValidCharacters_Succeeds()
    {
        const string AllValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_.-";

        Validate(AllValidChars);

        foreach (char c in AllValidChars)
        {
            Validate($"{c}");
            Validate($"tool{c}name");
            Validate($"{c}toolname");
            Validate($"toolname{c}");
        }

        static void Validate(string toolName)
        {
            var tool = McpServerTool.Create(() => "result", new McpServerToolCreateOptions { Name = toolName });
            Assert.Equal(toolName, tool.ProtocolTool.Name);
        }
    }

    [Test]
    public void WithInvalidCharacters_Throws()
    {
        Validate("café");
        Validate("tööl");
        Validate("🔧tool");

        for (int i = 0; i < 256; i++)
        {
            char c = (char)i;
            if (c is not (>= 'a' and <= 'z')
                 and not (>= 'A' and <= 'Z')
                 and not (>= '0' and <= '9')
                 and not ('_' or '-' or '.'))
            {
                Validate($"{c}");
                Validate($"tool{c}name");
                Validate($"{c}toolname");
                Validate($"toolname{c}");
            }
        }

        static void Validate(string toolName)
        {
            var ex = Assert.Throws<ArgumentException>(() => McpServerTool.Create(() => "result", new McpServerToolCreateOptions { Name = toolName }));
            Assert.Contains(toolName, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Test]
    [TestCase(1)]
    [TestCase(10)]
    [TestCase(127)]
    [TestCase(128)]
    public void WithValidLengths_Succeeds(int length)
    {
        string validName = new('a', length);
        var tool = McpServerTool.Create(() => "result", new McpServerToolCreateOptions { Name = validName });
        Assert.Equal(validName, tool.ProtocolTool.Name);
    }

    [Test]
    [TestCase(0)]
    [TestCase(129)]
    [TestCase(130)]
    public void WithInvalidLengths_ThrowsArgumentException(int length)
    {
        string invalidName = new('a', length);
        var ex = Assert.Throws<ArgumentException>(() => McpServerTool.Create(() => "result", new McpServerToolCreateOptions { Name = invalidName }));
        Assert.Contains(invalidName, ex.Message);
    }

    [Test]
    public void UsingAttribute_ValidatesToolName()
    {
        var validTool = McpServerTool.Create([McpServerTool(Name = "valid_tool")] () => "result");
        Assert.Equal("valid_tool", validTool.ProtocolTool.Name);

        var ex = Assert.Throws<ArgumentException>(() => McpServerTool.Create([McpServerTool(Name = "invalid@tool")] () => "result"));
        Assert.Contains("invalid@tool", ex.Message);
    }

    [Test]
    public void UsingAttributeWithInvalidCharacters_ThrowsArgumentException()
    {
        var validTool = McpServerTool.Create([McpServerTool(Name = "tool")] () => "result");
        Assert.Equal("tool", validTool.ProtocolTool.Name);

        var ex = Assert.Throws<ArgumentException>(() => McpServerTool.Create([McpServerTool(Name = "tööl")] () => "result"));
        Assert.Contains("tööl", ex.Message);
    }

    [Test]
    public void FromMethodInfo_ValidatesToolName()
    {
        Assert.Equal(nameof(aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa), McpServerTool.Create(aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa).ProtocolTool.Name);

        var ex = Assert.Throws<ArgumentException>(() => McpServerTool.Create(aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa));
        Assert.Contains(nameof(aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa), ex.Message);
    }

    [Test]
    public void FromAIFunction_ValidatesToolName()
    {
        var validTool = McpServerTool.Create(AIFunctionFactory.Create(() => "result", new AIFunctionFactoryOptions { Name = "valid_ai" }));
        Assert.Equal("valid_ai", validTool.ProtocolTool.Name);

        var invalidFunction = AIFunctionFactory.Create(() => "result", new AIFunctionFactoryOptions { Name = "invalid ai" });
        var ex = Assert.Throws<ArgumentException>(() => McpServerTool.Create(invalidFunction));
        Assert.Contains("invalid ai", ex.Message);
    }

    [Test]
    public void FromNullAIFunctionName_ThrowsArgumentNullException()
    {
        AIFunction f = new NullNameAIFunction();
        Assert.Throws<ArgumentException>(() => McpServerTool.Create(f));
    }

    private static void aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa() { }

    private static void aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa() { }

    private sealed class NullNameAIFunction : AIFunction
    {
        public override string Name => null!;

        protected override ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
