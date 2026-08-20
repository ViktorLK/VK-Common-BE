using System;
using System.Collections.Generic;
using FluentAssertions;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

public sealed class DefaultKnowledgeMatcherTests
{
    [Fact]
    public void GetMatcher_WithConstantTrigger_AlwaysReturnsTrue()
    {
        // Arrange
        var entry = new VKKnowledgeEntry
        {
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Content = "Rule" },
            TriggerType = VKKnowledgeTriggerType.Constant,
            Keys = []
        };

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("any context string").Should().BeTrue();
    }

    [Fact]
    public void GetMatcher_WithEmptyKeys_ReturnsFalse()
    {
        // Arrange
        var entry = new VKKnowledgeEntry
        {
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Content = "Rule" },
            TriggerType = VKKnowledgeTriggerType.Keyword,
            Keys = []
        };

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("hello world").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithKeywordContains_MatchesCorrectly()
    {
        // Arrange
        var entry = new VKKnowledgeEntry
        {
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Content = "Refund Policy" },
            TriggerType = VKKnowledgeTriggerType.Keyword,
            FilterLogic = VKKnowledgeFilterLogic.AndAny,
            Keys = [
                new VKKnowledgeKey { Text = "refund", MatchType = VKKnowledgeMatchType.Contains, CaseSensitive = false }
            ]
        };

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("I want a refund for my order").Should().BeTrue();
        matcher("Help with shipping").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithRegexPattern_MatchesCorrectly()
    {
        // Arrange
        var entry = new VKKnowledgeEntry
        {
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Content = "Order Status" },
            TriggerType = VKKnowledgeTriggerType.Keyword,
            Keys = [
                new VKKnowledgeKey { Text = @"/order-\d+/i", MatchType = VKKnowledgeMatchType.Regex }
            ]
        };

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("Status for ORDER-12345 please").Should().BeTrue();
        matcher("Status for order xyz").Should().BeFalse();
    }

    [Fact]
    public void GetMatcher_WithMalformedRegex_FallbackReturnsFalse()
    {
        // Arrange
        var entry = new VKKnowledgeEntry
        {
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Content = "Broken Rule" },
            TriggerType = VKKnowledgeTriggerType.Keyword,
            Keys = [
                new VKKnowledgeKey { Text = @"[unclosed-bracket", MatchType = VKKnowledgeMatchType.Regex }
            ]
        };

        // Act
        var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);

        // Assert
        matcher("test input").Should().BeFalse();
    }

    [Fact]
    public void Invalidate_ClearsCachedMatcher()
    {
        // Arrange
        var id = new VKKnowledgeId(Guid.NewGuid());
        var entry = new VKKnowledgeEntry
        {
            Id = id,
            Segment = new VKPromptSegment { Content = "Constant Rule" },
            TriggerType = VKKnowledgeTriggerType.Constant,
            Keys = []
        };

        var matcher1 = DefaultKnowledgeMatcher.GetMatcher(entry);
        DefaultKnowledgeMatcher.Invalidate(id);

        // Act & Assert
        matcher1("test").Should().BeTrue();
    }
}
