using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

public sealed class InMemoryEchoStoreTests : VKUnitTestBase
{
    [Fact]
    public async Task SaveTraceAsync_And_GetHistoryAsync_ReturnsTraces()
    {
        // Arrange
        var store = new InMemoryEchoStore();
        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var trace1 = new VKEchoTraceBuilder()
            .WithSessionId(sessionId)
            .WithRole(VKChatRole.User)
            .WithContent("Msg 1")
            .Build();
        var trace2 = new VKEchoTraceBuilder()
            .WithSessionId(sessionId)
            .WithRole(VKChatRole.Assistant)
            .WithContent("Msg 2")
            .Build();

        store.Seed(sessionId, [trace1, trace2]);

        // Act
        var result = await store.GetHistoryAsync(sessionId, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task TwoPhaseRetrieval_GetMetadataAndTracesByIds_OperatesCorrectly()
    {
        // Arrange
        var store = new InMemoryEchoStore();
        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var trace1 = new VKEchoTraceBuilder()
            .WithSessionId(sessionId)
            .WithRole(VKChatRole.User)
            .WithContent("Msg 1")
            .WithTokenCount(100)
            .Build();
        var trace2 = new VKEchoTraceBuilder()
            .WithSessionId(sessionId)
            .WithRole(VKChatRole.Assistant)
            .WithContent("Msg 2")
            .WithTokenCount(100)
            .Build();

        await store.SaveHistoryBatchAsync([trace1, trace2], CancellationToken.None);

        // Act: Phase 1
        var metaResult = await store.GetMetadataAsync(sessionId, CancellationToken.None);
        metaResult.Should().BeSuccess();
        metaResult.Value.Should().HaveCount(2);
        metaResult.Value.First().Id.Should().Be(trace1.Id);

        // Act: Phase 2 (fetch only trace2)
        var tracesResult = await store.GetTracesByIdsAsync([trace2.Id], CancellationToken.None);

        // Assert
        tracesResult.Should().BeSuccess();
        tracesResult.Value.Should().HaveCount(1);
        tracesResult.Value.First().Content.Should().Be("Msg 2");
    }

    [Fact]
    public async Task RemoveAndClear_OperatesCorrectly()
    {
        // Arrange
        var store = new InMemoryEchoStore();
        var sessionId = new VKSessionThreadBuilder().Build().Id;
        var trace = new VKEchoTraceBuilder()
            .WithSessionId(sessionId)
            .WithRole(VKChatRole.User)
            .WithContent("Msg 1")
            .Build();

        store.Seed(sessionId, trace);

        // Act
        store.Remove(sessionId);
        var res1 = await store.GetHistoryAsync(sessionId, CancellationToken.None);

        store.Seed(sessionId, trace);
        store.Clear();
        var res2 = await store.GetHistoryAsync(sessionId, CancellationToken.None);

        // Assert
        res1.Should().BeSuccess();
        res1.Value.Should().BeEmpty();
        res2.Should().BeSuccess();
        res2.Value.Should().BeEmpty();
    }
}
