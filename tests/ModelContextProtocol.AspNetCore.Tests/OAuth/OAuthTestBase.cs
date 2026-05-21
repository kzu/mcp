using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.AspNetCore.Tests.Utils;
using ModelContextProtocol.Authentication;
using System.Net;

namespace ModelContextProtocol.AspNetCore.Tests.OAuth;

public abstract class OAuthTestBase : KestrelInMemoryTest
{
    protected const string McpServerUrl = "http://localhost:5000";
    protected const string OAuthServerUrl = "https://localhost:7029";

    protected CancellationTokenSource TestCts { get; private set; } = null!;
    protected TestOAuthServer.Program TestOAuthServer { get; private set; } = null!;
    private Task _testOAuthRunTask = Task.CompletedTask;

    protected virtual bool ConfigureDefaultMcpMetadata => true;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        TestCts = new();

        // Let the HandleAuthorizationUrlAsync take a look at the Location header
        SocketsHttpHandler.AllowAutoRedirect = false;
        // The dev cert may not be installed on the CI, but AddJwtBearer requires an HTTPS backchannel by default.
        // The easiest workaround is to disable cert validation for testing purposes.
        SocketsHttpHandler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;

        TestOAuthServer = new TestOAuthServer.Program(XunitLoggerProvider, KestrelInMemoryTransport);
        _testOAuthRunTask = TestOAuthServer.RunServerAsync(cancellationToken: TestCts.Token);

        Builder.Services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Backchannel = HttpClient;
            options.Authority = OAuthServerUrl;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = McpServerUrl,
                ValidIssuer = OAuthServerUrl,
                NameClaimType = "name",
                RoleClaimType = "roles"
            };
        })
        .AddMcp(options =>
        {
            if (ConfigureDefaultMcpMetadata)
            {
                options.ResourceMetadata = new ProtectedResourceMetadata
                {
                    AuthorizationServers = { OAuthServerUrl },
                    ScopesSupported = ["mcp:tools"]
                };
            }
        });

        ConfigureMcpAuthenticationOptions(Builder.Services);

        Builder.Services.AddAuthorization();
        Builder.Services.AddMcpServer().WithHttpTransport();
    }

    [TearDown]
    public async Task TearDownOAuthAsync()
    {
        TestCts.Cancel();
        try
        {
            await _testOAuthRunTask;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            TestCts.Dispose();
        }
    }

    protected virtual void ConfigureMcpAuthenticationOptions(IServiceCollection services)
    {
    }

    protected async Task<WebApplication> StartMcpServerAsync(string path = "", string? authScheme = null, Action<WebApplication>? configureMiddleware = null)
    {
        // Wait for the OAuth server to be ready before starting the MCP server.
        // This prevents race conditions in CI where the OAuth server may not be
        // fully initialized when the first test request is made.
        await TestOAuthServer.ServerStarted.WaitAsync(TestContext.CurrentContext.CancellationToken);

        Builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters.ValidAudience = $"{McpServerUrl}{path}";
        });

        var app = Builder.Build();
        
        // Allow tests to add custom middleware before MapMcp
        configureMiddleware?.Invoke(app);
        
        app.MapMcp(path).RequireAuthorization(new AuthorizeAttribute
        {
            AuthenticationSchemes = authScheme
        });
        await app.StartAsync(TestContext.CurrentContext.CancellationToken);
        return app;
    }

    protected async Task<string?> HandleAuthorizationUrlAsync(Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken)
    {
        using var redirectResponse = await HttpClient.GetAsync(authorizationUri, cancellationToken);
        if (redirectResponse.StatusCode != HttpStatusCode.Redirect)
        {
            return null;
        }

        var location = redirectResponse.Headers.Location;

        if (location is not null && !string.IsNullOrEmpty(location.Query))
        {
            var queryParams = QueryHelpers.ParseQuery(location.Query);
            return queryParams["code"];
        }

        return null;
    }
}
