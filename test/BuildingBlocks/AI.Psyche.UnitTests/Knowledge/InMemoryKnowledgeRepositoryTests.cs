using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

public sealed class InMemoryKnowledgeRepositoryTests : VKUnitTestBase
{
    [Fact]
    public async Task FindByIdAsync_WhenExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();
        var entry = new VKKnowledgeEntryBuilder().WithContent("Doc").Build();
        repository.Seed(entry);

        // Act
        var result = await repository.FindByIdAsync(entry.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccessWithValue(entry);
    }

    [Fact]
    public async Task FindByIdAsync_WhenEmptyOrNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();

        // Act
        var emptyRes = await repository.FindByIdAsync(VKKnowledgeId.Empty, CancellationToken.None);
        var notFoundRes = await repository.FindByIdAsync(new VKKnowledgeEntryBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKKnowledgeErrors.NotFound);
        notFoundRes.Should().BeFailure(VKKnowledgeErrors.NotFound);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenSeeded_ReturnsEntries()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Knowledge Text")
            .Build();
        repository.Seed(entry);

        // Act
        var result = await repository.ListByIdsAsync([entry.Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().ContainSingle(e => e.Id == entry.Id);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();

        // Act
        var result = await repository.ListByIdsAsync([new VKKnowledgeEntryBuilder().Build().Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_ReturnsAllSeededEntries()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();
        var entry1 = new VKKnowledgeEntryBuilder().WithContent("Doc 1").Build();
        var entry2 = new VKKnowledgeEntryBuilder().WithContent("Doc 2").Build();
        repository.Seed([entry1, entry2]);

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
        var repository = new InMemoryKnowledgeRepository();
        var entry = new VKKnowledgeEntryBuilder().WithContent("Doc").Build();
        repository.Seed(entry);

        // Act
        var exists = await repository.ExistsAsync(entry.Id, CancellationToken.None);
        var notExists = await repository.ExistsAsync(new VKKnowledgeEntryBuilder().Build().Id, CancellationToken.None);
        var emptyExists = await repository.ExistsAsync(VKKnowledgeId.Empty, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
        emptyExists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WhenItemIsNew_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();
        var item = new VKKnowledgeEntryBuilder().WithContent("Doc").Build();

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
        var repository = new InMemoryKnowledgeRepository();
        var item = new VKKnowledgeEntryBuilder().WithContent("Doc").Build();
        repository.Seed(item);

        // Act
        var result = await repository.AddAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKKnowledgeErrors.AlreadyExists);
    }

    [Fact]
    public async Task UpdateAsync_WhenItemExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();
        var item = new VKKnowledgeEntryBuilder().WithContent("Old").Build();
        repository.Seed(item);

        var updated = new VKKnowledgeEntryBuilder().WithId(item.Id).WithContent("New").Build();

        // Act
        var result = await repository.UpdateAsync(updated, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var found = await repository.FindByIdAsync(item.Id, CancellationToken.None);
        found.Should().BeSuccess();
        found.Value!.Payload.Content.Should().Be("New");
    }

    [Fact]
    public async Task UpdateAsync_WhenItemNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();
        var item = new VKKnowledgeEntryBuilder().WithContent("Doc").Build();

        // Act
        var result = await repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKKnowledgeErrors.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemExists_RemovesAndReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();
        var entry = new VKKnowledgeEntryBuilder().WithContent("Doc").Build();
        repository.Seed(entry);

        // Act
        var result = await repository.DeleteAsync(entry.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        (await repository.ExistsAsync(entry.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenItemNotFoundOrEmpty_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();

        // Act
        var emptyRes = await repository.DeleteAsync(VKKnowledgeId.Empty, CancellationToken.None);
        var notFoundRes = await repository.DeleteAsync(new VKKnowledgeEntryBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKKnowledgeErrors.NotFound);
        notFoundRes.Should().BeFailure(VKKnowledgeErrors.NotFound);
    }

    [Fact]
    public async Task Clear_WhenCalled_RemovesAllEntries()
    {
        // Arrange
        var repository = new InMemoryKnowledgeRepository();
        var entry = new VKKnowledgeEntryBuilder().WithContent("Doc").Build();
        repository.Seed(entry);

        // Act
        repository.Clear();
        var result = await repository.ListAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().BeEmpty();
    }
}
