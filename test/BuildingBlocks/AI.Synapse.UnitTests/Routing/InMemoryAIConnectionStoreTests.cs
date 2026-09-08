using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VK.Blocks.AI.Synapse.Routing.Internal;
using VK.Blocks.AI.Synapse.UnitTests.Builders;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Routing;

/// <summary>
/// Unit tests for <see cref="InMemoryAIConnectionStore"/>.
/// Follows AP.01, CS.01, CS.03, and DL.01.
/// </summary>
public sealed class InMemoryAIConnectionStoreTests
{
    [Fact]
    public async Task Seed_And_GetConnectionListAsync_ReturnsSeededConnections()
    {
        // Arrange
        var store = new InMemoryAIConnectionStore();
        var conn1 = new VKAIConnectionBuilder().WithId("conn-1").Build();
        var conn2 = new VKAIConnectionBuilder().WithId("conn-2").Build();

        // Act
        store.Seed(conn1).Seed(conn2);
        var result = await store.GetConnectionListAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(conn1);
        result.Value.Should().Contain(conn2);
    }
}
