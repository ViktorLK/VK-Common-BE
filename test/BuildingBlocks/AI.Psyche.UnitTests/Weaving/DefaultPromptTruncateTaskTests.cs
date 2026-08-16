using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

/// <summary>
/// Unit tests for the <see cref="DefaultPromptTruncateTask"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPromptTruncateTaskTests
{
    private static (VKPsycheContext Context, IServiceProvider Services) CreateTestContext(
        string userInput = "hello")
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var request = new VKPsycheRequest
        {
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            UserInput = userInput
        };

        var context = new VKPsycheContext
        {
            Request = request,
            Services = services
        };

        return (context, services);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHistoryExceedsBudget_TruncatesHistory()
    {
        // Arrange
        var tokenCounterMock = new Mock<IVKTokenCounter>();
        tokenCounterMock.Setup(c => c.CountTokens(It.IsAny<string>(), It.IsAny<string>())).Returns(30);
        var options = new VKWeavingOptions
        {
            TotalContextLimit = 100,
            MaxResponseTokens = 20,
            AvailableHistoryLimit = 50
        };

        // Mock token counter: each history segment has 30 tokens.
        tokenCounterMock.Setup(c => c.CountTokens(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(30);

        var loggerMock = new Mock<ILogger<DefaultPromptTruncateTask>>();
        var task = new DefaultPromptTruncateTask(tokenCounterMock.Object, options, loggerMock.Object);

        var (context, _) = CreateTestContext();
        var mockMetadata = new Mock<IVKFragmentMetadata>().Object;

        var f1 = new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            RenderOrder = 2,
            Segment = new VKPromptSegment { Role = VKChatRole.User, Content = "Recent Msg" },
            Metadata = mockMetadata
        };
        var f2 = new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            RenderOrder = 1,
            Segment = new VKPromptSegment { Role = VKChatRole.Assistant, Content = "Middle Msg" },
            Metadata = mockMetadata
        };
        var f3 = new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            RenderOrder = 0,
            Segment = new VKPromptSegment { Role = VKChatRole.User, Content = "Oldest Msg" },
            Metadata = mockMetadata
        };

        context.AddFragment(f1);
        context.AddFragment(f2);
        context.AddFragment(f3);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Allowed budget is Math.Min(100 - 20 - 0, 50) = 50 tokens.
        // We can only fit one 30-token history fragment (f1).
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Echo);
        context.Fragments.First().Segment.Content.Should().Be("Recent Msg");
    }
}
