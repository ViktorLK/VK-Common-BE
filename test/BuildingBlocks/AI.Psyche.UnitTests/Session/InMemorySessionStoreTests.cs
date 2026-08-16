using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Session.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Session;

public sealed class InMemorySessionStoreTests
{
    [Fact]
    public async Task GetSessionAsync_WhenSeededAndIdentityMatches_ReturnsSession()
    {
        // Arrange
        var userId = new VKUserId(Guid.NewGuid());
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);
        identityMock.SetupGet(i => i.UserId).Returns(userId);

        var store = new InMemorySessionStore(identityMock.Object);
        var sessionId = new VKSessionId(Guid.NewGuid());
        var session = new VKSessionThread
        {
            Id = sessionId,
            TenantId = VKTenantId.Default,
            UserId = userId,
            PersonaId = new VKPersonaId(Guid.NewGuid())
        };
        store.Seed(session);

        // Act
        var result = await store.GetSessionAsync(sessionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(session);
    }

    [Fact]
    public async Task GetSessionAsync_WhenIdentityMismatch_ReturnsNull()
    {
        // Arrange
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);
        identityMock.SetupGet(i => i.UserId).Returns(new VKUserId(Guid.NewGuid()));

        var store = new InMemorySessionStore(identityMock.Object);
        var sessionId = new VKSessionId(Guid.NewGuid());
        var session = new VKSessionThread
        {
            Id = sessionId,
            TenantId = VKTenantId.Default,
            UserId = new VKUserId(Guid.NewGuid()),
            PersonaId = new VKPersonaId(Guid.NewGuid())
        };
        store.Seed(session);

        // Act
        var result = await store.GetSessionAsync(sessionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Action act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveSessionAsync_SavesSessionSuccessfully()
    {
        // Arrange
        var userId = new VKUserId(Guid.NewGuid());
        var identityMock = new Mock<IVKIdentityContext>();
        identityMock.SetupGet(i => i.TenantId).Returns(VKTenantId.Default);
        identityMock.SetupGet(i => i.UserId).Returns(userId);

        var store = new InMemorySessionStore(identityMock.Object);
        var sessionId = new VKSessionId(Guid.NewGuid());
        var session = new VKSessionThread
        {
            Id = sessionId,
            TenantId = VKTenantId.Default,
            UserId = userId,
            PersonaId = new VKPersonaId(Guid.NewGuid())
        };

        // Act
        var saveResult = await store.SaveSessionAsync(session, CancellationToken.None);
        var getResult = await store.GetSessionAsync(sessionId, CancellationToken.None);

        // Assert
        saveResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().Be(session);
    }
}
