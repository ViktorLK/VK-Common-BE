using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Session.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Session;

public sealed class InMemorySessionStoreTests
{
    [Fact]
    public async Task GetSessionAsync_WhenSeeded_ReturnsSession()
    {
        // Arrange
        var store = new InMemorySessionStore();
        var sessionId = new VKSessionId(Guid.NewGuid());
        var session = new VKSessionThread
        {
            Id = sessionId
        };
        store.Seed(session);

        // Act
        var result = await store.GetSessionAsync(sessionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(session);
    }

    [Fact]
    public async Task GetSessionAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var store = new InMemorySessionStore();
        var sessionId = new VKSessionId(Guid.NewGuid());

        // Act
        var result = await store.GetSessionAsync(sessionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Action act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateSessionAsync_UpdatesSessionSuccessfully()
    {
        // Arrange
        var store = new InMemorySessionStore();
        var sessionId = new VKSessionId(Guid.NewGuid());
        var session = new VKSessionThread
        {
            Id = sessionId
        };

        // Act
        var saveResult = await store.UpdateSessionAsync(session, CancellationToken.None);
        var getResult = await store.GetSessionAsync(sessionId, CancellationToken.None);

        // Assert
        saveResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().Be(session);
    }
}
