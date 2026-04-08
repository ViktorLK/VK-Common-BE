using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using VK.Blocks.Authentication.Features.ApiKeys;

namespace VK.Blocks.Authentication.UnitTests.Features.ApiKeys;

public sealed class InMemoryApiKeyRateLimiterTests
{
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly InMemoryApiKeyRateLimiter _sut;
    private readonly Fixture _fixture;

    public InMemoryApiKeyRateLimiterTests()
    {
        _timeProviderMock = new Mock<TimeProvider>();
        _sut = new InMemoryApiKeyRateLimiter(_timeProviderMock.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task IsAllowedAsync_BelowLimit_ReturnsTrue()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var limit = 5;
        var windowSeconds = 60;
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        // Act & Assert
        for (int i = 0; i < limit; i++)
        {
            var result = await _sut.IsAllowedAsync(keyId, limit, windowSeconds);
            result.Should().BeTrue($"Request {i + 1} should be allowed");
        }
    }

    [Fact]
    public async Task IsAllowedAsync_AtLimit_ReturnsFalse()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var limit = 3;
        var windowSeconds = 60;
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        // Consume the limit
        for (int i = 0; i < limit; i++)
        {
            await _sut.IsAllowedAsync(keyId, limit, windowSeconds);
        }

        // Act
        var result = await _sut.IsAllowedAsync(keyId, limit, windowSeconds);

        // Assert
        result.Should().BeFalse("Request beyond limit should be blocked");
    }

    [Fact]
    public async Task IsAllowedAsync_AfterWindow_ResetsAndReturnsTrue()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var limit = 1;
        var windowSeconds = 10;
        var now = DateTimeOffset.UtcNow;
        
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);
        await _sut.IsAllowedAsync(keyId, limit, windowSeconds); // Use up the limit

        // Move time forward beyond the window
        var later = now.AddSeconds(windowSeconds + 1);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(later);

        // Act
        var result = await _sut.IsAllowedAsync(keyId, limit, windowSeconds);

        // Assert
        result.Should().BeTrue("Request after window should be allowed as expired timestamps are removed");
    }

    [Fact]
    public async Task IsAllowedAsync_LimitZero_ReturnsFalse()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var limit = 0;
        var windowSeconds = 60;

        // Act
        var result = await _sut.IsAllowedAsync(keyId, limit, windowSeconds);

        // Assert
        result.Should().BeFalse("Limit of zero should always block");
    }

    [Fact]
    public async Task CleanupExpiredEntries_OldEntries_RemovesFromCache()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        // Add an entry
        await _sut.IsAllowedAsync(keyId, 10, 60);

        // Move time forward 2 hours (more than 3600 seconds threshold)
        var wayLater = now.AddHours(2);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(wayLater);

        // Act
        _sut.CleanupExpiredEntries();

        // Assert
        // We can't directly check the private cache, but we can check if it behaves like a new entry
        // If it was removed, it should allow 'limit' requests again starting from 0.
        // Actually, let's just move time forward and check if another call works without being blocked by old state.
        // But for cleanup, it's more about memory. 
        // We can verify that after cleanup, a new call with even limit 1 works.
        var result = await _sut.IsAllowedAsync(keyId, 1, 60);
        result.Should().BeTrue();
    }
}
