using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

public sealed class InMemoryKnowledgeStoreTests
{
    [Fact]
    public async Task GetKnowledgeEntriesAsync_WhenSeeded_ReturnsEntries()
    {
        // Arrange
        var store = new InMemoryKnowledgeStore();
        var idGuid = Guid.NewGuid();
        var entryId = new VKKnowledgeId(idGuid);
        var entry = new VKKnowledgeEntry
        {
            Id = entryId,
            Segment = new VKPromptSegment { Content = "Knowledge Text" }
        };
        store.Seed(entry);

        // Act
        var result = await store.GetKnowledgeEntriesAsync([entryId], CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(e => e.Id == entry.Id);
    }

    [Fact]
    public async Task GetKnowledgeEntriesAsync_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        var store = new InMemoryKnowledgeStore();

        // Act
        var result = await store.GetKnowledgeEntriesAsync([new VKKnowledgeId(Guid.NewGuid())], CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
