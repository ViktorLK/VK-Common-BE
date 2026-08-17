using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

/// <summary>
/// Unit tests for the <see cref="DefaultKnowledgeFormatter"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultKnowledgeFormatterTests
{
    private readonly Mock<IVKKnowledgeRenderer> _rendererMock;
    private readonly DefaultKnowledgeFormatter _formatter;

    public DefaultKnowledgeFormatterTests()
    {
        _rendererMock = new Mock<IVKKnowledgeRenderer>();
        _formatter = new DefaultKnowledgeFormatter(_rendererMock.Object);
    }

    private static VKPsycheContext CreateTestContext()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var request = new VKPsycheRequest
        {
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            UserInput = "test-user-input"
        };

        return new VKPsycheContext
        {
            Request = request,
            Services = services
        };
    }

    [Fact]
    public void CanFormat_WhenTierIsKnowledge_ReturnsTrue()
    {
        // Arrange
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = 0,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "test" },
            Metadata = new VKKnowledgeEntry
            {
                TenantId = VKTenantId.Default,
                Id = new VKKnowledgeId(Guid.NewGuid()),
                Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "test" }
            }
        };

        // Act
        var result = _formatter.CanFormat(fragment);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Format_WhenSingleEntry_WrapsInKnowledgeTag()
    {
        // Arrange
        var entry = new VKKnowledgeEntry
        {
            TenantId = VKTenantId.Default,
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Apples are red." }
        };

        _rendererMock.Setup(r => r.Render(entry)).Returns("Apples are red.");

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
        var result = _formatter.Format(fragment, context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void Format_WhenSinglePinnedEntry_UsesEntryTagRegardlessOfDepth()
    {
        // Arrange
        var entry = new VKKnowledgeEntry
        {
            TenantId = VKTenantId.Default,
            Id = new VKKnowledgeId(Guid.NewGuid()),
            XmlTag = "lore",
            Segment = new VKPromptSegment { Role = VKChatRole.User, Content = "Apples are red." }
        };

        _rendererMock.Setup(r => r.Render(entry)).Returns("Apples are red.");

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
        var result = _formatter.Format(fragment, context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void Format_WhenMultipleEntriesInSameSlot_GroupsIntoSingleTagAndYieldsEmptyForOthers()
    {
        // Arrange
        var entry1 = new VKKnowledgeEntry
        {
            TenantId = VKTenantId.Default,
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Apples are red." }
        };
        var entry2 = new VKKnowledgeEntry
        {
            TenantId = VKTenantId.Default,
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Bananas are yellow." }
        };

        _rendererMock.Setup(r => r.Render(entry1)).Returns("Apples are red.");
        _rendererMock.Setup(r => r.Render(entry2)).Returns("Bananas are yellow.");

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
        var result1 = _formatter.Format(frag1, context);
        var result2 = _formatter.Format(frag2, context);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be(expected);

        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().BeEmpty();
    }

    [Fact]
    public void Format_WhenEntriesInDifferentSlots_DoesNotGroupThem()
    {
        // Arrange
        var entryBefore = new VKKnowledgeEntry
        {
            TenantId = VKTenantId.Default,
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Before fact.", AbsoluteDepth = 1 }
        };
        var entryAfter = new VKKnowledgeEntry
        {
            TenantId = VKTenantId.Default,
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "After fact.", AbsoluteDepth = 2 }
        };

        _rendererMock.Setup(r => r.Render(entryBefore)).Returns("Before fact.");
        _rendererMock.Setup(r => r.Render(entryAfter)).Returns("After fact.");

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
        var resultBefore = _formatter.Format(fragBefore, context);
        var resultAfter = _formatter.Format(fragAfter, context);

        // Assert
        resultBefore.IsSuccess.Should().BeTrue();
        resultBefore.Value.Should().Be(expectedBefore);

        resultAfter.IsSuccess.Should().BeTrue();
        resultAfter.Value.Should().Be(expectedAfter);
    }
}
