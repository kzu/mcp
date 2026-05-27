using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ModelContextProtocol.AspNetCore.Tests;

public class MapMcpSseTests() : MapMcpTests()
{
    protected override bool UseStreamableHttp => false;
    protected override bool Stateless => false;

    protected override void ConfigureStateless(HttpServerTransportOptions options)
    {
        base.ConfigureStateless(options);
        options.EnableLegacySse = true;
    }

    [Test]
    [TestCase("/mcp")]
    [TestCase("/mcp/secondary")]
    public async Task Allows_Customizing_Route(string pattern)
    {
        Builder.Services.AddMcpServer().WithHttpTransport(options => options.EnableLegacySse = true);
        await using var app = Builder.Build();

        app.MapMcp(pattern);

        await app.StartAsync(TestContext.CurrentContext.CancellationToken);

        using var response = await HttpClient.GetAsync($"http://localhost:5000{pattern}/sse", HttpCompletionOption.ResponseHeadersRead, TestContext.CurrentContext.CancellationToken);
        response.EnsureSuccessStatusCode();
        using var sseStream = await response.Content.ReadAsStreamAsync(TestContext.CurrentContext.CancellationToken);
        using var sseStreamReader = new StreamReader(sseStream, System.Text.Encoding.UTF8);
        var eventLine = await sseStreamReader.ReadLineAsync(TestContext.CurrentContext.CancellationToken);
        var dataLine = await sseStreamReader.ReadLineAsync(TestContext.CurrentContext.CancellationToken);
        Assert.NotNull(eventLine);
        Assert.Equal("event: endpoint", eventLine);
        Assert.NotNull(dataLine);
        Assert.Equal($"data: {pattern}/message", dataLine[..dataLine.IndexOf('?')]);
    }

    [Test]
    [TestCase("/a", "/a/sse")]
    [TestCase("/a/", "/a/sse")]
    [TestCase("/a/b", "/a/b/sse")]
    public async Task CanConnect_WithMcpClient_AfterCustomizingRoute(string routePattern, string requestPath)
    {
        Builder.Services.AddMcpServer(options =>
        {
            options.ServerInfo = new()
            {
                Name = "TestCustomRouteServer",
                Version = "1.0.0",
            };
        }).WithHttpTransport(options => options.EnableLegacySse = true);
        await using var app = Builder.Build();

        app.MapMcp(routePattern);

        await app.StartAsync(TestContext.CurrentContext.CancellationToken);

        await using var mcpClient = await ConnectAsync(requestPath);

        Assert.Equal("TestCustomRouteServer", mcpClient.ServerInfo.Name);
    }

    [Test]
    public async Task EnablePollingAsync_ThrowsInvalidOperationException_InSseMode()
    {
        InvalidOperationException? capturedException = null;
        var pollingTool = McpServerTool.Create(async (RequestContext<CallToolRequestParams> context) =>
        {
            try
            {
                await context.EnablePollingAsync(retryInterval: TimeSpan.FromSeconds(1));
            }
            catch (InvalidOperationException ex)
            {
                capturedException = ex;
            }

            return "Complete";
        }, options: new() { Name = "polling_tool" });

        Builder.Services.AddMcpServer().WithHttpTransport(options => options.EnableLegacySse = true).WithTools([pollingTool]);

        await using var app = Builder.Build();
        app.MapMcp();

        await app.StartAsync(TestContext.CurrentContext.CancellationToken);

        await using var mcpClient = await ConnectAsync();

        await mcpClient.CallToolAsync("polling_tool", cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.NotNull(capturedException);
        Assert.Contains("Streamable HTTP", capturedException.Message, StringComparison.OrdinalIgnoreCase);
    }
}
