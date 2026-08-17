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
    public async Task GetRelevantEntriesAsync_WhenSeeded_ReturnsMatchingTenantEntries()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var store = new InMemoryKnowledgeStore(identityMock.Object);
        var idGuid = Guid.NewGuid();
        var personaId = new VKPersonaId(idGuid);
        var entry = new VKKnowledgeEntry
        {
            Id = new VKKnowledgeId(idGuid),
            TenantId = VKTenantId.Default,
            Segment = new VKPromptSegment { Content = "Knowledge Text" }
        };
        store.Seed(entry);

        // Act
        var result = await store.GetRelevantEntriesAsync(personaId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(e => e.Id == entry.Id);
    }

    [Fact]
    public async Task GetRelevantEntriesAsync_WhenNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var store = new InMemoryKnowledgeStore(identityMock.Object);

        // Act
        var result = await store.GetRelevantEntriesAsync(new VKPersonaId(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKKnowledgeErrors.NotFound);
    }
}
