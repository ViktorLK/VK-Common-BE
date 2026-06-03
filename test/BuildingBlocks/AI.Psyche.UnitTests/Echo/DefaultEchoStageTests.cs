using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

/// <summary>
/// Unit tests for the <see cref="DefaultEchoStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultEchoStageTests
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_AddsEchoFragmentWithDialogueHistory()
    {
        // Arrange
        var echoStoreMock = new Mock<IVKEchoStore>();
        var tokenCounterMock = new Mock<IVKTokenCounter>();
        var loggerMock = new Mock<ILogger<DefaultEchoStage>>();

        var echoOptionsMock = new Mock<IOptions<VKEchoOptions>>();
        echoOptionsMock.Setup(o => o.Value).Returns(new VKEchoOptions
        {
            IncludeSystemMessages = false,
            PruneUnit = VKEchoPruneUnit.Turn,
            TokenBudgetRatio = 0.5
        });

        var weavingOptionsMock = new Mock<IOptions<VKWeavingOptions>>();
        weavingOptionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions
        {
            TotalContextLimit = 4000
        });

        var history = new List<VKEchoTrace>
        {
            new() { Role = VKChatRole.User, Content = "Hello" },
            new() { Role = VKChatRole.Assistant, Content = "Hi there" }
        };

        echoStoreMock.Setup(s => s.GetHistoryAsync("test-session", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(history));

        tokenCounterMock.Setup(c => c.CountTokens(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(10);

        var stage = new DefaultEchoStage(
            echoStoreMock.Object,
            tokenCounterMock.Object,
            echoOptionsMock.Object,
            weavingOptionsMock.Object,
            loggerMock.Object
        );

        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "how are you?",
            CorrelationId = "test-correlation"
        };

        // Act
        var result = await stage.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var echoFragments = context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).ToList();
        echoFragments.Should().HaveCount(2);

        echoFragments[0].Metadata.Should().BeOfType<VKEchoTrace>();
        ((VKEchoTrace)echoFragments[0].Metadata).Content.Should().Be("Hello");
        ((VKEchoTrace)echoFragments[1].Metadata).Content.Should().Be("Hi there");
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxWindowSizeOverride_RespectsOverride()
    {
        // Arrange
        var echoStoreMock = new Mock<IVKEchoStore>();
        var tokenCounterMock = new Mock<IVKTokenCounter>();
        var loggerMock = new Mock<ILogger<DefaultEchoStage>>();

        var echoOptionsMock = new Mock<IOptions<VKEchoOptions>>();
        echoOptionsMock.Setup(o => o.Value).Returns(new VKEchoOptions
        {
            IncludeSystemMessages = false,
            PruneUnit = VKEchoPruneUnit.Message,
            TokenBudgetRatio = 0.5,
            MaxWindowSize = null
        });

        var weavingOptionsMock = new Mock<IOptions<VKWeavingOptions>>();
        weavingOptionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions
        {
            TotalContextLimit = 4000
        });

        var history = new List<VKEchoTrace>
        {
            new() { Role = VKChatRole.User, Content = "Message 1" },
            new() { Role = VKChatRole.Assistant, Content = "Message 2" },
            new() { Role = VKChatRole.User, Content = "Message 3" }
        };

        echoStoreMock.Setup(s => s.GetHistoryAsync("test-session", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(history));

        tokenCounterMock.Setup(c => c.CountTokens(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(10);

        var stage = new DefaultEchoStage(
            echoStoreMock.Object,
            tokenCounterMock.Object,
            echoOptionsMock.Object,
            weavingOptionsMock.Object,
            loggerMock.Object
        );

        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "how are you?",
            CorrelationId = "test-correlation",
            Echo = new VKEchoArgs { MaxWindowSize = 2 }
        };

        // Act
        var result = await stage.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var echoFragments = context.Fragments.Where(f => f.TierType == VKPromptTierType.Echo).ToList();
        echoFragments.Should().HaveCount(2);
        ((VKEchoTrace)echoFragments[0].Metadata).Content.Should().Be("Message 2");
        ((VKEchoTrace)echoFragments[1].Metadata).Content.Should().Be("Message 3");
    }
}
