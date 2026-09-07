using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Synapse;
using VK.Blocks.AI.Synapse.Internal;
using VK.Blocks.AI.Synapse.UnitTests.Builders;
using VK.Blocks.Resilience;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Quota;

/// <summary>
/// Unit tests for <see cref="LocalAICircuitBreaker"/>.
/// Follows AP.01, CS.01, and DL.01.
/// </summary>
public sealed class LocalAICircuitBreakerTests
{
    private readonly Mock<IVKCircuitBreaker> _circuitBreakerMock = new();
    private readonly VKQuotaOptions _options;
    private readonly LocalAICircuitBreaker _aiCircuitBreaker;

    public LocalAICircuitBreakerTests()
    {
        _options = new VKQuotaOptions
        {
            DefaultCircuitBreakerThreshold = 5,
            DefaultCooldownDuration = TimeSpan.FromSeconds(45)
        };
        _aiCircuitBreaker = new LocalAICircuitBreaker(_circuitBreakerMock.Object, _options);
    }

    [Fact]
    public void IsAllowed_WhenConnectionNull_ReturnsFalse()
    {
        // Act & Assert
        _aiCircuitBreaker.IsAllowed(null!).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_DelegatesToUnderlyingCircuitBreaker()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().WithId("conn-1").Build();
        var key = $"{conn.TenantId}_{conn.Id}";

        _circuitBreakerMock.Setup(c => c.IsAllowed(key)).Returns(true);

        // Act
        var allowed = _aiCircuitBreaker.IsAllowed(conn);

        // Assert
        allowed.Should().BeTrue();
        _circuitBreakerMock.Verify(c => c.IsAllowed(key), Times.Once);
    }

    [Fact]
    public void RecordSuccess_DelegatesWithConfiguredThreshold()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().WithId("conn-2").Build();
        var key = $"{conn.TenantId}_{conn.Id}";

        // Act
        _aiCircuitBreaker.RecordSuccess(conn);
        _aiCircuitBreaker.RecordSuccess(null!);

        // Assert
        _circuitBreakerMock.Verify(c => c.RecordSuccess(key, 5), Times.Once);
    }

    [Fact]
    public void RecordFailure_DelegatesWithConfiguredCooldownAndThreshold()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().WithId("conn-3").Build();
        var key = $"{conn.TenantId}_{conn.Id}";
        var ex = new TimeoutException("Gateway timeout");

        // Act
        _aiCircuitBreaker.RecordFailure(conn, ex);
        _aiCircuitBreaker.RecordFailure(null!, ex);

        // Assert
        _circuitBreakerMock.Verify(c => c.RecordFailure(
            key,
            ex,
            TimeSpan.FromSeconds(45),
            5,
            0.5), Times.Once);
    }

    [Fact]
    public void GetProvidersOnCooldown_MatchesKnownConnectionsByKey()
    {
        // Arrange
        var conn1 = new VKAIConnectionBuilder().WithId("conn-open-1").Build();
        var conn2 = new VKAIConnectionBuilder().WithId("conn-open-2").Build();
        var key1 = $"{conn1.TenantId}_{conn1.Id}";
        var key2 = $"{conn2.TenantId}_{conn2.Id}";

        // Touch to register in _knownConnections
        _aiCircuitBreaker.IsAllowed(conn1);
        _aiCircuitBreaker.IsAllowed(conn2);

        _circuitBreakerMock.Setup(c => c.GetKeysOnCooldown()).Returns(new List<string> { key1 });

        // Act
        var cooldownList = _aiCircuitBreaker.GetProvidersOnCooldown();

        // Assert
        cooldownList.Should().ContainSingle().Which.Should().Be(conn1);
    }
}
