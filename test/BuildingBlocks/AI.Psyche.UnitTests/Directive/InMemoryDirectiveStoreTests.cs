using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

public sealed class InMemoryDirectiveStoreTests
{
    [Fact]
    public async Task GetDirectivesAsync_WhenSeeded_ReturnsSuccess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<InMemoryDirectiveStore>>();

        var store = new InMemoryDirectiveStore(loggerMock.Object);
        var directiveId = new VKDirectiveId(Guid.NewGuid());
        var directive = new VKDirectiveCharter
        {
            Id = directiveId,
            Overview = "Test Charter"
        };
        store.Seed(directive);

        // Act
        var result = await store.GetDirectivesAsync([directiveId], CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(d => d.Id == directiveId);
    }

    [Fact]
    public async Task GetDirectivesAsync_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<InMemoryDirectiveStore>>();

        var store = new InMemoryDirectiveStore(loggerMock.Object);

        // Act
        var result = await store.GetDirectivesAsync([new VKDirectiveId(Guid.NewGuid())], CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveAndClear_RemovesDirectivesFromStore()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<InMemoryDirectiveStore>>();

        var store = new InMemoryDirectiveStore(loggerMock.Object);
        var id1 = new VKDirectiveId(Guid.NewGuid());
        var id2 = new VKDirectiveId(Guid.NewGuid());
        store.Seed([
            new VKDirectiveCharter { Id = id1, Overview = "1" },
            new VKDirectiveCharter { Id = id2, Overview = "2" }
        ]);

        // Act
        store.Remove(id1);
        var res1 = await store.GetDirectivesAsync([id1], CancellationToken.None);

        store.Clear();
        var res2 = await store.GetDirectivesAsync([id2], CancellationToken.None);

        // Assert
        res1.Value.Should().BeEmpty();
        res2.Value.Should().BeEmpty();
    }
}
