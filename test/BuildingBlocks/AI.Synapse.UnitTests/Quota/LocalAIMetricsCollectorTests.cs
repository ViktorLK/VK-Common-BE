using System;
using FluentAssertions;
using VK.Blocks.AI.Synapse.Internal;
using VK.Blocks.AI.Synapse.UnitTests.Builders;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Quota;

/// <summary>
/// Unit tests for <see cref="LocalAIMetricsCollector"/>.
/// Follows AP.01, CS.01, CS.06, and DL.01.
/// </summary>
public sealed class LocalAIMetricsCollectorTests
{
    [Fact]
    public void GetAverageLatencyMs_WhenNoMetricsRecorded_ReturnsZero()
    {
        // Arrange
        var collector = new LocalAIMetricsCollector();
        var conn = new VKAIConnectionBuilder().Build();

        // Act
        var avg = collector.GetAverageLatencyMs(conn);

        // Assert
        avg.Should().Be(0);
        collector.GetAverageLatencyMs(null!).Should().Be(0);
    }

    [Fact]
    public void RecordMetrics_CalculatesExponentialMovingAverageLatency()
    {
        // Arrange
        var fakeTime = new ManualTestTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var collector = new LocalAIMetricsCollector(fakeTime);
        var conn = new VKAIConnectionBuilder().Build();

        // First call sets baseline
        collector.RecordMetrics(conn, 100, TimeSpan.FromMilliseconds(100));
        collector.GetAverageLatencyMs(conn).Should().Be(100);

        // Second call applies EMA: (100 * 0.8) + (200 * 0.2) = 80 + 40 = 120
        collector.RecordMetrics(conn, 200, TimeSpan.FromMilliseconds(200));
        collector.GetAverageLatencyMs(conn).Should().BeApproximately(120, 0.001);
    }

    [Fact]
    public void RecordMetrics_WhenConnectionNull_DoesNotThrow()
    {
        // Arrange
        var collector = new LocalAIMetricsCollector();

        // Act
        var act = () => collector.RecordMetrics(null!, 50, TimeSpan.FromMilliseconds(50));

        // Assert
        act.Should().NotThrow();
    }
}
