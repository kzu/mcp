using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Runtime.InteropServices;

namespace ModelContextProtocol.Tests.Server;

public class McpServerLoggingLevelTests
{
    public McpServerLoggingLevelTests()
    {
#if !NET
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Assert.Ignore("https://github.com/modelcontextprotocol/csharp-sdk/issues/587");
#endif
    }

    [Test]
    public async Task CanCreateServerWithLoggingLevelHandler()
    {
        var services = new ServiceCollection();

        services.AddMcpServer()
            .WithStreamServerTransport(Stream.Null, Stream.Null)
            .WithSetLoggingLevelHandler(async (ctx, ct) => new EmptyResult());

        await using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<McpServer>();
    }

    [Test]
    public async Task AddingLoggingLevelHandlerSetsLoggingCapability()
    {
        var services = new ServiceCollection();

        services.AddMcpServer()
            .WithStreamServerTransport(Stream.Null, Stream.Null)
            .WithSetLoggingLevelHandler(async (ctx, ct) => new EmptyResult());

        await using var provider = services.BuildServiceProvider();

        var server = provider.GetRequiredService<McpServer>();

        Assert.NotNull(server.ServerOptions.Capabilities?.Logging);
        Assert.NotNull(server.ServerOptions.Handlers.SetLoggingLevelHandler);
    }

    [Test]
    public async Task ServerWithoutCallingLoggingLevelHandlerDoesNotSetLoggingCapability()
    {
        var services = new ServiceCollection();
        services.AddMcpServer()
            .WithStreamServerTransport(Stream.Null, Stream.Null);
        await using var provider = services.BuildServiceProvider();
        var server = provider.GetRequiredService<McpServer>();
        Assert.Null(server.ServerOptions.Capabilities?.Logging);
    }
}
