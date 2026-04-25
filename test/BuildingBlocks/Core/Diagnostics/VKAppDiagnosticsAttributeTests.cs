namespace VK.Blocks.Core.UnitTests.Diagnostics;

public class VKAppDiagnosticsAttributeTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange & Act
        var attr = new VKAppDiagnosticsAttribute("MyApp")
        {
            Version = "2.0.0",
            Description = "Test App"
        };

        // Assert
        attr.AppName.Should().Be("MyApp");
        attr.Version.Should().Be("2.0.0");
        attr.Description.Should().Be("Test App");
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var attr = new VKAppDiagnosticsAttribute("MyApp");

        // Assert
        attr.AppName.Should().Be("MyApp");
        attr.Version.Should().Be("1.0.0");
        attr.Description.Should().BeNull();
    }
}
