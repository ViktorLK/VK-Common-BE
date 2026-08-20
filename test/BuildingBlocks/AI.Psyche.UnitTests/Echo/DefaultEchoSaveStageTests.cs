using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

public sealed class DefaultEchoSaveStageTests
{
    [Fact]
    public async Task ExecuteAsync_WithSessionAndUserResponse_SavesEchoTraces()
    {
        // Arrange
        var storeMock = new Mock<IVKEchoStore>();
        storeMock.Setup(s => s.SaveHistoryAsync(It.IsAny<VKEchoTrace>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        var modelFactoryMock = new Mock<IVKPsycheModelFactory>();
        var sessionId = new VKSessionId(Guid.NewGuid());
        var userTrace = new VKEchoTrace { Id = new VKEchoId(Guid.NewGuid()), SessionId = sessionId, Role = VKChatRole.User, Content = "hello" };
        var assistantTrace = new VKEchoTrace { Id = new VKEchoId(Guid.NewGuid()), SessionId = sessionId, Role = VKChatRole.Assistant, Content = "hi" };

        var options = new VKEchoOptions { Enabled = true, AutoSaveHistory = true };
        var loggerMock = new Mock<ILogger<DefaultEchoSaveStage>>();
        var stage = new DefaultEchoSaveStage(storeMock.Object, modelFactoryMock.Object, options, loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();
        var session = new VKSessionThread
        {
            Id = sessionId
        };
        context.SetState(session);
        context.ResponseBuilder.ChatResponse = new VKChatResponse
        {
            Message = new VKChatMessage { Role = VKChatRole.Assistant, Content = "hi" }
        };

        modelFactoryMock.Setup(m => m.CreateEcho(sessionId, VKChatRole.User, "hello", 0, context.CreatedAt)).Returns(userTrace);
        modelFactoryMock.Setup(m => m.CreateEcho(sessionId, VKChatRole.Assistant, "hi", 0, null)).Returns(assistantTrace);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        storeMock.Verify(s => s.SaveHistoryAsync(userTrace, It.IsAny<CancellationToken>()), Times.Once);
        storeMock.Verify(s => s.SaveHistoryAsync(assistantTrace, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWeaveOnly_DoesNotSaveTraces()
    {
        // Arrange
        var storeMock = new Mock<IVKEchoStore>();
        var modelFactoryMock = new Mock<IVKPsycheModelFactory>();
        var options = new VKEchoOptions { Enabled = true, AutoSaveHistory = true };
        var loggerMock = new Mock<ILogger<DefaultEchoSaveStage>>();
        var stage = new DefaultEchoSaveStage(storeMock.Object, modelFactoryMock.Object, options, loggerMock.Object);

        var request = new VKPsycheRequest
        {
            PersonaIds = [new VKPersonaId(Guid.NewGuid())],
            UserInput = "hello",
            WeaveOnly = true
        };
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = context.Services
        };

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        storeMock.Verify(s => s.SaveHistoryAsync(It.IsAny<VKEchoTrace>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
