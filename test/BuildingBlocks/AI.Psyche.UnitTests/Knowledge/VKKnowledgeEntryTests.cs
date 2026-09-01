using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

/// <summary>
/// Unit tests for <see cref="VKKnowledgeEntry"/> aggregate root.
/// Follows AP.01, CS.01, and DL.01 rules.
/// </summary>
public sealed class VKKnowledgeEntryTests : VKUnitTestBase
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = new VKKnowledgeId(Guid.NewGuid());
        var segment = new VKPromptSegment { Content = "Knowledge Text" };
        var keys = new List<VKKnowledgeKey>
        {
            new() { Text = "dragon", MatchType = VKKnowledgeMatchType.Contains, CaseSensitive = false }
        };

        // Act
        var result = VKKnowledgeEntry.Create(
            id,
            segment,
            VKKnowledgeTriggerType.Keyword,
            VKKnowledgeFilterLogic.AndAll,
            xmlTag: "lore",
            keys: keys);

        // Assert
        result.Should().BeSuccess();
        var entry = result.Value!;
        entry.Id.Should().Be(id);
        entry.Segment.Content.Should().Be("Knowledge Text");
        entry.TriggerType.Should().Be(VKKnowledgeTriggerType.Keyword);
        entry.FilterLogic.Should().Be(VKKnowledgeFilterLogic.AndAll);
        entry.XmlTag.Should().Be("lore");
        entry.Keys.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsException()
    {
        // Act
        Action act = () => VKKnowledgeEntry.Create(VKKnowledgeId.Empty, new VKPromptSegment { Content = "C" });

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rehydrate_WithValidParameters_RestoresAggregate()
    {
        // Arrange
        var id = new VKKnowledgeId(Guid.NewGuid());
        var segment = new VKPromptSegment { Content = "Rehydrated" };

        // Act
        var entry = VKKnowledgeEntry.Rehydrate(
            id,
            segment,
            VKKnowledgeTriggerType.Constant,
            VKKnowledgeFilterLogic.AndAny,
            "tag",
            []);

        // Assert
        entry.Id.Should().Be(id);
        entry.TriggerType.Should().Be(VKKnowledgeTriggerType.Constant);
        entry.XmlTag.Should().Be("tag");
    }

    [Fact]
    public void UpdateSegment_WhenCalled_UpdatesSegment()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Original")
            .Build();

        var newSegment = new VKPromptSegment { Content = "New Segment" };

        // Act
        var result = entry.UpdateSegment(newSegment);

        // Assert
        result.Should().BeSuccess();
        entry.Segment.Content.Should().Be("New Segment");
    }

    [Fact]
    public void UpdateTriggerSettings_WhenCalled_UpdatesSettings()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Original")
            .Build();

        // Act
        var result = entry.UpdateTriggerSettings(
            VKKnowledgeTriggerType.Keyword,
            VKKnowledgeFilterLogic.AndAll,
            "newTag");

        // Assert
        result.Should().BeSuccess();
        entry.TriggerType.Should().Be(VKKnowledgeTriggerType.Keyword);
        entry.FilterLogic.Should().Be(VKKnowledgeFilterLogic.AndAll);
        entry.XmlTag.Should().Be("newTag");
    }

    [Fact]
    public void ReplaceKeys_WhenCalled_ReplacesAllKeys()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Original")
            .Build();

        var newKeys = new List<VKKnowledgeKey>
        {
            new() { Text = "magic", MatchType = VKKnowledgeMatchType.WholeWord }
        };

        // Act
        var result = entry.ReplaceKeys(newKeys);

        // Assert
        result.Should().BeSuccess();
        entry.Keys.Should().HaveCount(1);
        entry.Keys[0].Text.Should().Be("magic");
    }

    [Fact]
    public void AddKey_WhenCalled_AddsKeyToList()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Original")
            .Build();

        // Act
        var result = entry.AddKey(new VKKnowledgeKey { Text = "spell", MatchType = VKKnowledgeMatchType.Regex });

        // Assert
        result.Should().BeSuccess();
        entry.Keys.Should().HaveCount(1);
        entry.Keys[0].Text.Should().Be("spell");
    }
}
