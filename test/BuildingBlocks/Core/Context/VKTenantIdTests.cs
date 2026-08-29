using VK.Blocks.Core;

namespace VK.Blocks.Core.UnitTests.Context;

public sealed class VKTenantIdTests
{
    [Fact]
    public void Default_ShouldReturnExpectedSentinelGuid()
    {
        // Arrange
        var expectedGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var defaultTenant = VKTenantId.Default;

        // Assert
        defaultTenant.Value.Should().Be(expectedGuid);
        defaultTenant.Should().NotBe(default(VKTenantId));
    }

    [Fact]
    public void FromNullable_ValidGuidString_ReturnsMatchingTenantId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = VKTenantId.FromNullable(guid.ToString());

        // Assert
        result.Value.Should().Be(guid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-guid")]
    public void FromNullable_InvalidOrEmptyInput_ReturnsDefaultTenantId(string? input)
    {
        // Act
        var result = VKTenantId.FromNullable(input);

        // Assert
        result.Should().Be(VKTenantId.Default);
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new VKTenantId(guid);
        var id2 = new VKTenantId(guid);

        // Act & Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }
}
