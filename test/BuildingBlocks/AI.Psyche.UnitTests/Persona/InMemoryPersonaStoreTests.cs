using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Persona;

public sealed class InMemoryPersonaStoreTests
{
    [Fact]
    public async Task GetPersonaAsync_WhenSeededAndTenantMatches_ReturnsSuccess()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var store = new InMemoryPersonaStore(identityMock.Object);
        var personaId = new VKPersonaId(Guid.NewGuid());
        var persona = new VKPersonaAnchor
        {
            Id = personaId,
            TenantId = VKTenantId.Default,
            Name = "Support Bot",
            Description = "Bot desc"
        };
        store.Seed(persona);

        // Act
        var result = await store.GetPersonaAsync(personaId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(persona);
    }

    [Fact]
    public async Task GetPersonaAsync_WhenNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var store = new InMemoryPersonaStore(identityMock.Object);

        // Act
        var result = await store.GetPersonaAsync(new VKPersonaId(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKPersonaErrors.NotFound);
    }

    [Fact]
    public async Task GetPersonaAsync_WhenTenantMismatch_ReturnsNotFoundFailure()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(new VKTenantId(Guid.NewGuid()));

        var store = new InMemoryPersonaStore(identityMock.Object);
        var personaId = new VKPersonaId(Guid.NewGuid());
        var persona = new VKPersonaAnchor
        {
            Id = personaId,
            TenantId = VKTenantId.Default,
            Name = "Support Bot",
            Description = "Bot desc"
        };
        store.Seed(persona);

        // Act
        var result = await store.GetPersonaAsync(personaId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKPersonaErrors.NotFound);
    }

    [Fact]
    public async Task RemoveAndClear_RemovesPersonasFromStore()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);

        var store = new InMemoryPersonaStore(identityMock.Object);
        var id1 = new VKPersonaId(Guid.NewGuid());
        var id2 = new VKPersonaId(Guid.NewGuid());
        store.Seed([
            new VKPersonaAnchor { Id = id1, TenantId = VKTenantId.Default, Name = "P1", Description = "1" },
            new VKPersonaAnchor { Id = id2, TenantId = VKTenantId.Default, Name = "P2", Description = "2" }
        ]);

        // Act
        store.Remove(id1);
        var res1 = await store.GetPersonaAsync(id1, CancellationToken.None);

        store.Clear();
        var res2 = await store.GetPersonaAsync(id2, CancellationToken.None);

        // Assert
        res1.IsFailure.Should().BeTrue();
        res2.IsFailure.Should().BeTrue();
    }
}
