using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace ModelContextProtocol.Tests.Protocol;

public static class ProtocolTypeTests
{
    [TestCase(Role.User, "\"user\"")]
    [TestCase(Role.Assistant, "\"assistant\"")]
    public static void SerializeRole_ShouldBeCamelCased(Role role, string expectedValue)
    {
        var actualValue = JsonSerializer.Serialize(role, McpJsonUtilities.DefaultOptions);

        Assert.Equal(expectedValue, actualValue);
    }
    [TestCase(LoggingLevel.Debug, "\"debug\"")]
    [TestCase(LoggingLevel.Info, "\"info\"")]
    [TestCase(LoggingLevel.Notice, "\"notice\"")]
    [TestCase(LoggingLevel.Warning, "\"warning\"")]
    [TestCase(LoggingLevel.Error, "\"error\"")]
    [TestCase(LoggingLevel.Critical, "\"critical\"")]
    [TestCase(LoggingLevel.Alert, "\"alert\"")]
    [TestCase(LoggingLevel.Emergency, "\"emergency\"")]
    public static void SerializeLoggingLevel_ShouldBeCamelCased(LoggingLevel level, string expectedValue)
    {
        var actualValue = JsonSerializer.Serialize(level, McpJsonUtilities.DefaultOptions);

        Assert.Equal(expectedValue, actualValue);
    }
    [TestCase(ContextInclusion.None, "\"none\"")]
    [TestCase(ContextInclusion.ThisServer, "\"thisServer\"")]
    [TestCase(ContextInclusion.AllServers, "\"allServers\"")]
    public static void ContextInclusion_ShouldBeCamelCased(ContextInclusion level, string expectedValue)
    {
        var actualValue = JsonSerializer.Serialize(level, McpJsonUtilities.DefaultOptions);

        Assert.Equal(expectedValue, actualValue);
    }
}
