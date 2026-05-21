using ModelContextProtocol.Server;

namespace ModelContextProtocol.Tests.Server;

public class McpHeaderAttributeTests
{
    [TestCase("Region")]
    [TestCase("TenantId")]
    [TestCase("Priority")]
    [TestCase("X-Custom")]
    public void Constructor_ValidHeaderName_Succeeds(string name)
    {
        var attr = new McpHeaderAttribute(name);
        Assert.Equal(name, attr.Name);
    }

    [Test]
    public void Constructor_NameWithSpace_Throws()
    {
        Assert.Throws<ArgumentException>(() => new McpHeaderAttribute("My Region"));
    }

    [Test]
    public void Constructor_NameWithColon_Throws()
    {
        Assert.Throws<ArgumentException>(() => new McpHeaderAttribute("Region:Primary"));
    }

    [Test]
    public void Constructor_NullName_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => new McpHeaderAttribute(null!));
    }

    [Test]
    public void Constructor_EmptyName_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => new McpHeaderAttribute(""));
    }

    [Test]
    public void Constructor_WhitespaceName_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => new McpHeaderAttribute("  "));
    }

    [Test]
    public void Constructor_NameWithControlCharacter_Throws()
    {
        Assert.Throws<ArgumentException>(() => new McpHeaderAttribute("Region\t1"));
    }

    [Test]
    public void Constructor_NameWithNonAscii_Throws()
    {
        Assert.Throws<ArgumentException>(() => new McpHeaderAttribute("Région"));
    }
}
