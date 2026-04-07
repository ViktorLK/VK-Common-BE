using FluentAssertions;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Features.SemanticAttributes;

namespace VK.Blocks.Authentication.UnitTests.Features.SemanticAttributes;

public sealed class SemanticAttributesTests
{
    [Fact]
    public void ApiKeyAuthorizeAttribute_ShouldBeInstantiatable()
    {
        // Act
        var attribute = new ApiKeyAuthorizeAttribute();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void JwtAuthorizeAttribute_ShouldBeInstantiatable()
    {
        // Act
        var attribute = new JwtAuthorizeAttribute();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void AuthGroupAttribute_ShouldSetCorrectPolicy()
    {
        // Arrange
        var groupName = "Admins";
        var expectedPolicy = $"{AuthenticationConstants.GroupPolicyPrefix}{groupName}";

        // Act
        var attribute = new AuthGroupAttribute(groupName);

        // Assert
        attribute.Policy.Should().Be(expectedPolicy);
    }
}
