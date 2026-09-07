using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Synapse.Internal;
using VK.Blocks.AI.Synapse.UnitTests.Builders;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Quota;

/// <summary>
/// Unit tests for <see cref="DefaultAIProviderTracker"/>.
/// Follows AP.01, CS.01, and DL.01.
/// </summary>
public sealed class DefaultAIProviderTrackerTests
{
    private readonly Mock<IVKAICircuitBreaker> _circuitBreakerMock = new();
    private readonly Mock<IVKAIRateLimiter> _rateLimiterMock = new();
    private readonly Mock<IVKAIMetricsCollector> _metricsCollectorMock = new();
    private readonly DefaultAIProviderTracker _tracker;

    public DefaultAIProviderTrackerTests()
    {
        _tracker = new DefaultAIProviderTracker(
            _circuitBreakerMock.Object,
            _rateLimiterMock.Object,
            _metricsCollectorMock.Object);
    }

    [Fact]
    public void IsAvailable_WhenConnectionIsNull_ReturnsFalse()
    {
        // Act & Assert
        _tracker.IsAvailable(null!).Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_WhenCircuitOpenOrRateLimited_ReturnsFalse()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();

        _circuitBreakerMock.Setup(c => c.IsAllowed(conn)).Returns(true);
        _rateLimiterMock.Setup(r => r.IsAllowed(conn)).Returns(false);

        // Act & Assert
        _tracker.IsAvailable(conn).Should().BeFalse();

        _circuitBreakerMock.Setup(c => c.IsAllowed(conn)).Returns(false);
        _rateLimiterMock.Setup(r => r.IsAllowed(conn)).Returns(true);

        // Act & Assert
        _tracker.IsAvailable(conn).Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_WhenBothCircuitAndRateLimiterAllow_ReturnsTrue()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();
        _circuitBreakerMock.Setup(c => c.IsAllowed(conn)).Returns(true);
        _rateLimiterMock.Setup(r => r.IsAllowed(conn)).Returns(true);

        // Act & Assert
        _tracker.IsAvailable(conn).Should().BeTrue();
    }

    [Fact]
    public void RecordRequest_AcquiresRateLimiter()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();

        // Act
        _tracker.RecordRequest(conn);
        _tracker.RecordRequest(null!);

        // Assert
        _rateLimiterMock.Verify(r => r.Acquire(conn), Times.Once);
    }

    [Fact]
    public void MarkSuccess_ReleasesRateLimiterAndRecordsCircuitSuccess()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();

        // Act
        _tracker.MarkSuccess(conn);
        _tracker.MarkSuccess(null!);

        // Assert
        _rateLimiterMock.Verify(r => r.Release(conn), Times.Once);
        _circuitBreakerMock.Verify(c => c.RecordSuccess(conn), Times.Once);
    }

    [Fact]
    public void MarkFailure_ReleasesRateLimiterAndRecordsCircuitFailure()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();
        var ex = new InvalidOperationException("API Error");

        // Act
        _tracker.MarkFailure(conn, ex);
        _tracker.MarkFailure(null!, ex);

        // Assert
        _rateLimiterMock.Verify(r => r.Release(conn), Times.Once);
        _circuitBreakerMock.Verify(c => c.RecordFailure(conn, ex), Times.Once);
    }

    [Fact]
    public void RecordMetrics_PassesMetricsToCollector()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();
        var latency = TimeSpan.FromMilliseconds(200);

        // Act
        _tracker.RecordMetrics(conn, 1500, latency);
        _tracker.RecordMetrics(null!, 1500, latency);

        // Assert
        _metricsCollectorMock.Verify(m => m.RecordMetrics(conn, 1500, latency), Times.Once);
    }

    [Fact]
    public void GetProvidersOnCooldown_ReturnsCooldownListFromCircuitBreaker()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();
        _circuitBreakerMock.Setup(c => c.GetProvidersOnCooldown()).Returns(new List<VKAIConnection> { conn });

        // Act
        var result = _tracker.GetProvidersOnCooldown();

        // Assert
        result.Should().ContainSingle().Which.Should().Be(conn);
    }
}
