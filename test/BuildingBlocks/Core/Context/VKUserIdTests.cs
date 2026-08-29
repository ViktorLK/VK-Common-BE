using VK.Blocks.Core;

namespace VK.Blocks.Core.UnitTests.Context;

public sealed class VKUserIdTests
{
    [Fact]
    public void Anonymous_ShouldReturnExpectedSentinelGuid()
    {
        // Arrange
        var expectedGuid = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Act
        var anonymousUser = VKUserId.Anonymous;

        // Assert
        anonymousUser.Value.Should().Be(expectedGuid);
        anonymousUser.Should().NotBe(default(VKUserId));
    }

    [Fact]
    public void System_ShouldReturnExpectedSentinelGuid()
    {
        // Arrange
        var expectedGuid = Guid.Parse("00000000-0000-0000-0000-000000000003");

        // Act
        var systemUser = VKUserId.System;

        // Assert
        systemUser.Value.Should().Be(expectedGuid);
        systemUser.Should().NotBe(default(VKUserId));
    }

    [Fact]
    public void FromNullable_ValidGuidString_ReturnsMatchingUserId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = VKUserId.FromNullable(guid.ToString());

        // Assert
        result.Value.Should().Be(guid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-guid")]
    public void FromNullable_InvalidOrEmptyInput_ReturnsAnonymousUser(string? input)
    {
        // Act
        var result = VKUserId.FromNullable(input);

        // Assert
        result.Should().Be(VKUserId.Anonymous);
    }

    [Fact]
    public void Sentinels_ShouldBeDistinct()
    {
        VKUserId.Anonymous.Should().NotBe(VKUserId.System);
    }
}
