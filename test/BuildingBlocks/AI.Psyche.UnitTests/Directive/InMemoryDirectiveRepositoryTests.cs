using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

public sealed class InMemoryDirectiveRepositoryTests : VKUnitTestBase
{
    [Fact]
    public async Task FindByIdAsync_WhenExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var item = new VKDirectiveCharterBuilder().WithOverview("Overview").Build();
        repository.Seed(item);

        // Act
        var result = await repository.FindByIdAsync(item.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccessWithValue(item);
    }

    [Fact]
    public async Task FindByIdAsync_WhenEmptyOrNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var id = new VKDirectiveId(Guid.NewGuid());

        // Act
        var emptyRes = await repository.FindByIdAsync(VKDirectiveId.Empty, CancellationToken.None);
        var notFoundRes = await repository.FindByIdAsync(new VKDirectiveCharterBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKDirectiveErrors.NotFound);
        notFoundRes.Should().BeFailure(VKDirectiveErrors.NotFound);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenSeeded_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var directive = new VKDirectiveCharterBuilder().WithOverview("Test Charter").Build();
        repository.Seed(directive);

        // Act
        var result = await repository.ListByIdsAsync([directive.Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().ContainSingle(d => d.Id == directive.Id);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());

        // Act
        var result = await repository.ListByIdsAsync([new VKDirectiveCharterBuilder().Build().Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_WhenCalled_ReturnsAllDirectives()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var item1 = new VKDirectiveCharterBuilder().Build();
        var item2 = new VKDirectiveCharterBuilder().Build();
        repository.Seed([item1, item2]);

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
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var item = new VKDirectiveCharterBuilder().Build();
        repository.Seed(item);

        // Act
        var exists = await repository.ExistsAsync(item.Id, CancellationToken.None);
        var notExists = await repository.ExistsAsync(new VKDirectiveCharterBuilder().Build().Id, CancellationToken.None);
        var emptyExists = await repository.ExistsAsync(VKDirectiveId.Empty, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
        emptyExists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WhenItemIsNew_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var item = new VKDirectiveCharterBuilder().Build();

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
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var item = new VKDirectiveCharterBuilder().Build();
        repository.Seed(item);

        // Act
        var result = await repository.AddAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKDirectiveErrors.AlreadyExists);
    }

    [Fact]
    public async Task UpdateAsync_WhenItemExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var item = new VKDirectiveCharterBuilder().WithOverview("Old").Build();
        repository.Seed(item);

        var updated = new VKDirectiveCharterBuilder().WithId(item.Id).WithOverview("New").Build();

        // Act
        var result = await repository.UpdateAsync(updated, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var found = await repository.FindByIdAsync(item.Id, CancellationToken.None);
        found.Should().BeSuccess();
        found.Value!.Overview.Should().Be("New");
    }

    [Fact]
    public async Task UpdateAsync_WhenItemNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var item = new VKDirectiveCharterBuilder().Build();

        // Act
        var result = await repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKDirectiveErrors.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemExists_RemovesAndReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var item = new VKDirectiveCharterBuilder().Build();
        repository.Seed(item);

        // Act
        var result = await repository.DeleteAsync(item.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        (await repository.ExistsAsync(item.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenItemNotFoundOrEmpty_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());

        // Act
        var emptyRes = await repository.DeleteAsync(VKDirectiveId.Empty, CancellationToken.None);
        var notFoundRes = await repository.DeleteAsync(new VKDirectiveCharterBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKDirectiveErrors.NotFound);
        notFoundRes.Should().BeFailure(VKDirectiveErrors.NotFound);
    }

    [Fact]
    public async Task RemoveAndClear_RemovesDirectivesFromStore()
    {
        // Arrange
        var repository = new InMemoryDirectiveRepository(GetMockObject<ILogger<InMemoryDirectiveRepository>>());
        var item1 = new VKDirectiveCharterBuilder().WithOverview("1").Build();
        var item2 = new VKDirectiveCharterBuilder().WithOverview("2").Build();
        repository.Seed([item1, item2]);

        // Act
        repository.Remove(item1.Id);
        var res1 = await repository.ListByIdsAsync([item1.Id], CancellationToken.None);

        repository.Clear();
        var res2 = await repository.ListByIdsAsync([item2.Id], CancellationToken.None);

        // Assert
        res1.Should().BeSuccess();
        res1.Value.Should().BeEmpty();
        res2.Should().BeSuccess();
        res2.Value.Should().BeEmpty();
    }
}
