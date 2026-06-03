using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

/// <summary>
/// Unit tests for the <see cref="DefaultKnowledgeStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultKnowledgeStageTests
{
    [Fact]
    public async Task ExecuteAsync_WhenKeywordMatches_AddsKnowledgeFragment()
    {
        // Arrange
        var storeMock = new Mock<IVKKnowledgeStore>();
        var optionsMock = new Mock<IOptions<VKKnowledgeOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKKnowledgeOptions { Enabled = true });

        var weavingOptionsMock = new Mock<IOptions<VKWeavingOptions>>();
        weavingOptionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions());

        var entry = new VKKnowledgeEntry
        {
            Id = "k1",
            Content = "Apples are delicious fruits.",
            IsEnabled = true,
            Keys = new List<VKKnowledgeKey>
            {
                new() { Text = "apple", MatchType = VKKnowledgeMatchType.Contains, CaseSensitive = false }
            }
        };

        IEnumerable<VKKnowledgeEntry> entries = new[] { entry };
        storeMock.Setup(s => s.GetRelevantEntriesAsync("test-persona", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(entries));

        var stage = new DefaultKnowledgeStage(optionsMock.Object, storeMock.Object, weavingOptionsMock.Object);

        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "I really like to eat an apple every day!",
            CorrelationId = "test-correlation"
        };

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge);

        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge).Subject;
        fragment.Metadata.Should().BeOfType<VKKnowledgeEntry>();

        var parsedEntry = (VKKnowledgeEntry)fragment.Metadata;
        parsedEntry.Id.Should().Be("k1");
    }

    [Fact]
    public async Task ExecuteAsync_WithUserBasedTurns_CountsDialogueRoundsCorrectly()
    {
        // Arrange
        var storeMock = new Mock<IVKKnowledgeStore>();
        var optionsMock = new Mock<IOptions<VKKnowledgeOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKKnowledgeOptions { Enabled = true });
        var weavingOptionsMock = new Mock<IOptions<VKWeavingOptions>>();
        weavingOptionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions());

        var entry = new VKKnowledgeEntry
        {
            Id = "k_cooldown",
            Content = "High-importance facts.",
            IsEnabled = true,
            CooldownTurns = 2, // Cooldown is 2 User-turns
            Keys = new List<VKKnowledgeKey> { new() { Text = "trigger", MatchType = VKKnowledgeMatchType.Contains } }
        };

        storeMock.Setup(s => s.GetRelevantEntriesAsync("test-persona", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IEnumerable<VKKnowledgeEntry>>([entry]));

        var stage = new DefaultKnowledgeStage(optionsMock.Object, storeMock.Object, weavingOptionsMock.Object);

        // We simulate a historical timeline containing a previous match, then assistant reply, then user again.
        // Index 0: User: "trigger keyword" -> triggers k_cooldown (Turn 0)
        // Index 1: Assistant: "assistant response with trigger" -> inside turn 0
        // Index 2: User: "new trigger" -> (Turn 1). Since cooldown is 2 User-turns, it should be in cooldown (Turn 1 - Turn 0 < 2) and NOT trigger!
        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "new trigger",
            CorrelationId = "test-correlation"
        };

        // Add historical echoes
        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            Role = VKChatRole.User,
            RenderOrder = 100,
            Metadata = new VKEchoTrace { Role = VKChatRole.User, Content = "trigger keyword" }
        });
        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            Role = VKChatRole.Assistant,
            RenderOrder = 101,
            Metadata = new VKEchoTrace { Role = VKChatRole.Assistant, Content = "assistant response with trigger" }
        });

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Should NOT trigger because it is in cooldown based on User-turns (Turn 1 is within CooldownTurns=2 from Turn 0)
        context.Fragments.Should().NotContain(f => f.TierType == VKPromptTierType.Knowledge);
    }

    [Fact]
    public async Task ExecuteAsync_WithExclusiveGroup_PrunesLowerWeightEntries()
    {
        // Arrange
        var storeMock = new Mock<IVKKnowledgeStore>();
        var optionsMock = new Mock<IOptions<VKKnowledgeOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKKnowledgeOptions { Enabled = true });
        var weavingOptionsMock = new Mock<IOptions<VKWeavingOptions>>();
        weavingOptionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions());

        var entry1 = new VKKnowledgeEntry
        {
            Id = "k_low",
            Content = "Low weight fact.",
            IsEnabled = true,
            ExclusiveGrouping = new VKExclusiveGrouping("group_A", 10),
            Keys = new List<VKKnowledgeKey> { new() { Text = "test", MatchType = VKKnowledgeMatchType.Contains } }
        };

        var entry2 = new VKKnowledgeEntry
        {
            Id = "k_high",
            Content = "High weight fact.",
            IsEnabled = true,
            ExclusiveGrouping = new VKExclusiveGrouping("group_A", 100),
            Keys = new List<VKKnowledgeKey> { new() { Text = "test", MatchType = VKKnowledgeMatchType.Contains } }
        };

        storeMock.Setup(s => s.GetRelevantEntriesAsync("test-persona", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IEnumerable<VKKnowledgeEntry>>([entry1, entry2]));

        var stage = new DefaultKnowledgeStage(optionsMock.Object, storeMock.Object, weavingOptionsMock.Object);

        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "this is a test sentence",
            CorrelationId = "test-correlation"
        };

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Should only contain the high-weight entry in the exclusive group
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge);
        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge).Subject;
        var entry = (VKKnowledgeEntry)fragment.Metadata;
        entry.Id.Should().Be("k_high");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAbsolutePosition_PreservesCustomTagAndSetsDepth()
    {
        // Arrange
        var storeMock = new Mock<IVKKnowledgeStore>();
        var optionsMock = new Mock<IOptions<VKKnowledgeOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKKnowledgeOptions { Enabled = true });
        var weavingOptionsMock = new Mock<IOptions<VKWeavingOptions>>();
        weavingOptionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions());

        var entry = new VKKnowledgeEntry
        {
            Id = "k_pinned",
            Content = "Pinned lore.",
            Tag = "lore",
            IsEnabled = true,
            TriggerType = VKKnowledgeTriggerType.Constant,
            Position = new VKKnowledgeAbsolutePosition(VKChatRole.User, 2)
        };

        storeMock.Setup(s => s.GetRelevantEntriesAsync("test-persona", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IEnumerable<VKKnowledgeEntry>>([entry]));

        var stage = new DefaultKnowledgeStage(optionsMock.Object, storeMock.Object, weavingOptionsMock.Object);
        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            CorrelationId = "test-correlation"
        };

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge).Subject;
        fragment.Depth.Should().Be(2);
        fragment.Role.Should().Be(VKChatRole.User);

        var wovenEntry = (VKKnowledgeEntry)fragment.Metadata!;
        wovenEntry.Tag.Should().Be("lore");
    }
}
