using System;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Synapse;
using VK.Blocks.AI.Synapse.Internal;
using VK.Blocks.AI.Synapse.UnitTests.Builders;
using VK.Blocks.Resilience;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Quota;

/// <summary>
/// Unit tests for <see cref="LocalAIRateLimiter"/>.
/// Follows AP.01, CS.01, and DL.01.
/// </summary>
public sealed class LocalAIRateLimiterTests
{
    private readonly Mock<IVKBulkhead> _bulkheadMock = new();
    private readonly Mock<IVKRateLimiter> _rateLimiterMock = new();
    private readonly VKQuotaOptions _options;
    private readonly LocalAIRateLimiter _aiRateLimiter;

    public LocalAIRateLimiterTests()
    {
        _options = new VKQuotaOptions
        {
            DefaultMaxConcurrency = 8,
            DefaultRequestsPerMinute = 120
        };
        _aiRateLimiter = new LocalAIRateLimiter(
            _bulkheadMock.Object,
            _rateLimiterMock.Object,
            _options);
    }

    [Fact]
    public void IsAllowed_WhenConnectionNull_ReturnsFalse()
    {
        // Act & Assert
        _aiRateLimiter.IsAllowed(null!).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_WhenBulkheadDisallows_ReturnsFalse()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().WithMaxConcurrency(5).Build();
        var key = $"{conn.TenantId}_{conn.Id}";

        _bulkheadMock.Setup(b => b.IsAllowed(key, 5)).Returns(false);

        // Act
        var allowed = _aiRateLimiter.IsAllowed(conn);

        // Assert
        allowed.Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_WhenRateLimiterDisallows_ReturnsFalse()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().WithMaxConcurrency(0).Build(); // Concurrency falls back to 8
        var key = $"{conn.TenantId}_{conn.Id}";

        _bulkheadMock.Setup(b => b.IsAllowed(key, 8)).Returns(true);
        _rateLimiterMock.Setup(r => r.IsAllowed(key, 120, TimeSpan.FromMinutes(1))).Returns(false);

        // Act
        var allowed = _aiRateLimiter.IsAllowed(conn);

        // Assert
        allowed.Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_WhenBothBulkheadAndRpmAllow_ReturnsTrue()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().WithMaxConcurrency(20).Build();
        var key = $"{conn.TenantId}_{conn.Id}";

        _bulkheadMock.Setup(b => b.IsAllowed(key, 20)).Returns(true);
        _rateLimiterMock.Setup(r => r.IsAllowed(key, 120, TimeSpan.FromMinutes(1))).Returns(true);

        // Act
        var allowed = _aiRateLimiter.IsAllowed(conn);

        // Assert
        allowed.Should().BeTrue();
    }

    [Fact]
    public void Acquire_AcquiresBulkheadAndRecordsRequest()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();
        var key = $"{conn.TenantId}_{conn.Id}";

        // Act
        _aiRateLimiter.Acquire(conn);
        _aiRateLimiter.Acquire(null!);

        // Assert
        _bulkheadMock.Verify(b => b.Acquire(key), Times.Once);
        _rateLimiterMock.Verify(r => r.RecordRequest(key), Times.Once);
    }

    [Fact]
    public void Release_ReleasesBulkhead()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();
        var key = $"{conn.TenantId}_{conn.Id}";

        // Act
        _aiRateLimiter.Release(conn);
        _aiRateLimiter.Release(null!);

        // Assert
        _bulkheadMock.Verify(b => b.Release(key), Times.Once);
    }
}
