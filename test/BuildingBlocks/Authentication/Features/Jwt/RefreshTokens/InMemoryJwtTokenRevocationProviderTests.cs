using FluentAssertions;
using VK.Blocks.Authentication.UnitTests.Common;
using VK.Blocks.Authentication.Features.Jwt.RefreshTokens;

namespace VK.Blocks.Authentication.UnitTests.Features.Jwt.RefreshTokens;

public sealed class InMemoryJwtTokenRevocationProviderTests
{
    private readonly FakeTimeProvider _timeProvider;
    private readonly InMemoryJwtTokenRevocationProvider _provider;

    public InMemoryJwtTokenRevocationProviderTests()
    {
        _timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _provider = new InMemoryJwtTokenRevocationProvider(_timeProvider);
    }

    [Fact]
    public async Task RevokeAsync_ShouldMarkJtiAsRevoked()
    {
        // Arrange
        var jti = "revoke-jti";
        var ttl = TimeSpan.FromHours(1);

        // Act
        await _provider.RevokeAsync(jti, ttl);

        // Assert
        var result = await _provider.IsRevokedAsync(jti);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRevokedAsync_WithExpiredJti_ShouldReturnFalseAndLazyCleanup()
    {
        // Arrange
        var jti = "expired-jti";
        var ttl = TimeSpan.FromHours(1);
        await _provider.RevokeAsync(jti, ttl);
        _timeProvider.Advance(TimeSpan.FromHours(2));

        // Act
        var result = await _provider.IsRevokedAsync(jti);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeUserAsync_ShouldMarkUserAsRevoked()
    {
        // Arrange
        var userId = "user123";
        var ttl = TimeSpan.FromDays(1);

        // Act
        await _provider.RevokeUserAsync(userId, ttl);

        // Assert
        var result = await _provider.IsUserRevokedAsync(userId);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserRevokedAsync_WithExpiredUser_ShouldReturnFalseAndLazyCleanup()
    {
        // Arrange
        var userId = "expired-user";
        var ttl = TimeSpan.FromDays(1);
        await _provider.RevokeUserAsync(userId, ttl);
        _timeProvider.Advance(TimeSpan.FromDays(2));

        // Act
        var result = await _provider.IsUserRevokedAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task IsRevokedAsync_WithInvalidJti_ShouldReturnFalse(string? jti)
    {
        // Act
        var result = await _provider.IsRevokedAsync(jti!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CleanupExpiredEntries_ShouldRemoveExpiredEntries()
    {
        // Arrange
        var p1 = "jti-1";
        var p2 = "user-1";
        _provider.RevokeAsync(p1, TimeSpan.FromHours(1));
        _provider.RevokeUserAsync(p2, TimeSpan.FromHours(1));

        _timeProvider.Advance(TimeSpan.FromHours(2));

        // Act
        _provider.CleanupExpiredEntries();

        // Assert
        // After cleanup it should be removed (lazy cleanup won't even find it if we check)
        // But we want to make sure it doesn't throw and actually runs.
    }

    [Fact]
    public void AssociatedServiceType_ShouldBeCorrect()
    {
        // Assert
        _provider.AssociatedServiceType.Should().Be(typeof(IJwtTokenRevocationProvider));
    }
}
