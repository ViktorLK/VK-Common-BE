using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

public sealed class DefaultKnowledgeRendererTests : VKUnitTestBase
{
    [Fact]
    public void Render_ReturnsSegmentContent()
    {
        // Arrange
        var renderer = new DefaultKnowledgeRenderer();
        var entry = new VKKnowledgeEntryBuilder()
            .WithSegment(new VKPromptSegment { Content = "Rendered knowledge text" })
            .Build();

        // Act
        var result = renderer.Render(entry);

        // Assert
        result.Should().Be("Rendered knowledge text");
    }
}
