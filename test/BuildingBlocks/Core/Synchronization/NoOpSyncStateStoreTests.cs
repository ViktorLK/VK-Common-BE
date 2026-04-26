namespace VK.Blocks.Core.UnitTests.Synchronization;

public class VKNoOpSyncStateStoreTests
{
    [Fact]
    public async Task GetLastHashAsync_ReturnsNull()
    {
        // Arrange
        var store = new VKNoOpSyncStateStore();

        // Act
        var result = await store.GetLastHashAsync("key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateHashAsync_ReturnsSuccess()
    {
        // Arrange
        var store = new VKNoOpSyncStateStore();

        // Act
        var result = await store.UpdateHashAsync("key", "hash");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
