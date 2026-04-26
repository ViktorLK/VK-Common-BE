using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.Authentication.ApiKeys.Internal;

namespace VK.Blocks.Authentication.UnitTests.ApiKeys.Internal;

public sealed class InMemoryApiKeyRevocationProviderTests
{
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly InMemoryApiKeyRevocationProvider _sut;

    public InMemoryApiKeyRevocationProviderTests()
    {
        _timeProviderMock = new Mock<TimeProvider>();
        _sut = new InMemoryApiKeyRevocationProvider(_timeProviderMock.Object);
    }

    [Fact]
    public async Task IsRevokedAsync_NotRevoked_ReturnsFalse()
    {
        // Arrange
        var keyId = Guid.NewGuid().ToString();
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        // Act
        var result = await _sut.IsRevokedAsync(keyId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeAsync_ValidKey_MarksAsRevoked()
    {
        // Arrange
        var keyId = Guid.NewGuid().ToString();
        var ttl = TimeSpan.FromMinutes(5);
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        // Act
        await _sut.RevokeAsync(keyId, ttl);
        var result = await _sut.IsRevokedAsync(keyId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRevokedAsync_ExpiredRevocation_ReturnsFalseAndRemoves()
    {
        // Arrange
        var keyId = Guid.NewGuid().ToString();
        var ttl = TimeSpan.FromMinutes(5);
        var now = DateTimeOffset.UtcNow;

        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);
        await _sut.RevokeAsync(keyId, ttl);

        // Move time past expiration
        var later = now.AddMinutes(6);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(later);

        // Act
        var result = await _sut.IsRevokedAsync(keyId);

        // Assert
        result.Should().BeFalse("Expired revocation should not be active");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task IsRevokedAsync_NullOrEmpty_ReturnsFalse(string? keyId)
    {
        // Act
        var result = await _sut.IsRevokedAsync(keyId!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CleanupExpiredEntries_RemovesExpiredKeys()
    {
        // Arrange
        var keyId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        // Revoke with 1 minute TTL
        await _sut.RevokeAsync(keyId, TimeSpan.FromMinutes(1));

        // Move time past expiration
        var later = now.AddMinutes(2);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(later);

        // Act
        _sut.CleanupExpiredEntries();

        // Assert
        var result = await _sut.IsRevokedAsync(keyId);
        result.Should().BeFalse();
    }
}
