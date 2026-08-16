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
    public async Task SaveTraceAsync_And_GetHistoryAsync_ReturnsTracesMatchingTenant()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var store = new InMemoryEchoStore(identityMock.Object);
        var sessionId = new VKSessionId(Guid.NewGuid());
        var trace1 = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = sessionId,
            TenantId = VKTenantId.Default,
            Role = VKChatRole.User,
            Content = "Msg 1"
        };
        var traceOtherTenant = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = sessionId,
            TenantId = new VKTenantId(Guid.NewGuid()),
            Role = VKChatRole.User,
            Content = "Msg Other"
        };

        store.Seed(sessionId, [trace1, traceOtherTenant]);

        // Act
        var result = await store.GetHistoryAsync(sessionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(t => t.Content == "Msg 1");
    }

    [Fact]
    public async Task RemoveAndClear_OperatesCorrectly()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var store = new InMemoryEchoStore(identityMock.Object);
        var sessionId = new VKSessionId(Guid.NewGuid());
        var trace = new VKEchoTrace
        {
            Id = new VKEchoId(Guid.NewGuid()),
            SessionId = sessionId,
            TenantId = VKTenantId.Default,
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
