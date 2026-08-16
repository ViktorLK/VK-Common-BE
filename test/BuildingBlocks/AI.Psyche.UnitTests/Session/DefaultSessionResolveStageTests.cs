using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Session.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Session;

public sealed class DefaultSessionResolveStageTests
{
    [Fact]
    public async Task ExecuteAsync_WithActiveSessionId_AttachesSessionState()
    {
        // Arrange
        var storeMock = new Mock<IVKSessionStore>();
        var sessionId = new VKSessionId(Guid.NewGuid());
        var session = new VKSessionThread
        {
            Id = sessionId,
            TenantId = VKTenantId.Default,
            UserId = new VKUserId(Guid.NewGuid()),
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            Status = VKSessionStatus.Active
        };
        storeMock.Setup(s => s.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<VKSessionThread?>(session));

        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionResolveStage(options, storeMock.Object);
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithSessionId(sessionId)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.State<VKSessionThread>().Should().Be(session);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSessionInactive_ReturnsFailure()
    {
        // Arrange
        var storeMock = new Mock<IVKSessionStore>();
        var sessionId = new VKSessionId(Guid.NewGuid());
        var session = new VKSessionThread
        {
            Id = sessionId,
            TenantId = VKTenantId.Default,
            UserId = new VKUserId(Guid.NewGuid()),
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            Status = VKSessionStatus.Closed
        };
        storeMock.Setup(s => s.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<VKSessionThread?>(session));

        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionResolveStage(options, storeMock.Object);
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithSessionId(sessionId)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKSessionErrors.SessionNotActive);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptySessionId_ReturnsSuccessWithoutState()
    {
        // Arrange
        var storeMock = new Mock<IVKSessionStore>();
        storeMock.Setup(s => s.GetSessionAsync(It.IsAny<VKSessionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<VKSessionThread?>(VKSessionErrors.NotFound));

        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionResolveStage(options, storeMock.Object);
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.State<VKSessionThread>().Should().BeNull();
    }
}
