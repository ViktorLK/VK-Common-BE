using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Pattern;

/// <summary>
/// Unit tests for <see cref="VKPatternEntry"/> aggregate root.
/// Follows AP.01, CS.01, and DL.01 rules.
/// </summary>
public sealed class VKPatternEntryTests : VKUnitTestBase
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = new VKPatternId(Guid.NewGuid());
        var segment = new VKPromptSegment { Content = "Pattern content", DepthPriority = 10 };

        // Act
        var result = VKPatternEntry.Create(id, segment);

        // Assert
        result.Should().BeSuccess();
        var pattern = result.Value!;
        pattern.Id.Should().Be(id);
        pattern.Segment.Content.Should().Be("Pattern content");
        pattern.Segment.DepthPriority.Should().Be(10);
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsException()
    {
        // Act
        Action act = () => VKPatternEntry.Create(VKPatternId.Empty, new VKPromptSegment { Content = "C" });

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rehydrate_WithValidParameters_RestoresAggregate()
    {
        // Arrange
        var id = new VKPatternId(Guid.NewGuid());
        var segment = new VKPromptSegment { Content = "Rehydrated" };

        // Act
        var pattern = VKPatternEntry.Rehydrate(id, segment);

        // Assert
        pattern.Id.Should().Be(id);
        pattern.Segment.Content.Should().Be("Rehydrated");
    }

    [Fact]
    public void UpdateSegment_WhenCalled_UpdatesSegment()
    {
        // Arrange
        var pattern = new VKPatternEntryBuilder()
            .WithContent("Initial")
            .Build();

        var newSegment = new VKPromptSegment { Content = "Updated", DepthPriority = 20 };

        // Act
        var result = pattern.UpdateSegment(newSegment);

        // Assert
        result.Should().BeSuccess();
        pattern.Segment.Content.Should().Be("Updated");
        pattern.Segment.DepthPriority.Should().Be(20);
    }
}
