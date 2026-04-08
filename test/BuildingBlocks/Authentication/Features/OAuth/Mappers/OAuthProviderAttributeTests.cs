using FluentAssertions;
using VK.Blocks.Authentication.Features.OAuth.Mappers;

namespace VK.Blocks.Authentication.UnitTests.Features.OAuth.Mappers;

public sealed class OAuthProviderAttributeTests
{
    [Fact]
    public void Constructor_ShouldSetProviderName()
    {
        // Arrange
        var providerName = "Google";

        // Act
        var attribute = new OAuthProviderAttribute(providerName);

        // Assert
        attribute.ProviderName.Should().Be(providerName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WhenInvalidProviderName_ShouldThrow(string? providerName)
    {
        // Act
        var action = () => new OAuthProviderAttribute(providerName!);

        // Assert
        action.Should().Throw<ArgumentException>();
    }
}
