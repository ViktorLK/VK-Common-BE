using Moq;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.Jwt.Internal;
using VK.Blocks.Authentication.Features.Jwt.Persistence;

namespace VK.Blocks.Authentication.UnitTests.Features.Jwt.Internal;

public sealed class JwtTokenRevocationServiceTests
{
    private readonly Mock<IJwtTokenRevocationProvider> _revocationProviderMock;
    private readonly JwtTokenRevocationService _service;

    public JwtTokenRevocationServiceTests()
    {
        _revocationProviderMock = new Mock<IJwtTokenRevocationProvider>();
        _service = new JwtTokenRevocationService(_revocationProviderMock.Object);
    }

    [Fact]
    public async Task RevokeUserTokensAsync_WithJti_ShouldCallRevokeAsync()
    {
        // Arrange
        var userId = "user123";
        var jti = "token-jti";
        var ttl = TimeSpan.FromHours(2);
        var ct = CancellationToken.None;

        // Act
        await _service.RevokeUserTokensAsync(userId, jti, ttl, ct);

        // Assert
        _revocationProviderMock.Verify(x => x.RevokeAsync(jti, ttl, ct), Times.Once);
    }

    [Fact]
    public async Task RevokeUserTokensAsync_WithNullJti_ShouldNotCallRevokeAsync()
    {
        // Arrange
        var userId = "user123";
        string? jti = null;

        // Act
        await _service.RevokeUserTokensAsync(userId, jti!);

        // Assert
        _revocationProviderMock.Verify(x => x.RevokeAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeUserTokensAsync_WithDefaultTtl_ShouldUseOneDay()
    {
        // Arrange
        var userId = "user123";
        var jti = "token-jti";
        var expectedTtl = TimeSpan.FromDays(1);

        // Act
        await _service.RevokeUserTokensAsync(userId, jti);

        // Assert
        _revocationProviderMock.Verify(x => x.RevokeAsync(jti, expectedTtl, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithUserId_ShouldCallRevokeUserAsync()
    {
        // Arrange
        var userId = "user123";
        var ttl = TimeSpan.FromDays(2);
        var ct = CancellationToken.None;

        // Act
        await _service.RevokeAllUserTokensAsync(userId, ttl, ct);

        // Assert
        _revocationProviderMock.Verify(x => x.RevokeUserAsync(userId, ttl, ct), Times.Once);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithNullUserId_ShouldNotCallRevokeUserAsync()
    {
        // Arrange
        string? userId = null;

        // Act
        await _service.RevokeAllUserTokensAsync(userId!);

        // Assert
        _revocationProviderMock.Verify(x => x.RevokeUserAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithDefaultTtl_ShouldUseSevenDays()
    {
        // Arrange
        var userId = "user123";
        var expectedTtl = TimeSpan.FromDays(7);

        // Act
        await _service.RevokeAllUserTokensAsync(userId);

        // Assert
        _revocationProviderMock.Verify(x => x.RevokeUserAsync(userId, expectedTtl, It.IsAny<CancellationToken>()), Times.Once);
    }
}
