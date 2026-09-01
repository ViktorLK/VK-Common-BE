using VK.Blocks.AI.Psyche.Session.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Session;

public sealed class InMemorySessionRepositoryTests : VKUnitTestBase
{
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public async Task FindByIdAsync_WhenSeeded_ReturnsSession()
    {
        // Arrange
        var repository = new InMemorySessionRepository();
        var session = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        repository.Seed(session);

        // Act
        var result = await repository.FindByIdAsync(session.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccessWithValue(session);
    }

    [Fact]
    public async Task FindByIdAsync_WhenEmptyOrNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var repository = new InMemorySessionRepository();
        var sessionId = new VKSessionId(Guid.NewGuid());

        // Act
        var emptyRes = await repository.FindByIdAsync(VKSessionId.Empty, CancellationToken.None);
        var result = await repository.FindByIdAsync(new VKSessionThreadBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKSessionErrors.NotFound);
        result.Should().BeFailure(VKSessionErrors.NotFound);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenSeeded_ReturnsMatches()
    {
        // Arrange
        var repository = new InMemorySessionRepository();
        var session = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        repository.Seed(session);

        // Act
        var result = await repository.ListByIdsAsync([session.Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().ContainSingle(s => s.Id == session.Id);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        var repository = new InMemorySessionRepository();

        // Act
        var result = await repository.ListByIdsAsync([new VKSessionThreadBuilder().Build().Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_WhenCalled_ReturnsAllSessions()
    {
        // Arrange
        var repository = new InMemorySessionRepository();
        var s1 = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        var s2 = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        repository.Seed(s1);
        repository.Seed(s2);

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
        var repository = new InMemorySessionRepository();
        var session = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        repository.Seed(session);

        // Act
        var exists = await repository.ExistsAsync(session.Id, CancellationToken.None);
        var notExists = await repository.ExistsAsync(new VKSessionThreadBuilder().Build().Id, CancellationToken.None);
        var emptyExists = await repository.ExistsAsync(VKSessionId.Empty, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
        emptyExists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WhenItemIsNew_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemorySessionRepository();
        var item = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();

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
        var repository = new InMemorySessionRepository();
        var item = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        repository.Seed(item);

        // Act
        var result = await repository.AddAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKSessionErrors.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesSessionSuccessfully()
    {
        // Arrange
        var repository = new InMemorySessionRepository();
        var session = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        repository.Seed(session);

        // Act
        var saveResult = await repository.UpdateAsync(session, CancellationToken.None);
        var getResult = await repository.FindByIdAsync(session.Id, CancellationToken.None);

        // Assert
        saveResult.Should().BeSuccess();
        getResult.Should().BeSuccessWithValue(session);
    }

    [Fact]
    public async Task UpdateAsync_WhenItemNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemorySessionRepository();
        var item = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();

        // Act
        var result = await repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKSessionErrors.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemExists_RemovesAndReturnsSuccess()
    {
        // Arrange
        var repository = new InMemorySessionRepository();
        var session = new VKSessionThreadBuilder().WithCreatedAt(_now).Build();
        repository.Seed(session);

        // Act
        var result = await repository.DeleteAsync(session.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        (await repository.ExistsAsync(session.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenItemNotFoundOrEmpty_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemorySessionRepository();

        // Act
        var emptyRes = await repository.DeleteAsync(VKSessionId.Empty, CancellationToken.None);
        var notFoundRes = await repository.DeleteAsync(new VKSessionThreadBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKSessionErrors.NotFound);
        notFoundRes.Should().BeFailure(VKSessionErrors.NotFound);
    }
}
