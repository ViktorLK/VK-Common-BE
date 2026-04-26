using System;
using FluentAssertions;

namespace VK.Blocks.Authentication.UnitTests.OAuth;

public sealed class VKOAuthProviderAttributeTests
{
    [Fact]
    public void Constructor_ShouldSetProviderName()
    {
        // Arrange
        var providerName = "Google";

        // Act
        var attribute = new VKOAuthProviderAttribute(providerName);

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
        var action = () => new VKOAuthProviderAttribute(providerName!);

        // Assert
        action.Should().Throw<ArgumentException>();
    }
}
