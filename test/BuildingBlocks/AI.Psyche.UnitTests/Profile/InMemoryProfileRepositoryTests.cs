using VK.Blocks.AI.Psyche.Profile.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Profile;

public sealed class InMemoryProfileRepositoryTests : VKUnitTestBase
{
    [Fact]
    public async Task FindByIdAsync_WhenSeeded_ReturnsPresence()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var profile = new VKProfilePresenceBuilder()
            .WithPreferredLanguage("ja-JP")
            .Build();
        repository.Seed(profile);

        // Act
        var result = await repository.FindByIdAsync(profile.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccessWithValue(profile);
    }

    [Fact]
    public async Task FindByIdAsync_WhenEmptyOrNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var profileId = new VKProfileId(Guid.NewGuid());

        // Act
        var emptyRes = await repository.FindByIdAsync(VKProfileId.Empty, CancellationToken.None);
        var getRes = await repository.FindByIdAsync(new VKProfilePresenceBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKProfileErrors.NotFound);
        getRes.Should().BeFailure(VKProfileErrors.NotFound);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenSeeded_ReturnsMatches()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var profile = new VKProfilePresenceBuilder().WithDisplayName("Alice").Build();
        repository.Seed(profile);

        // Act
        var result = await repository.ListByIdsAsync([profile.Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().ContainSingle(p => p.Id == profile.Id);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();

        // Act
        var result = await repository.ListByIdsAsync([new VKProfilePresenceBuilder().Build().Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_WhenCalled_ReturnsAllProfiles()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var p1 = new VKProfilePresenceBuilder().WithDisplayName("A").Build();
        var p2 = new VKProfilePresenceBuilder().WithDisplayName("B").Build();
        repository.Seed(p1);
        repository.Seed(p2);

        // Act
        var result = await repository.ListAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExistsAsync_WhenCalled_ReturnsExpectedBoolean()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var profile = new VKProfilePresenceBuilder().Build();
        repository.Seed(profile);

        // Act
        var exists = await repository.ExistsAsync(profile.Id, CancellationToken.None);
        var notExists = await repository.ExistsAsync(new VKProfilePresenceBuilder().Build().Id, CancellationToken.None);
        var emptyExists = await repository.ExistsAsync(VKProfileId.Empty, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
        emptyExists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WhenItemIsNew_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var item = new VKProfilePresenceBuilder().Build();

        // Act
        var result = await repository.AddAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        (await repository.ExistsAsync(item.Id, CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_WhenItemAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var item = new VKProfilePresenceBuilder().Build();
        repository.Seed(item);

        // Act
        var result = await repository.AddAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKProfileErrors.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenItemExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var item = new VKProfilePresenceBuilder().WithDisplayName("Old").Build();
        repository.Seed(item);

        var updated = new VKProfilePresenceBuilder().WithId(item.Id).WithDisplayName("New").Build();

        // Act
        var result = await repository.UpdateAsync(updated, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var found = await repository.FindByIdAsync(item.Id, CancellationToken.None);
        found.Should().BeSuccess();
        found.Value!.DisplayName.Should().Be("New");
    }

    [Fact]
    public async Task UpdateAsync_WhenItemNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var item = new VKProfilePresenceBuilder().Build();

        // Act
        var result = await repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKProfileErrors.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemExists_RemovesAndReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();
        var profile = new VKProfilePresenceBuilder().Build();
        repository.Seed(profile);

        // Act
        var result = await repository.DeleteAsync(profile.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        (await repository.ExistsAsync(profile.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenItemNotFoundOrEmpty_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryProfileRepository();

        // Act
        var emptyRes = await repository.DeleteAsync(VKProfileId.Empty, CancellationToken.None);
        var notFoundRes = await repository.DeleteAsync(new VKProfilePresenceBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKProfileErrors.NotFound);
        notFoundRes.Should().BeFailure(VKProfileErrors.NotFound);
    }

    [Fact]
    public async Task RemoveAndClear_RemovesProfiles()
    {
        // Arrange
        var p1 = new VKProfilePresenceBuilder().WithDisplayName("P1").Build();
        var p2 = new VKProfilePresenceBuilder().WithDisplayName("P2").Build();
        var repository = new InMemoryProfileRepository();
        repository.Seed(p1);
        repository.Seed(p2);

        // Act
        repository.Remove(p1.Id);
        var res1 = await repository.ListAllAsync(CancellationToken.None);

        repository.Clear();
        var res2 = await repository.ListAllAsync(CancellationToken.None);

        // Assert
        res1.Should().BeSuccess();
        res1.Value.Should().ContainSingle(p => p.Id == p2.Id);
        res2.Should().BeSuccess();
        res2.Value.Should().BeEmpty();
    }
}
