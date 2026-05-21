using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace ModelContextProtocol.Tests.Protocol;

public static class IconTests
{
    [Test]
    public static void Icon_SerializationRoundTrip_PreservesAllProperties()
    {
        // Arrange
        var original = new Icon
        {
            Source = "https://example.com/icon.png",
            MimeType = "image/png",
            Sizes = ["48x48"],
            Theme = "light"
        };

        // Act - Serialize to JSON
        string json = JsonSerializer.Serialize(original, McpJsonUtilities.DefaultOptions);
        
        // Act - Deserialize back from JSON
        var deserialized = JsonSerializer.Deserialize<Icon>(json, McpJsonUtilities.DefaultOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Source, deserialized.Source);
        Assert.Equal(original.MimeType, deserialized.MimeType);
        Assert.Equal(original.Sizes, deserialized.Sizes);
        Assert.Equal(original.Theme, deserialized.Theme);
    }

    [Test]
    public static void Icon_SerializationRoundTrip_WithOnlyRequiredProperties()
    {
        // Arrange
        var original = new Icon
        {
            Source = "data:image/svg+xml;base64,PHN2Zy4uLjwvc3ZnPg=="
        };

        // Act - Serialize to JSON
        string json = JsonSerializer.Serialize(original, McpJsonUtilities.DefaultOptions);
        
        // Act - Deserialize back from JSON
        var deserialized = JsonSerializer.Deserialize<Icon>(json, McpJsonUtilities.DefaultOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Source, deserialized.Source);
        Assert.Equal(original.MimeType, deserialized.MimeType);
        Assert.Equal(original.Sizes, deserialized.Sizes);
        Assert.Null(deserialized.Theme);
    }

    [Test]
    public static void Icon_HasCorrectJsonPropertyNames()
    {
        var icon = new Icon
        {
            Source = "https://example.com/icon.svg",
            MimeType = "image/svg+xml",
            Sizes = ["any"],
            Theme = "dark"
        };

        string json = JsonSerializer.Serialize(icon, McpJsonUtilities.DefaultOptions);

        Assert.Contains("\"src\":", json);
        Assert.Contains("\"mimeType\":", json);
        Assert.Contains("\"sizes\":", json);
        Assert.Contains("\"theme\":", json);
    }
    [TestCase("""{}""")]
    [TestCase("""{"mimeType":"image/png"}""")]
    [TestCase("""{"sizes":"48x48"}""")]
    [TestCase("""{"mimeType":"image/png","sizes":"48x48"}""")]
    public static void Icon_DeserializationWithMissingSrc_ThrowsJsonException(string invalidJson)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Icon>(invalidJson, McpJsonUtilities.DefaultOptions));
    }
    [TestCase("false")]
    [TestCase("true")]
    [TestCase("42")]
    [TestCase("[]")]
    public static void Icon_DeserializationWithInvalidJson_ThrowsJsonException(string invalidJson)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Icon>(invalidJson, McpJsonUtilities.DefaultOptions));
    }
}
