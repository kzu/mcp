using ModelContextProtocol.Client;

namespace ModelContextProtocol.AspNetCore.Tests;

public class StatelessServerIntegrationTests : StreamableHttpServerIntegrationTests
{
    protected override HttpClientTransportOptions ClientTransportOptions => new()
    {
        Endpoint = new("http://localhost:5000/stateless"),
        Name = "In-memory Streamable HTTP Client",
        TransportMode = HttpTransportMode.StreamableHttp,
    };
}
