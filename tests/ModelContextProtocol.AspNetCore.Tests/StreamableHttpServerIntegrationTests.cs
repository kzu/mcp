using ModelContextProtocol.Client;
using System.Text;

namespace ModelContextProtocol.AspNetCore.Tests;

public class StreamableHttpServerIntegrationTests(SseServerIntegrationTestFixture fixture)
    : HttpServerIntegrationTests(fixture, testOutputHelper)

{
     private const string InitializeRequest = """
        {"jsonrpc":"2.0","id":"1","method":"initialize","params":{"protocolVersion":"2025-03-26","capabilities":{},"clientInfo":{"name":"IntegrationTestClient","version":"1.0.0"}}}
        """;

    protected override HttpClientTransportOptions ClientTransportOptions => new()
    {
        Endpoint = new("http://localhost:5000/"),
        Name = "In-memory Streamable HTTP Client",
        TransportMode = HttpTransportMode.StreamableHttp,
    };

    [Test]
    public async Task EventSourceResponse_Includes_ExpectedHeaders()
    {
        using var initializeRequestBody = new StringContent(InitializeRequest, Encoding.UTF8, "application/json");
        using var postRequest = new HttpRequestMessage(HttpMethod.Post, "/")
        {
            Headers =
            {
                Accept = { new("application/json"), new("text/event-stream")}
            },
            Content = initializeRequestBody,
        };
        using var sseResponse = await _fixture.HttpClient.SendAsync(postRequest, TestContext.CurrentContext.CancellationToken);

        sseResponse.EnsureSuccessStatusCode();

        Assert.Equal("text/event-stream", sseResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal("identity", sseResponse.Content.Headers.ContentEncoding.ToString());
        Assert.NotNull(sseResponse.Headers.CacheControl);
        Assert.True(sseResponse.Headers.CacheControl.NoStore);
        Assert.True(sseResponse.Headers.CacheControl.NoCache);
    }

    [Test]
    public async Task EventSourceStream_Includes_MessageEventType()
    {
        using var initializeRequestBody = new StringContent(InitializeRequest, Encoding.UTF8, "application/json");
        using var postRequest = new HttpRequestMessage(HttpMethod.Post, "/")
        {
            Headers =
            {
                Accept = { new("application/json"), new("text/event-stream")}
            },
            Content = initializeRequestBody,
        };
        using var sseResponse = await _fixture.HttpClient.SendAsync(postRequest, TestContext.CurrentContext.CancellationToken);
        using var sseResponseStream = await sseResponse.Content.ReadAsStreamAsync(TestContext.CurrentContext.CancellationToken);
        using var streamReader = new StreamReader(sseResponseStream);

        var messageEvent = await streamReader.ReadLineAsync(TestContext.CurrentContext.CancellationToken);
        Assert.Equal("event: message", messageEvent);
    }
}
