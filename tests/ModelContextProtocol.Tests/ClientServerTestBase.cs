using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using ModelContextProtocol.Tests.Utils;
using System.IO.Pipelines;

namespace ModelContextProtocol.Tests;

[CancelAfter(60_000)]
public abstract class ClientServerTestBase : LoggedTest, IAsyncDisposable
{
    private Pipe _clientToServerPipe = null!;
    private Pipe _serverToClientPipe = null!;
    private CancellationTokenSource _cts = null!;
    private Task _serverTask = Task.CompletedTask;
    private McpServer? _server;
    private ServiceProvider? _serviceProvider;

    protected virtual bool StartServerOnSetUp => true;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _clientToServerPipe = new();
        _serverToClientPipe = new();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CurrentContext.CancellationToken);

        ServiceCollection = new ServiceCollection();
        ServiceCollection.AddLogging();
        ServiceCollection.AddSingleton(XunitLoggerProvider);
        ServiceCollection.AddSingleton<ILoggerProvider>(MockLoggerProvider);
        McpServerBuilder = ServiceCollection
            .AddMcpServer()
            .WithStreamServerTransport(_clientToServerPipe.Reader.AsStream(), _serverToClientPipe.Writer.AsStream());

        ConfigureServices(ServiceCollection, McpServerBuilder);

        if (StartServerOnSetUp)
        {
            StartServer();
        }
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        await DisposeAsync();
    }

    protected ServiceCollection ServiceCollection { get; private set; } = null!;

    protected IMcpServerBuilder McpServerBuilder { get; private set; } = null!;

    protected McpServer Server
    {
         get => _server ?? throw new InvalidOperationException("You must call StartServer first.");
         private set => _server = value;
    }

    protected ServiceProvider ServiceProvider
    {
         get => _serviceProvider ?? throw new InvalidOperationException("You must call StartServer first.");
         private set => _serviceProvider = value;
    }

    protected virtual void ConfigureServices(ServiceCollection services, IMcpServerBuilder mcpServerBuilder)
    {
    }

    protected McpServer StartServer()
    {
        ServiceProvider = ServiceCollection.BuildServiceProvider(validateScopes: true);
        Server = ServiceProvider.GetRequiredService<McpServer>();
        _serverTask = Server.RunAsync(_cts.Token);
        return Server;
    }

    public async ValueTask DisposeAsync()
    {
        if (_cts is null)
        {
            return;
        }

        await _cts.CancelAsync();

        _clientToServerPipe.Writer.Complete();
        _serverToClientPipe.Writer.Complete();

        await _serverTask;

        if (_serviceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
            _serviceProvider = null;
        }
        else if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
            _serviceProvider = null;
        }

        _server = null;

        _cts.Dispose();
        _cts = null!;
    }

    protected async Task<McpClient> CreateMcpClientForServer(McpClientOptions? clientOptions = null)
    {
        return await McpClient.CreateAsync(
            new StreamClientTransport(
                serverInput: _clientToServerPipe.Writer.AsStream(),
                _serverToClientPipe.Reader.AsStream(),
                LoggerFactory),
            clientOptions: clientOptions,
            loggerFactory: LoggerFactory,
            cancellationToken: TestContext.CurrentContext.CancellationToken);
    }
}
