namespace VK.Blocks.Core.UnitTests.Security;

public class SecurityMetadataEntryTests
{
    [Fact]
    public void SecurityMetadataEntry_Initialization_ShouldSetProperties()
    {
        // Arrange & Act
        var entry = new VKSecurityMetadataEntry("TestKey", new { }, "TestModule");

        // Assert
        entry.Key.Should().Be("TestKey");
    }
}
