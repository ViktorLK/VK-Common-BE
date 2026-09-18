using Moq;
using VK.Blocks.AI.Psyche.Session.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Session;

public sealed class DefaultSessionResolveStageTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WithActiveSessionId_AttachesSessionState()
    {
        // Arrange
        var repoMock = GetMock<IVKPsycheSessionRepository>();
        var session = new VKSessionThreadBuilder().Build();
        repoMock.Setup(s => s.FindByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(session));

        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionResolveStage(options, repoMock.Object);
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithSessionId(session.Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.State<VKSessionThread>().Should().Be(session);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSessionInactive_ReturnsFailure()
    {
        // Arrange
        var repoMock = GetMock<IVKPsycheSessionRepository>();
        var session = new VKSessionThreadBuilder().Build();
        session.Close(DateTimeOffset.UtcNow);
        repoMock.Setup(s => s.FindByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(session));

        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionResolveStage(options, repoMock.Object);
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithSessionId(session.Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKSessionErrors.SessionNotActive);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptySessionId_ReturnsSuccessWithoutState()
    {
        // Arrange
        var repoMock = GetMock<IVKPsycheSessionRepository>();
        repoMock.Setup(s => s.FindByIdAsync(It.IsAny<VKSessionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<VKSessionThread>(VKSessionErrors.NotFound));

        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionResolveStage(options, repoMock.Object);
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithSessionId(VKSessionId.Empty)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.State<VKSessionThread>().Should().BeNull();
        repoMock.Verify(s => s.FindByIdAsync(It.IsAny<VKSessionId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExplicitSessionNotFound_ReturnsFailure()
    {
        // Arrange
        var missingSessionId = new VKSessionId(Guid.NewGuid());
        var repoMock = GetMock<IVKPsycheSessionRepository>();
        repoMock.Setup(s => s.FindByIdAsync(missingSessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<VKSessionThread>(VKSessionErrors.NotFound));

        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionResolveStage(options, repoMock.Object);
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithSessionId(missingSessionId)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKSessionErrors.NotFound);
        context.State<VKSessionThread>().Should().BeNull();
    }
}
