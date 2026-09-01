using VK.Blocks.AI.Psyche.Pattern.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Pattern;

public sealed class InMemoryPatternRepositoryTests : VKUnitTestBase
{
    [Fact]
    public async Task FindByIdAsync_WhenExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();
        var pattern = new VKPatternEntryBuilder().WithContent("Rule").Build();
        repository.Seed(pattern);

        // Act
        var result = await repository.FindByIdAsync(pattern.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccessWithValue(pattern);
    }

    [Fact]
    public async Task FindByIdAsync_WhenEmptyOrNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();

        // Act
        var emptyRes = await repository.FindByIdAsync(VKPatternId.Empty, CancellationToken.None);
        var notFoundRes = await repository.FindByIdAsync(new VKPatternEntryBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKPatternErrors.NotFound);
        notFoundRes.Should().BeFailure(VKPatternErrors.NotFound);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenSeeded_ReturnsPatterns()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();
        var entry = new VKPatternEntryBuilder()
            .WithContent("Rule")
            .Build();
        repository.Seed(entry);

        // Act
        var result = await repository.ListByIdsAsync([entry.Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().ContainSingle(p => p.Id == entry.Id);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();

        // Act
        var result = await repository.ListByIdsAsync([new VKPatternEntryBuilder().Build().Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_WhenCalled_ReturnsAllPatterns()
    {
        // Arrange
        var p1 = new VKPatternEntryBuilder().WithContent("R1").Build();
        var p2 = new VKPatternEntryBuilder().WithContent("R2").Build();
        var repository = new InMemoryPatternRepository([p1, p2]);

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
        var repository = new InMemoryPatternRepository();
        var pattern = new VKPatternEntryBuilder().WithContent("R").Build();
        repository.Seed(pattern);

        // Act
        var exists = await repository.ExistsAsync(pattern.Id, CancellationToken.None);
        var notExists = await repository.ExistsAsync(new VKPatternEntryBuilder().Build().Id, CancellationToken.None);
        var emptyExists = await repository.ExistsAsync(VKPatternId.Empty, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
        emptyExists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WhenItemIsNew_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();
        var item = new VKPatternEntryBuilder().WithContent("R").Build();

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
        var repository = new InMemoryPatternRepository();
        var item = new VKPatternEntryBuilder().WithContent("R").Build();
        repository.Seed(item);

        // Act
        var result = await repository.AddAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKPatternErrors.AlreadyExists);
    }

    [Fact]
    public async Task UpdateAsync_WhenItemExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();
        var item = new VKPatternEntryBuilder().WithContent("Old").Build();
        repository.Seed(item);

        var updated = new VKPatternEntryBuilder().WithId(item.Id).WithContent("New").Build();

        // Act
        var result = await repository.UpdateAsync(updated, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var found = await repository.FindByIdAsync(item.Id, CancellationToken.None);
        found.Should().BeSuccess();
        found.Value!.Segment.Content.Should().Be("New");
    }

    [Fact]
    public async Task UpdateAsync_WhenItemNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();
        var item = new VKPatternEntryBuilder().WithContent("R").Build();

        // Act
        var result = await repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKPatternErrors.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemExists_RemovesAndReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();
        var pattern = new VKPatternEntryBuilder().WithContent("R").Build();
        repository.Seed(pattern);

        // Act
        var result = await repository.DeleteAsync(pattern.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        (await repository.ExistsAsync(pattern.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenItemNotFoundOrEmpty_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();

        // Act
        var emptyRes = await repository.DeleteAsync(VKPatternId.Empty, CancellationToken.None);
        var notFoundRes = await repository.DeleteAsync(new VKPatternEntryBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKPatternErrors.NotFound);
        notFoundRes.Should().BeFailure(VKPatternErrors.NotFound);
    }

    [Fact]
    public async Task RemoveAndClear_RemovesPatternsFromStore()
    {
        // Arrange
        var repository = new InMemoryPatternRepository();
        var p1 = new VKPatternEntryBuilder().WithContent("1").Build();
        var p2 = new VKPatternEntryBuilder().WithContent("2").Build();
        repository.Seed([p1, p2]);

        // Act
        repository.Remove(p1.Id);
        var res1 = await repository.ListByIdsAsync([p1.Id], CancellationToken.None);

        repository.Clear();
        var res2 = await repository.ListByIdsAsync([p2.Id], CancellationToken.None);

        // Assert
        res1.Should().BeSuccess();
        res1.Value.Should().BeEmpty();
        res2.Should().BeSuccess();
        res2.Value.Should().BeEmpty();
    }
}
