using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

public sealed class InMemoryDirectiveStoreTests
{
    [Fact]
    public async Task GetDirectiveAsync_WhenSeededAndTenantMatches_ReturnsSuccess()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);
        var loggerMock = new Mock<ILogger<InMemoryDirectiveStore>>();

        var store = new InMemoryDirectiveStore(identityMock.Object, loggerMock.Object);
        var directiveId = new VKDirectiveId(Guid.NewGuid());
        var directive = new VKDirectiveCharter
        {
            Id = directiveId,
            TenantId = VKTenantId.Default,
            Overview = "Test Charter"
        };
        store.Seed(directive);

        // Act
        var result = await store.GetDirectiveAsync(directiveId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(directive);
    }

    [Fact]
    public async Task GetDirectiveAsync_WhenNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);
        var loggerMock = new Mock<ILogger<InMemoryDirectiveStore>>();

        var store = new InMemoryDirectiveStore(identityMock.Object, loggerMock.Object);

        // Act
        var result = await store.GetDirectiveAsync(new VKDirectiveId(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKDirectiveErrors.NotFound);
    }

    [Fact]
    public async Task GetDirectiveAsync_WhenTenantMismatch_ReturnsNotFoundFailure()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(new VKTenantId(Guid.NewGuid()));
        var loggerMock = new Mock<ILogger<InMemoryDirectiveStore>>();

        var store = new InMemoryDirectiveStore(identityMock.Object, loggerMock.Object);
        var directiveId = new VKDirectiveId(Guid.NewGuid());
        var directive = new VKDirectiveCharter
        {
            Id = directiveId,
            TenantId = VKTenantId.Default,
            Overview = "Test Charter"
        };
        store.Seed(directive);

        // Act
        var result = await store.GetDirectiveAsync(directiveId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKDirectiveErrors.NotFound);
    }

    [Fact]
    public async Task RemoveAndClear_RemovesDirectivesFromStore()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);
        var loggerMock = new Mock<ILogger<InMemoryDirectiveStore>>();

        var store = new InMemoryDirectiveStore(identityMock.Object, loggerMock.Object);
        var id1 = new VKDirectiveId(Guid.NewGuid());
        var id2 = new VKDirectiveId(Guid.NewGuid());
        store.Seed([
            new VKDirectiveCharter { Id = id1, TenantId = VKTenantId.Default, Overview = "1" },
            new VKDirectiveCharter { Id = id2, TenantId = VKTenantId.Default, Overview = "2" }
        ]);

        // Act
        store.Remove(id1);
        var res1 = await store.GetDirectiveAsync(id1, CancellationToken.None);

        store.Clear();
        var res2 = await store.GetDirectiveAsync(id2, CancellationToken.None);

        // Assert
        res1.IsFailure.Should().BeTrue();
        res2.IsFailure.Should().BeTrue();
    }
}
