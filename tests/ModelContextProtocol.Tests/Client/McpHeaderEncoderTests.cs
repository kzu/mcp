using ModelContextProtocol.Protocol;

namespace ModelContextProtocol.Tests.Client;

public class McpHeaderEncoderTests
{
    [Test]
    [TestCase("us-west1", "us-west1")]
    [TestCase("hello-world", "hello-world")]
    [TestCase("my_tool_name", "my_tool_name")]
    [TestCase("us west 1", "us west 1")]
    [TestCase("", "")]
    public void EncodeValue_PlainAscii_PassesThrough(string input, string expected)
    {
        var result = McpHeaderEncoder.EncodeValue(input);
        Assert.Equal(expected, result);
    }

    [Test]
    [TestCase(" us-west1", "=?base64?IHVzLXdlc3Qx?=")]
    [TestCase("us-west1 ", "=?base64?dXMtd2VzdDEg?=")]
    [TestCase(" us-west1 ", "=?base64?IHVzLXdlc3QxIA==?=")]
    [TestCase("\tindented", "=?base64?CWluZGVudGVk?=")]
    public void EncodeValue_LeadingTrailingWhitespace_Base64Encodes(string input, string expected)
    {
        var result = McpHeaderEncoder.EncodeValue(input);
        Assert.Equal(expected, result);
    }

    [Test]
    public void EncodeValue_NonAsciiCharacters_Base64Encodes()
    {
        var result = McpHeaderEncoder.EncodeValue("日本語");
        Assert.Equal("=?base64?5pel5pys6Kqe?=", result);
    }

    [Test]
    public void EncodeValue_NewlineCharacter_Base64Encodes()
    {
        var result = McpHeaderEncoder.EncodeValue("line1\nline2");
        Assert.Equal("=?base64?bGluZTEKbGluZTI=?=", result);
    }

    [Test]
    public void EncodeValue_CarriageReturnNewline_Base64Encodes()
    {
        var result = McpHeaderEncoder.EncodeValue("line1\r\nline2");
        Assert.Equal("=?base64?bGluZTENCmxpbmUy?=", result);
    }

    [Test]
    [TestCase(true, "true")]
    [TestCase(false, "false")]
    public void EncodeValue_Boolean_ConvertsToLowercase(bool input, string expected)
    {
        var result = McpHeaderEncoder.EncodeValue(input);
        Assert.Equal(expected, result);
    }

    [Test]
    [TestCase(42, "42")]
    [TestCase(3.14, "3.14")]
    [TestCase(0, "0")]
    [TestCase(-1, "-1")]
    public void EncodeValue_Number_ConvertsToString(object input, string expected)
    {
        var result = McpHeaderEncoder.EncodeValue(input);
        Assert.Equal(expected, result);
    }

    [Test]
    public void EncodeValue_Null_ReturnsNull()
    {
        var result = McpHeaderEncoder.EncodeValue(null);
        Assert.Null(result);
    }

    [Test]
    public void EncodeValue_UnsupportedType_ReturnsNull()
    {
        var result = McpHeaderEncoder.EncodeValue(new object());
        Assert.Null(result);
    }

    [Test]
    [TestCase("us-west1", "us-west1")]
    [TestCase("", "")]
    public void DecodeValue_PlainAscii_ReturnsAsIs(string input, string expected)
    {
        var result = McpHeaderEncoder.DecodeValue(input);
        Assert.Equal(expected, result);
    }

    [Test]
    public void DecodeValue_Null_ReturnsNull()
    {
        var result = McpHeaderEncoder.DecodeValue(null);
        Assert.Null(result);
    }

    [Test]
    public void DecodeValue_ValidBase64_Decodes()
    {
        var result = McpHeaderEncoder.DecodeValue("=?base64?SGVsbG8=?=");
        Assert.Equal("Hello", result);
    }

    [Test]
    public void DecodeValue_CaseInsensitivePrefix_Decodes()
    {
        var result = McpHeaderEncoder.DecodeValue("=?BASE64?SGVsbG8=?=");
        Assert.Equal("Hello", result);
    }

    [Test]
    public void DecodeValue_InvalidBase64_ReturnsNull()
    {
        var result = McpHeaderEncoder.DecodeValue("=?base64?SGVs!!!bG8=?=");
        Assert.Null(result);
    }

    [Test]
    public void DecodeValue_MissingPrefix_ReturnsLiteralValue()
    {
        var result = McpHeaderEncoder.DecodeValue("SGVsbG8=");
        Assert.Equal("SGVsbG8=", result);
    }

    [Test]
    public void DecodeValue_MissingSuffix_ReturnsLiteralValue()
    {
        var result = McpHeaderEncoder.DecodeValue("=?base64?SGVsbG8=");
        Assert.Equal("=?base64?SGVsbG8=", result);
    }

    [Test]
    [TestCase("us-west1")]
    [TestCase("Hello, 世界")]
    [TestCase(" padded ")]
    [TestCase("line1\nline2")]
    [TestCase("\tindented")]
    [TestCase("a\tb")]
    public void RoundTrip_EncodeDecode_PreservesValue(string original)
    {
        var encoded = McpHeaderEncoder.EncodeValue(original);
        Assert.NotNull(encoded);

        var decoded = McpHeaderEncoder.DecodeValue(encoded);
        Assert.Equal(original, decoded);
    }

    [Test]
    public void EncodeValue_EmbeddedTab_Base64Encodes()
    {
        var result = McpHeaderEncoder.EncodeValue("col1\tcol2");
        Assert.StartsWith("=?base64?", result);
        Assert.EndsWith("?=", result);

        // Verify round-trip
        var decoded = McpHeaderEncoder.DecodeValue(result);
        Assert.Equal("col1\tcol2", decoded);
    }
}
