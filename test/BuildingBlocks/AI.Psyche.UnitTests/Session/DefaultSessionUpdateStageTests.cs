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

public sealed class DefaultSessionUpdateStageTests
{
    [Fact]
    public async Task ExecuteAsync_WithSessionInContext_UpdatesTurnCountAndSaves()
    {
        // Arrange
        var storeMock = new Mock<IVKSessionStore>();
        storeMock.Setup(s => s.SaveSessionAsync(It.IsAny<VKSessionThread>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionUpdateStage(options, storeMock.Object, TimeProvider.System);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();
        var session = new VKSessionThread
        {
            Id = new VKSessionId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            UserId = new VKUserId(Guid.NewGuid()),
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            TurnCount = 2
        };
        context.SetState(session);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        storeMock.Verify(s => s.SaveSessionAsync(It.Is<VKSessionThread>(st => st.TurnCount == 3), It.IsAny<CancellationToken>()), Times.Once);
        context.State<VKSessionThread>()!.TurnCount.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIsWeaveOnly_DoesNotUpdateSession()
    {
        // Arrange
        var storeMock = new Mock<IVKSessionStore>();
        var options = new VKSessionOptions { Enabled = true };
        var stage = new DefaultSessionUpdateStage(options, storeMock.Object, TimeProvider.System);

        var request = new VKPsycheRequest
        {
            TenantId = VKTenantId.Default,
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            UserInput = "hello",
            WeaveOnly = true
        };
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        context = new VKPsycheContext
        {
            Request = request,
            Services = context.Services
        };

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        storeMock.Verify(s => s.SaveSessionAsync(It.IsAny<VKSessionThread>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
