using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VK.Blocks.AI.Psyche.Pattern.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Pattern;

public sealed class InMemoryPatternStoreTests
{
    [Fact]
    public async Task GetCurrentPatternsAsync_WhenSeeded_ReturnsPatterns()
    {
        // Arrange
        var store = new InMemoryPatternStore();
        var id = new VKPatternId(Guid.NewGuid());
        var entry = new VKPatternEntry
        {
            Id = id,
            Segment = new VKPromptSegment { Content = "Rule" }
        };
        store.Seed(entry);

        // Act
        var result = await store.GetCurrentPatternsAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(p => p.Id == id);
    }

    [Fact]
    public async Task RemoveAndClear_RemovesPatterns()
    {
        // Arrange
        var id1 = new VKPatternId(Guid.NewGuid());
        var id2 = new VKPatternId(Guid.NewGuid());
        var store = new InMemoryPatternStore([
            new VKPatternEntry { Id = id1, Segment = new VKPromptSegment { Content = "R1" } },
            new VKPatternEntry { Id = id2, Segment = new VKPromptSegment { Content = "R2" } }
        ]);

        // Act
        store.Remove(id1);
        var res1 = await store.GetCurrentPatternsAsync(CancellationToken.None);

        store.Clear();
        var res2 = await store.GetCurrentPatternsAsync(CancellationToken.None);

        // Assert
        res1.Value.Should().ContainSingle(p => p.Id == id2);
        res2.Value.Should().BeEmpty();
    }
}
