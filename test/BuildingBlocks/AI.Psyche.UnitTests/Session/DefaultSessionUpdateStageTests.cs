using Moq;
using VK.Blocks.AI.Psyche.Session.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Session;

public sealed class DefaultSessionUpdateStageTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WithSessionInContext_UpdatesTurnCountAndSaves()
    {
        // Arrange
        GetMock<IVKPsycheSessionRepository>()
            .Setup(s => s.UpdateAsync(It.IsAny<VKSessionThread>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionUpdateStage(options, GetMockObject<IVKPsycheSessionRepository>(), TimeProvider.System);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();
        var session = new VKSessionThreadBuilder().Build();
        session.IncrementTurn(DateTimeOffset.UtcNow);
        session.IncrementTurn(DateTimeOffset.UtcNow);
        context.SetState(session);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IVKPsycheSessionRepository>()
            .Verify(s => s.UpdateAsync(It.Is<VKSessionThread>(st => st.TurnCount == 3), It.IsAny<CancellationToken>()), Times.Once);
        context.State<VKSessionThread>()!.TurnCount.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoSessionInContext_ReturnsSuccessWithoutCallingRepo()
    {
        // Arrange
        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionUpdateStage(options, GetMockObject<IVKPsycheSessionRepository>(), TimeProvider.System);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IVKPsycheSessionRepository>()
            .Verify(s => s.UpdateAsync(It.IsAny<VKSessionThread>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIsWeaveOnly_DoesNotUpdateSession()
    {
        // Arrange
        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionUpdateStage(options, GetMockObject<IVKPsycheSessionRepository>(), TimeProvider.System);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithWeaveOnly()
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IVKPsycheSessionRepository>()
            .Verify(s => s.UpdateAsync(It.IsAny<VKSessionThread>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
