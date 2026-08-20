using System;
using FluentAssertions;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

public sealed class DefaultKnowledgeRendererTests
{
    [Fact]
    public void Render_ReturnsSegmentContent()
    {
        // Arrange
        var renderer = new DefaultKnowledgeRenderer();
        var entry = new VKKnowledgeEntry
        {
            Id = new VKKnowledgeId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Content = "Rendered knowledge text" }
        };

        // Act
        var result = renderer.Render(entry);

        // Assert
        result.Should().Be("Rendered knowledge text");
    }
}
