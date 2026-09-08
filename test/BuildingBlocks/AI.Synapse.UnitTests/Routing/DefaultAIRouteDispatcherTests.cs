using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse;
using VK.Blocks.AI.Synapse.Cost.Internal;
using VK.Blocks.AI.Synapse.Internal;
using VK.Blocks.AI.Synapse.Routing.Internal;
using VK.Blocks.AI.Synapse.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Routing;

/// <summary>
/// Unit tests for <see cref="DefaultAIRouteDispatcher"/>.
/// Follows AP.01, CS.01, CS.03, and DL.01.
/// </summary>
public sealed class DefaultAIRouteDispatcherTests
{
    private readonly Mock<IVKAIRouter> _routerMock = new();
    private readonly Mock<IVKAIProviderTracker> _trackerMock = new();
    private readonly Mock<IVKAIProviderPool> _providerPoolMock = new();
    private readonly Mock<IVKIdentityContext> _identityContextMock = new();
    private readonly Mock<IVKAICostCalculator> _costCalculatorMock = new();
    private readonly Mock<IVKAITokenBudgetManager> _tokenBudgetManagerMock = new();
    private readonly Mock<IVKAIEngineAccessor> _engineAccessorMock = new();
    private readonly VKRoutingOptions _routingOptions;
    private readonly DefaultAIRouteDispatcher _dispatcher;

    public DefaultAIRouteDispatcherTests()
    {
        _routingOptions = new VKRoutingOptions
        {
            MaxFallbackAttempts = 3,
            RequestTimeout = TimeSpan.FromSeconds(5),
            OverallTimeout = TimeSpan.FromSeconds(10),
            EnableCrossProviderFallback = true
        };

        _identityContextMock.Setup(i => i.TenantId).Returns(new VKTenantId(Guid.NewGuid()));

        _dispatcher = new DefaultAIRouteDispatcher(
            _routerMock.Object,
            _trackerMock.Object,
            _providerPoolMock.Object,
            _identityContextMock.Object,
            _routingOptions,
            _costCalculatorMock.Object,
            _tokenBudgetManagerMock.Object,
            _engineAccessorMock.Object,
            NullLogger<DefaultAIRouteDispatcher>.Instance);
    }

    [Fact]
    public async Task SelectCandidateAsync_WhenPoolEmpty_ReturnsFailure()
    {
        // Arrange
        _providerPoolMock.Setup(p => p.GetAvailablePoolAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKAIConnection>>([]));

        // Act
        var result = await _dispatcher.SelectCandidateAsync(new VKAIRouteArgs(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be(VKAISynapseErrors.NoAvailableProvider.Code);
    }

    [Fact]
    public async Task SelectCandidateAsync_WhenCandidatesFound_ReturnsFirstCandidate()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder().Build();
        _providerPoolMock.Setup(p => p.GetAvailablePoolAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKAIConnection>>([conn]));

        _routerMock.Setup(r => r.ResolveCandidatesAsync(It.IsAny<VKAIRouteArgs>(), It.IsAny<IEnumerable<VKAIConnection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKAIConnection>>([conn]));

        // Act
        var result = await _dispatcher.SelectCandidateAsync(new VKAIRouteArgs(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(conn);
    }

    [Fact]
    public async Task ExecuteWithFallbackAsync_WhenFirstFailsAndSecondSucceeds_FallbacksSuccessfully()
    {
        // Arrange
        var conn1 = new VKAIConnectionBuilder().WithId("conn-1").Build();
        var conn2 = new VKAIConnectionBuilder().WithId("conn-2").Build();

        _providerPoolMock.Setup(p => p.GetAvailablePoolAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKAIConnection>>([conn1, conn2]));

        _routerMock.Setup(r => r.ResolveCandidatesAsync(It.IsAny<VKAIRouteArgs>(), It.IsAny<IEnumerable<VKAIConnection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKAIConnection>>([conn1, conn2]));

        // Act
        var result = await _dispatcher.ExecuteWithFallbackAsync<string>(
            new VKAIRouteArgs(),
            (conn, token) =>
            {
                if (conn.Id == "conn-1")
                {
                    return Task.FromResult(VKResult.Failure<string>(new VKError("AI.Fail", "First failed")));
                }
                return Task.FromResult(VKResult.Success("Success From Conn2"));
            },
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Success From Conn2");

        _trackerMock.Verify(t => t.MarkFailure(conn1, It.IsAny<Exception>()), Times.Once);
        _trackerMock.Verify(t => t.MarkSuccess(conn2), Times.Once);
    }

    [Fact]
    public async Task ExecuteChatWithFallbackAsync_WithValidChatEngine_CallsEngineAndRecordsUsage()
    {
        // Arrange
        var conn = new VKAIConnectionBuilder()
            .WithProvider(VKAIProviderType.OpenAI)
            .WithModelId(VKAIModelIds.OpenAI.Gpt4O)
            .Build();

        _providerPoolMock.Setup(p => p.GetAvailablePoolAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKAIConnection>>([conn]));

        _routerMock.Setup(r => r.ResolveCandidatesAsync(It.IsAny<VKAIRouteArgs>(), It.IsAny<IEnumerable<VKAIConnection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKAIConnection>>([conn]));

        var mockChatEngine = new Mock<IVKChatEngine>();
        var chatResponse = new VKChatResponse
        {
            Message = VKChatMessage.FromText(VKChatRole.Assistant, "Hello from OpenAI"),
            Usage = new VKAITokenUsage
            {
                InputTokens = 100,
                OutputTokens = 50
            }
        };

        mockChatEngine.Setup(e => e.SendAsync(It.IsAny<IEnumerable<VKChatMessage>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(chatResponse));

        _engineAccessorMock.Setup(a => a.GetChatEngine(VKAIProviderType.OpenAI))
            .Returns(mockChatEngine.Object);

        // Act
        var messages = new List<VKChatMessage> { VKChatMessage.FromText(VKChatRole.User, "Hi") };
        var result = await _dispatcher.ExecuteChatWithFallbackAsync(messages, new VKAIRouteArgs(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Content.Should().Be("Hello from OpenAI");

        _costCalculatorMock.Verify(c => c.CalculateCost(
            VKAIProviderType.OpenAI.ToString(),
            VKAIModelIds.OpenAI.Gpt4O,
            100,
            50), Times.Once);

        _tokenBudgetManagerMock.Verify(t => t.RecordUsageAsync(
            It.IsAny<string>(),
            150,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
