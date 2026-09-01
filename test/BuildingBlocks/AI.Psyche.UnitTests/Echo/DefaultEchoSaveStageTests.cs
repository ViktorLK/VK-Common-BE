using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Echo;

public sealed class DefaultEchoSaveStageTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WithSessionAndUserResponse_SavesEchoTraces()
    {
        // Arrange
        var storeMock = GetMock<IVKEchoStore>();
        storeMock.Setup(s => s.SaveHistoryAsync(It.IsAny<VKEchoTrace>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        var modelFactoryMock = GetMock<IVKPsycheModelFactory>();
        var session = new VKSessionThreadBuilder().Build();
        var userTrace = new VKEchoTraceBuilder().WithSessionId(session.Id).WithRole(VKChatRole.User).WithContent("hello").Build();
        var assistantTrace = new VKEchoTraceBuilder().WithSessionId(session.Id).WithRole(VKChatRole.Assistant).WithContent("hi").Build();

        var options = new VKEchoOptions { Enabled = true, AutoSaveHistory = true };
        var loggerMock = GetMock<ILogger<DefaultEchoSaveStage>>();
        var stage = new DefaultEchoSaveStage(storeMock.Object, modelFactoryMock.Object, options, loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();
        context.SetState(session);
        context.ResponseBuilder.ChatResponse = new VKChatResponse
        {
            Message = new VKChatMessage { Role = VKChatRole.Assistant, Content = "hi" }
        };

        modelFactoryMock.Setup(m => m.CreateEcho(session.Id, VKChatRole.User, "hello", 0, context.CreatedAt)).Returns(userTrace);
        modelFactoryMock.Setup(m => m.CreateEcho(session.Id, VKChatRole.Assistant, "hi", 0, null)).Returns(assistantTrace);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        storeMock.Verify(s => s.SaveHistoryAsync(userTrace, It.IsAny<CancellationToken>()), Times.Once);
        storeMock.Verify(s => s.SaveHistoryAsync(assistantTrace, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWeaveOnly_DoesNotSaveTraces()
    {
        // Arrange
        var storeMock = GetMock<IVKEchoStore>();
        var modelFactoryMock = GetMock<IVKPsycheModelFactory>();
        var options = new VKEchoOptions { Enabled = true, AutoSaveHistory = true };
        var loggerMock = GetMock<ILogger<DefaultEchoSaveStage>>();
        var stage = new DefaultEchoSaveStage(storeMock.Object, modelFactoryMock.Object, options, loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithWeaveOnly()
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        storeMock.Verify(s => s.SaveHistoryAsync(It.IsAny<VKEchoTrace>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
