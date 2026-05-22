namespace ModelContextProtocol.AspNetCore.Tests;

public class MapMcpStatelessTests() : MapMcpStreamableHttpTests()
{
    protected override bool UseStreamableHttp => true;
    protected override bool Stateless => true;
}
