using ModelContextProtocol.Protocol;

namespace ModelContextProtocol.Tests.Client;

public class McpRequestHeadersTests
{
    [Test]
    public void McpHttpHeaders_HasCorrectValues()
    {
        Assert.Equal("Mcp-Session-Id", McpHttpHeaders.SessionId);
        Assert.Equal("MCP-Protocol-Version", McpHttpHeaders.ProtocolVersion);
        Assert.Equal("Last-Event-ID", McpHttpHeaders.LastEventId);
        Assert.Equal("Mcp-Method", McpHttpHeaders.Method);
        Assert.Equal("Mcp-Name", McpHttpHeaders.Name);
        Assert.Equal("Mcp-Param-", McpHttpHeaders.ParamPrefix);
    }

    [Test]
    public void McpErrorCode_HeaderMismatch_HasCorrectValue()
    {
        Assert.Equal(-32001, (int)McpErrorCode.HeaderMismatch);
    }
    [TestCase("DRAFT-2026-v1", true)]
    [TestCase("2025-11-25", false)]
    [TestCase("2025-06-18", false)]
    [TestCase("2024-11-05", false)]
    [TestCase(null, false)]
    [TestCase("", false)]
    public void SupportsStandardHeaders_ReturnsExpected(string? version, bool expected)
    {
        Assert.Equal(expected, McpHttpHeaders.SupportsStandardHeaders(version));
    }
}
