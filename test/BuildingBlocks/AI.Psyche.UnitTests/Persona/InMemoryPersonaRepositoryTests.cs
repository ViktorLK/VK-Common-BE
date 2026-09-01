using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

public sealed class InMemoryPersonaRepositoryTests : VKUnitTestBase
{
    [Fact]
    public async Task FindByIdAsync_WhenExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();
        var persona = new VKPersonaAnchorBuilder().WithName("Support Bot").WithDescription("Bot desc").Build();
        repository.Seed(persona);

        // Act
        var result = await repository.FindByIdAsync(persona.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccessWithValue(persona);
    }

    [Fact]
    public async Task FindByIdAsync_WhenEmptyOrNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();

        // Act
        var emptyRes = await repository.FindByIdAsync(VKPersonaId.Empty, CancellationToken.None);
        var notFoundRes = await repository.FindByIdAsync(new VKPersonaAnchorBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKPersonaErrors.NotFound);
        notFoundRes.Should().BeFailure(VKPersonaErrors.NotFound);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenSeeded_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();
        var persona = new VKPersonaAnchorBuilder()
            .WithName("Support Bot")
            .WithDescription("Bot desc")
            .Build();
        repository.Seed(persona);

        // Act
        var result = await repository.ListByIdsAsync([persona.Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().ContainSingle(p => p.Id == persona.Id);
    }

    [Fact]
    public async Task ListByIdsAsync_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();

        // Act
        var result = await repository.ListByIdsAsync([new VKPersonaAnchorBuilder().Build().Id], CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_WhenCalled_ReturnsAllPersonas()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();
        var p1 = new VKPersonaAnchorBuilder().WithName("P1").WithDescription("1").Build();
        var p2 = new VKPersonaAnchorBuilder().WithName("P2").WithDescription("2").Build();
        repository.Seed([p1, p2]);

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
        var repository = new InMemoryPersonaRepository();
        var persona = new VKPersonaAnchorBuilder().WithName("P").WithDescription("D").Build();
        repository.Seed(persona);

        // Act
        var exists = await repository.ExistsAsync(persona.Id, CancellationToken.None);
        var notExists = await repository.ExistsAsync(new VKPersonaAnchorBuilder().Build().Id, CancellationToken.None);
        var emptyExists = await repository.ExistsAsync(VKPersonaId.Empty, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
        emptyExists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WhenItemIsNew_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();
        var item = new VKPersonaAnchorBuilder().WithName("P").WithDescription("D").Build();

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
        var repository = new InMemoryPersonaRepository();
        var item = new VKPersonaAnchorBuilder().WithName("P").WithDescription("D").Build();
        repository.Seed(item);

        // Act
        var result = await repository.AddAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKPersonaErrors.AlreadyExists);
    }

    [Fact]
    public async Task UpdateAsync_WhenItemExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();
        var item = new VKPersonaAnchorBuilder().WithName("Old").WithDescription("D").Build();
        repository.Seed(item);

        var updated = new VKPersonaAnchorBuilder().WithId(item.Id).WithName("New").WithDescription("D").Build();

        // Act
        var result = await repository.UpdateAsync(updated, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var found = await repository.FindByIdAsync(item.Id, CancellationToken.None);
        found.Should().BeSuccess();
        found.Value!.Name.Should().Be("New");
    }

    [Fact]
    public async Task UpdateAsync_WhenItemNotFound_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();
        var item = new VKPersonaAnchorBuilder().WithName("P").WithDescription("D").Build();

        // Act
        var result = await repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKPersonaErrors.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemExists_RemovesAndReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();
        var persona = new VKPersonaAnchorBuilder().WithName("P").WithDescription("D").Build();
        repository.Seed(persona);

        // Act
        var result = await repository.DeleteAsync(persona.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        (await repository.ExistsAsync(persona.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenItemNotFoundOrEmpty_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();

        // Act
        var emptyRes = await repository.DeleteAsync(VKPersonaId.Empty, CancellationToken.None);
        var notFoundRes = await repository.DeleteAsync(new VKPersonaAnchorBuilder().Build().Id, CancellationToken.None);

        // Assert
        emptyRes.Should().BeFailure(VKPersonaErrors.NotFound);
        notFoundRes.Should().BeFailure(VKPersonaErrors.NotFound);
    }

    [Fact]
    public async Task RemoveAndClear_RemovesPersonasFromStore()
    {
        // Arrange
        var repository = new InMemoryPersonaRepository();
        var p1 = new VKPersonaAnchorBuilder().WithName("1").WithDescription("D").Build();
        var p2 = new VKPersonaAnchorBuilder().WithName("2").WithDescription("D").Build();
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
