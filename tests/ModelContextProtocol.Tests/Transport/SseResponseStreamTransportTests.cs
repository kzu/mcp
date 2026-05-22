using ModelContextProtocol.Server;
using ModelContextProtocol.Tests.Utils;
using System.IO.Pipelines;

namespace ModelContextProtocol.Tests.Transport;

public class SseResponseStreamTransportTests() : LoggedTest()
{
    [Test]
    public async Task Can_Customize_MessageEndpoint()
    {
        var responsePipe = new Pipe();

        await using var transport = new SseResponseStreamTransport(responsePipe.Writer.AsStream(), "/my-message-endpoint");
        var transportRunTask = transport.RunAsync(TestExecutionContext.Current.CancellationToken);

        using var responseStreamReader = new StreamReader(responsePipe.Reader.AsStream());
        var firstLine = await responseStreamReader.ReadLineAsync(
#if NET
            TestExecutionContext.Current.CancellationToken
#endif
        );
        Assert.Equal("event: endpoint", firstLine);

        var secondLine = await responseStreamReader.ReadLineAsync(
#if NET
            TestExecutionContext.Current.CancellationToken
#endif
        );
        Assert.Equal("data: /my-message-endpoint", secondLine);

        responsePipe.Reader.Complete();
        responsePipe.Writer.Complete();
    }
}
