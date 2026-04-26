using FluentAssertions;

namespace VK.Blocks.Authentication.UnitTests.Common;

public sealed class SemanticAttributesTests
{
    [Fact]
    public void VKApiKeyAuthorizeAttribute_ShouldBeInstantiatable()
    {
        // Act
        var attribute = new VKApiKeyAuthorizeAttribute();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void VKJwtAuthorizeAttribute_ShouldBeInstantiatable()
    {
        // Act
        var attribute = new VKJwtAuthorizeAttribute();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void VKAuthGroupAttribute_ShouldSetCorrectPolicy()
    {
        // Arrange
        var groupName = "Admins";

        // Act
        var attribute = new VKAuthGroupAttribute(groupName);

        // Assert
        attribute.Policy.Should().Be(groupName);
    }
}
