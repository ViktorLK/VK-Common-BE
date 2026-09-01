using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.AI.Psyche.Weaving.Internal;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

/// <summary>
/// Unit tests for the <see cref="DefaultPromptTruncateTask"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPromptTruncateTaskTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WhenHistoryExceedsBudget_TruncatesHistory()
    {
        // Arrange
        GetMock<IVKTokenCounter>()
            .Setup(c => c.CountTokens(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(30);

        GetMock<IVKModelCatalog>()
            .Setup(m => m.GetModelMetadata(It.IsAny<string>()))
            .Returns(new VKModelMetadata { ModelId = "test-model", MaxOutputTokens = 20, ContextWindowSize = 50 });

        var options = new VKWeavingOptions
        {
            MaxContextBudget = 50
        };

        var task = new DefaultPromptTruncateTask(
            GetMockObject<IVKTokenCounter>(),
            GetMockObject<IVKModelCatalog>(),
            options,
            GetMockObject<ILogger<DefaultPromptTruncateTask>>());

        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        var mockMetadata = GetMockObject<IVKFragmentMetadata>();

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
        result.Should().BeSuccess();

        // Allowed budget is Math.Min(100 - 20 - 0, 50) = 50 tokens.
        // We can only fit one 30-token history fragment (f1).
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Echo);
        context.Fragments.First().Segment.Content.Should().Be("Recent Msg");
    }
}
