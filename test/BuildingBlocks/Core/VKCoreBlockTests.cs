namespace VK.Blocks.Core.UnitTests;

public class VKCoreBlockTests
{
    [Fact]
    public void VKCoreBlock_Properties_ShouldReturnExpectedValues()
    {
        // Arrange
        var block = VKCoreBlock.Instance;

        // Assert
        block.Name.Should().Be("Core");
        block.Identifier.Should().Be("VK.Blocks.Core");
        block.Version.Should().Be("1.0.0");
        block.Dependencies.Should().BeEmpty();
        block.ActivitySourceName.Should().Be("VK.Blocks.Core");
        block.MeterName.Should().Be("VK.Blocks.Core");
    }
}
