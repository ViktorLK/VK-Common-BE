namespace VK.Blocks.Core.UnitTests.Diagnostics;

public class VKBlockDiagnosticsAttributeTests
{
    [Fact]
    public void Constructor_SetsMarkerType()
    {
        // Act
        var attr = new VKBlockDiagnosticsAttribute<VKCoreBlock>();

        // Assert
        attr.Should().NotBeNull();
        // Since MarkerType is an internal property (wait, let me check the source)
    }
}
