using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

public sealed class InMemoryEchoStoreTests
{
    [Fact]
    public async Task SaveTraceAsync_And_GetHistoryAsync_ReturnsTraces()
    {
        // Arrange
        var store = new InMemoryEchoStore();
        var sessionId = new VKSessionId(Guid.NewGuid());
        var trace1 = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = sessionId,
            Role = VKChatRole.User,
            Content = "Msg 1"
        };
        var trace2 = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = sessionId,
            Role = VKChatRole.Assistant,
            Content = "Msg 2"
        };

        store.Seed(sessionId, [trace1, trace2]);

        // Act
        var result = await store.GetHistoryAsync(sessionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task RemoveAndClear_OperatesCorrectly()
    {
        // Arrange
        var store = new InMemoryEchoStore();
        var sessionId = new VKSessionId(Guid.NewGuid());
        var trace = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = sessionId,
            Role = VKChatRole.User,
            Content = "Msg 1"
        };

        store.Seed(sessionId, trace);

        // Act
        store.Remove(sessionId);
        var res1 = await store.GetHistoryAsync(sessionId, CancellationToken.None);

        store.Seed(sessionId, trace);
        store.Clear();
        var res2 = await store.GetHistoryAsync(sessionId, CancellationToken.None);

        // Assert
        res1.Value.Should().BeEmpty();
        res2.Value.Should().BeEmpty();
    }
}
