using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.AI.Psyche.Weaving.Internal;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

/// <summary>
/// Unit tests for the <see cref="DefaultPromptTruncateTask"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPromptTruncateTaskTests
{
    [Fact]
    public async Task ExecuteAsync_WhenHistoryExceedsBudget_TruncatesAndAddsToEvicted()
    {
        // Arrange
        var tokenCounterMock = new Mock<IVKTokenCounter>();
        var optionsMock = new Mock<IOptions<VKWeavingOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions
        {
            TotalContextLimit = 100,
            MaxResponseTokens = 20,
            AvailableHistoryLimit = 50
        });

        // Mock token counter: each history segment has 30 tokens.
        tokenCounterMock.Setup(c => c.CountTokens(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(30);

        var loggerMock = new Mock<ILogger<DefaultPromptTruncateTask>>();
        var task = new DefaultPromptTruncateTask(tokenCounterMock.Object, optionsMock.Object, loggerMock.Object);

        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "hello",
            CorrelationId = "test-correlation"
        };

        var mockMetadata = new Mock<IVKFragmentMetadata>().Object;
        var f1 = new VKPromptFragment { TierType = VKPromptTierType.Echo, Role = VKChatRole.User, Depth = 0, RenderOrder = 0, Content = "Recent Msg", Metadata = mockMetadata };
        var f2 = new VKPromptFragment { TierType = VKPromptTierType.Echo, Role = VKChatRole.Assistant, Depth = 1, RenderOrder = 0, Content = "Middle Msg", Metadata = mockMetadata };
        var f3 = new VKPromptFragment { TierType = VKPromptTierType.Echo, Role = VKChatRole.User, Depth = 2, RenderOrder = 0, Content = "Oldest Msg", Metadata = mockMetadata };

        context.AddFragment(f1);
        context.AddFragment(f2);
        context.AddFragment(f3);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Allowed budget is Math.Min(100 - 20 - 0, 50) = 50 tokens.
        // We can only fit one 30-token history fragment (f1, since it has Depth = 0, the lowest depth/most recent).
        // The other 2 history fragments (f2, f3) must be evicted.
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Echo);
        context.Fragments.First().Depth.Should().Be(0);

        context.Evicted.Should().HaveCount(2);
        context.Evicted.Select(e => e.Depth).Should().BeEquivalentTo([1, 2]);
    }
}
