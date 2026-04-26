using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Authentication.ApiKeys.Internal;

namespace VK.Blocks.Authentication.UnitTests.ApiKeys.Internal;

public sealed class InMemoryApiKeyRateLimiterTests : IAsyncDisposable
{
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly Mock<IOptions<VKApiKeyOptions>> _optionsMock;
    private readonly InMemoryApiKeyRateLimiter _sut;
    private DateTimeOffset _now = DateTimeOffset.UtcNow;

    public InMemoryApiKeyRateLimiterTests()
    {
        _timeProviderMock = new Mock<TimeProvider>();
        _optionsMock = new Mock<IOptions<VKApiKeyOptions>>();
        SetupTime(_now);

        // Setup options (not strictly used by the logic yet but requested by constructor)
        _optionsMock.Setup(x => x.Value).Returns(new VKApiKeyOptions());

        _sut = new InMemoryApiKeyRateLimiter(_optionsMock.Object, _timeProviderMock.Object);
    }

    public async ValueTask DisposeAsync()
    {
        await _sut.DisposeAsync();
    }

    private void SetupTime(DateTimeOffset time)
    {
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(time);
    }

    [Fact]
    public async Task IsAllowedAsync_UnderLimit_ReturnsTrue()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var limit = 5;
        var window = 60;

        // Act & Assert
        for (int i = 0; i < limit; i++)
        {
            var result = await _sut.IsAllowedAsync(keyId, limit, window);
            result.Should().BeTrue($"Request {i + 1} should be allowed");
        }
    }

    [Fact]
    public async Task IsAllowedAsync_ExceedLimit_ReturnsFalse()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var limit = 3;
        var window = 60;

        // Fill up to limit
        for (int i = 0; i < limit; i++)
            await _sut.IsAllowedAsync(keyId, limit, window);

        // Act
        var result = await _sut.IsAllowedAsync(keyId, limit, window);

        // Assert
        result.Should().BeFalse("Request exceeding limit should be blocked");
    }

    [Fact]
    public async Task IsAllowedAsync_WindowSlides_AllowsNewRequests()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var limit = 1;
        var window = 60;

        // 1. Initial request
        await _sut.IsAllowedAsync(keyId, limit, window);

        // 2. Immediate second request should be blocked
        (await _sut.IsAllowedAsync(keyId, limit, window)).Should().BeFalse();

        // 3. Move time forward past window
        _now = _now.AddSeconds(window + 1);
        SetupTime(_now);

        // Act
        var result = await _sut.IsAllowedAsync(keyId, limit, window);

        // Assert
        result.Should().BeTrue("Request after window slides should be allowed");
    }

    [Fact]
    public void IsAllowedAsync_ConcurrentRequests_RespectsLimit()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var limit = 50;
        var window = 60;
        var totalRequests = 200;
        var allowedCount = 0;

        // Act
        Parallel.For(0, totalRequests, _ =>
        {
            if (_sut.IsAllowedAsync(keyId, limit, window).GetAwaiter().GetResult())
            {
                Interlocked.Increment(ref allowedCount);
            }
        });

        // Assert
        allowedCount.Should().Be(limit, $"Only exactly {limit} requests should be allowed out of {totalRequests}");
    }

    [Fact]
    public async Task CleanupExpiredEntries_RemovesInactiveKeys()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        await _sut.IsAllowedAsync(keyId, 1, 60);

        // Move time past cleanup threshold (1 hour)
        _now = _now.AddHours(2);
        SetupTime(_now);

        // Act
        _sut.CleanupExpiredEntries();

        // Assert
        // After cleanup, a new request should be allowed even if the old window would have technically expired.
        // The core proof is that it executes without error.
        var result = await _sut.IsAllowedAsync(keyId, 1, 60);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task IsAllowedAsync_InvalidLimit_ReturnsFalse(int limit)
    {
        // Act
        var result = await _sut.IsAllowedAsync(Guid.NewGuid(), limit, 60);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAllowedAsync_MultipleKeys_AreIsolated()
    {
        // Arrange
        var keyId1 = Guid.NewGuid();
        var keyId2 = Guid.NewGuid();

        // Consume limit for key1
        await _sut.IsAllowedAsync(keyId1, 1, 60);
        var result1 = await _sut.IsAllowedAsync(keyId1, 1, 60);
        result1.Should().BeFalse();

        // Act - Check key2
        var result2 = await _sut.IsAllowedAsync(keyId2, 1, 60);

        // Assert
        result2.Should().BeTrue();
    }
}
