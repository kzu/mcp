using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Tests.Utils;
using NUnit.Framework;

namespace ModelContextProtocol.AspNetCore.Tests.Utils;

[CancelAfter(60_000)]
public abstract class KestrelInMemoryTest : LoggedTest
{
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        KestrelInMemoryTransport = new();
        SocketsHttpHandler = new();
        Builder = WebApplication.CreateEmptyBuilder(new());
        Builder.Services.AddSingleton<IConnectionListenerFactory>(KestrelInMemoryTransport);
        Builder.WebHost.UseKestrelCore();
        Builder.Services.AddRoutingCore();
        Builder.Services.AddLogging();
        Builder.Services.AddSingleton<ILoggerProvider>(MockLoggerProvider);
        Builder.Services.AddSingleton(XunitLoggerProvider);
        Builder.Logging.SetMinimumLevel(LogLevel.Debug);

        SocketsHttpHandler.ConnectCallback = (context, token) =>
        {
            var connection = KestrelInMemoryTransport.CreateConnection(context.DnsEndPoint);
            return new(connection.ClientStream);
        };

        HttpClient = new HttpClient(SocketsHttpHandler);
        ConfigureHttpClient(HttpClient);
    }

    [TearDown]
    public void TearDownHttp()
    {
        HttpClient.Dispose();
        SocketsHttpHandler.Dispose();
    }

    public WebApplicationBuilder Builder { get; private set; } = null!;

    public HttpClient HttpClient { get; protected set; } = null!;

    public SocketsHttpHandler SocketsHttpHandler { get; private set; } = null!;

    public KestrelInMemoryTransport KestrelInMemoryTransport { get; private set; } = null!;

    protected static void ConfigureHttpClient(HttpClient httpClient)
    {
        httpClient.BaseAddress = new Uri("http://localhost:5000/");
        httpClient.Timeout = TestConstants.HttpClientTimeout;
    }
}
