using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

/// <summary>
/// Unit tests for the <see cref="DefaultKnowledgeFormatter"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultKnowledgeFormatterTests : VKUnitTestBase
{
    private static VKPsycheContext CreateTestContext() =>
        new VKPsycheRequestBuilder().WithUserInput("test-user-input").BuildContext().Context;

    [Fact]
    public void CanFormat_WhenTierIsKnowledge_ReturnsTrue()
    {
        // Arrange
        var formatter = new DefaultKnowledgeFormatter(GetMockObject<IVKKnowledgeRenderer>());
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = 0,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "test" },
            Metadata = new VKKnowledgeEntryBuilder().WithContent("test").Build()
        };

        // Act
        var result = formatter.CanFormat(fragment);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Format_WhenSingleEntry_WrapsInKnowledgeTag()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Apples are red.")
            .Build();

        GetMock<IVKKnowledgeRenderer>()
            .Setup(r => r.Render(entry))
            .Returns("Apples are red.");

        var formatter = new DefaultKnowledgeFormatter(GetMockObject<IVKKnowledgeRenderer>());
        var context = CreateTestContext();
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = 900,
            Segment = entry.Segment,
            Metadata = entry
        };
        context.AddFragment(fragment);

        var expected =
            $"<knowledge>{Environment.NewLine}" +
            $"Apples are red.{Environment.NewLine}" +
            $"</knowledge>";

        // Act
        var result = formatter.Format(fragment, context);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void Format_WhenSinglePinnedEntry_UsesEntryTagRegardlessOfDepth()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Apples are red.", VKChatRole.User)
            .WithXmlTag("lore")
            .Build();

        GetMock<IVKKnowledgeRenderer>()
            .Setup(r => r.Render(entry))
            .Returns("Apples are red.");

        var formatter = new DefaultKnowledgeFormatter(GetMockObject<IVKKnowledgeRenderer>());
        var context = CreateTestContext();
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = 0,
            Segment = entry.Segment,
            Metadata = entry
        };
        context.AddFragment(fragment);

        var expected =
            $"<lore>{Environment.NewLine}" +
            $"Apples are red.{Environment.NewLine}" +
            $"</lore>";

        // Act
        var result = formatter.Format(fragment, context);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void Format_WhenMultipleEntriesInSameSlot_GroupsIntoSingleTagAndYieldsEmptyForOthers()
    {
        // Arrange
        var entry1 = new VKKnowledgeEntryBuilder().WithContent("Apples are red.").Build();
        var entry2 = new VKKnowledgeEntryBuilder().WithContent("Bananas are yellow.").Build();

        GetMock<IVKKnowledgeRenderer>().Setup(r => r.Render(entry1)).Returns("Apples are red.");
        GetMock<IVKKnowledgeRenderer>().Setup(r => r.Render(entry2)).Returns("Bananas are yellow.");

        var formatter = new DefaultKnowledgeFormatter(GetMockObject<IVKKnowledgeRenderer>());
        var context = CreateTestContext();
        var frag1 = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = 900,
            Segment = entry1.Segment,
            Metadata = entry1
        };
        var frag2 = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = 901,
            Segment = entry2.Segment,
            Metadata = entry2
        };
        context.AddFragment(frag1);
        context.AddFragment(frag2);

        var expected =
            $"<knowledge>{Environment.NewLine}" +
            $"Apples are red.{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"Bananas are yellow.{Environment.NewLine}" +
            $"</knowledge>";

        // Act
        var result1 = formatter.Format(frag1, context);
        var result2 = formatter.Format(frag2, context);

        // Assert
        result1.Should().BeSuccess();
        result1.Value.Should().Be(expected);

        result2.Should().BeSuccess();
        result2.Value.Should().BeEmpty();
    }

    [Fact]
    public void Format_WhenEntriesInDifferentSlots_DoesNotGroupThem()
    {
        // Arrange
        var entryBefore = new VKKnowledgeEntryBuilder()
            .WithSegment(new VKPromptSegment { Role = VKChatRole.System, Content = "Before fact.", AbsoluteDepth = 1 })
            .Build();
        var entryAfter = new VKKnowledgeEntryBuilder()
            .WithSegment(new VKPromptSegment { Role = VKChatRole.System, Content = "After fact.", AbsoluteDepth = 2 })
            .Build();

        GetMock<IVKKnowledgeRenderer>().Setup(r => r.Render(entryBefore)).Returns("Before fact.");
        GetMock<IVKKnowledgeRenderer>().Setup(r => r.Render(entryAfter)).Returns("After fact.");

        var formatter = new DefaultKnowledgeFormatter(GetMockObject<IVKKnowledgeRenderer>());
        var context = CreateTestContext();
        var fragBefore = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = 900,
            Segment = entryBefore.Segment,
            Metadata = entryBefore
        };
        var fragAfter = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = 1100,
            Segment = entryAfter.Segment,
            Metadata = entryAfter
        };
        context.AddFragment(fragBefore);
        context.AddFragment(fragAfter);

        var expectedBefore =
            $"<knowledge>{Environment.NewLine}" +
            $"Before fact.{Environment.NewLine}" +
            $"</knowledge>";

        var expectedAfter =
            $"<knowledge>{Environment.NewLine}" +
            $"After fact.{Environment.NewLine}" +
            $"</knowledge>";

        // Act
        var resultBefore = formatter.Format(fragBefore, context);
        var resultAfter = formatter.Format(fragAfter, context);

        // Assert
        resultBefore.Should().BeSuccess();
        resultBefore.Value.Should().Be(expectedBefore);

        resultAfter.Should().BeSuccess();
        resultAfter.Value.Should().Be(expectedAfter);
    }
}
