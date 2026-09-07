using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse;
using VK.Blocks.AI.Synapse.Routing.Internal;
using VK.Blocks.AI.Synapse.UnitTests.Builders;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Routing;

/// <summary>
/// Unit tests for <see cref="DefaultAIRouter"/>.
/// Follows AP.01, CS.01, CS.03, and DL.01.
/// </summary>
public sealed class DefaultAIRouterTests
{
    private readonly Mock<IVKAIProviderTracker> _trackerMock = new();
    private readonly Mock<IVKAIMetricsCollector> _metricsCollectorMock = new();

    public DefaultAIRouterTests()
    {
        // Default all connections to available
        _trackerMock.Setup(t => t.IsAvailable(It.IsAny<VKAIConnection>())).Returns(true);
    }

    [Fact]
    public async Task ResolveCandidatesAsync_WhenPoolEmpty_ReturnsFailure()
    {
        // Arrange
        var router = new DefaultAIRouter(_trackerMock.Object, new VKRoutingOptions());

        // Act
        var result = await router.ResolveCandidatesAsync(new VKAIRouteArgs(), [], CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be(VKAISynapseErrors.NoAvailableProvider.Code);
    }

    [Fact]
    public async Task ResolveCandidatesAsync_WhenNoHealthyCandidates_ReturnsFailure()
    {
        // Arrange
        var router = new DefaultAIRouter(_trackerMock.Object, new VKRoutingOptions());
        var conn = new VKAIConnectionBuilder().Build();
        _trackerMock.Setup(t => t.IsAvailable(conn)).Returns(false);

        // Act
        var result = await router.ResolveCandidatesAsync(new VKAIRouteArgs(), [conn], CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be(VKAISynapseErrors.NoAvailableProvider.Code);
    }

    [Fact]
    public async Task ResolveCandidatesAsync_WithPreferenceStrategy_OrdersByPreferredProviderAndModel()
    {
        // Arrange
        var router = new DefaultAIRouter(_trackerMock.Object, new VKRoutingOptions
        {
            Strategy = VKAIRoutingStrategy.Preference
        });

        var connDefault = new VKAIConnectionBuilder()
            .WithId("conn-default")
            .WithDefault(true)
            .WithProvider(VKAIProviderType.Anthropic)
            .Build();

        var connTarget = new VKAIConnectionBuilder()
            .WithId("conn-target")
            .WithDefault(false)
            .WithProvider(VKAIProviderType.OpenAI)
            .WithModelId(VKAIModelIds.OpenAI.Gpt4O)
            .Build();

        var args = new VKAIRouteArgs
        {
            PreferredProvider = VKAIProviderType.OpenAI,
            PreferredModelId = VKAIModelIds.OpenAI.Gpt4O
        };

        // Act
        var result = await router.ResolveCandidatesAsync(args, [connDefault, connTarget], CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Id.Should().Be("conn-target"); // Preferred provider (100) + model (50) > default (10)
    }

    [Fact]
    public async Task ResolveCandidatesAsync_WithCostOptimizedStrategy_RanksCheaperModelsFirst()
    {
        // Arrange
        var router = new DefaultAIRouter(_trackerMock.Object, new VKRoutingOptions
        {
            Strategy = VKAIRoutingStrategy.CostOptimized
        });

        var expensiveConn = new VKAIConnectionBuilder()
            .WithId("expensive")
            .WithModelId(VKAIModelIds.OpenAI.Gpt4O)
            .Build();

        var cheapConn = new VKAIConnectionBuilder()
            .WithId("cheap")
            .WithModelId(VKAIModelIds.OpenAI.Gpt4OMini)
            .Build();

        // Act
        var result = await router.ResolveCandidatesAsync(new VKAIRouteArgs(), [expensiveConn, cheapConn], CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Id.Should().Be("cheap");
        result.Value[1].Id.Should().Be("expensive");
    }

    [Fact]
    public async Task ResolveCandidatesAsync_WithWeightedStrategy_RanksHigherConcurrencyFirst()
    {
        // Arrange
        var router = new DefaultAIRouter(_trackerMock.Object, new VKRoutingOptions
        {
            Strategy = VKAIRoutingStrategy.WeightedRoundRobin
        });

        var connLow = new VKAIConnectionBuilder().WithId("low").WithMaxConcurrency(5).Build();
        var connHigh = new VKAIConnectionBuilder().WithId("high").WithMaxConcurrency(25).Build();

        // Act
        var result = await router.ResolveCandidatesAsync(new VKAIRouteArgs(), [connLow, connHigh], CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Id.Should().Be("high");
        result.Value[1].Id.Should().Be("low");
    }

    [Fact]
    public async Task ResolveCandidatesAsync_WithLatencyOptimizedStrategy_RanksLowerLatencyFirst()
    {
        // Arrange
        var router = new DefaultAIRouter(
            _trackerMock.Object,
            new VKRoutingOptions { Strategy = VKAIRoutingStrategy.LatencyOptimized },
            _metricsCollectorMock.Object);

        var connFast = new VKAIConnectionBuilder().WithId("fast").Build();
        var connSlow = new VKAIConnectionBuilder().WithId("slow").Build();

        _metricsCollectorMock.Setup(m => m.GetAverageLatencyMs(connFast)).Returns(80);
        _metricsCollectorMock.Setup(m => m.GetAverageLatencyMs(connSlow)).Returns(450);

        // Act
        var result = await router.ResolveCandidatesAsync(new VKAIRouteArgs(), [connSlow, connFast], CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Id.Should().Be("fast");
        result.Value[1].Id.Should().Be("slow");
    }
}
