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
        storeMock.Setup(s => s.SaveHistoryBatchAsync(It.IsAny<IReadOnlyCollection<VKEchoTrace>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        var modelFactoryMock = GetMock<IVKPsycheModelFactory>();
        var session = new VKSessionThreadBuilder().Build();
        var userTrace = new VKEchoTraceBuilder().WithSessionId(session.Id).WithRole(VKChatRole.User).WithContent("hello").Build();
        var assistantTrace = new VKEchoTraceBuilder().WithSessionId(session.Id).WithRole(VKChatRole.Assistant).WithContent("hi").Build();

        var options = new VKEchoOptions { Enabled = true, AutoSaveHistory = true };
        var loggerMock = GetMock<ILogger<DefaultEchoSaveStage>>();
        var tokenCounterMock = GetMock<IVKTokenCounter>();
        var stage = new DefaultEchoSaveStage(storeMock.Object, modelFactoryMock.Object, tokenCounterMock.Object, options, loggerMock.Object);

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
        storeMock.Verify(s => s.SaveHistoryBatchAsync(It.Is<IReadOnlyCollection<VKEchoTrace>>(c => c.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreFails_ReturnsFailure()
    {
        // Arrange
        var storeMock = GetMock<IVKEchoStore>();
        storeMock.Setup(s => s.SaveHistoryBatchAsync(It.IsAny<IReadOnlyCollection<VKEchoTrace>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure(new VKError("Echo.SaveFailed", "Failed to save echo history")));

        var modelFactoryMock = GetMock<IVKPsycheModelFactory>();
        var session = new VKSessionThreadBuilder().Build();
        var userTrace = new VKEchoTraceBuilder().WithSessionId(session.Id).WithRole(VKChatRole.User).WithContent("hello").Build();

        var options = new VKEchoOptions { Enabled = true, AutoSaveHistory = true };
        var loggerMock = GetMock<ILogger<DefaultEchoSaveStage>>();
        var tokenCounterMock = GetMock<IVKTokenCounter>();
        var stage = new DefaultEchoSaveStage(storeMock.Object, modelFactoryMock.Object, tokenCounterMock.Object, options, loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();
        context.SetState(session);

        modelFactoryMock.Setup(m => m.CreateEcho(session.Id, VKChatRole.User, "hello", 0, context.CreatedAt)).Returns(userTrace);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.FirstError.Code.Should().Be("Echo.SaveFailed");
    }

    [Fact]
    public async Task ExecuteAsync_WhenWeaveOnly_DoesNotSaveTraces()
    {
        // Arrange
        var storeMock = GetMock<IVKEchoStore>();
        var modelFactoryMock = GetMock<IVKPsycheModelFactory>();
        var tokenCounterMock = GetMock<IVKTokenCounter>();
        var options = new VKEchoOptions { Enabled = true, AutoSaveHistory = true };
        var loggerMock = GetMock<ILogger<DefaultEchoSaveStage>>();
        var stage = new DefaultEchoSaveStage(storeMock.Object, modelFactoryMock.Object, tokenCounterMock.Object, options, loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithWeaveOnly()
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        storeMock.Verify(s => s.SaveHistoryBatchAsync(It.IsAny<IReadOnlyCollection<VKEchoTrace>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithAssistantResponse_CalculatesTokensUsingTokenCounter()
    {
        // Arrange
        var storeMock = GetMock<IVKEchoStore>();
        storeMock.Setup(s => s.SaveHistoryBatchAsync(It.IsAny<IReadOnlyCollection<VKEchoTrace>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        var modelFactoryMock = GetMock<IVKPsycheModelFactory>();
        var session = new VKSessionThreadBuilder().Build();
        var assistantTrace = new VKEchoTraceBuilder().WithSessionId(session.Id).WithRole(VKChatRole.Assistant).WithContent("assistant response").Build();

        var options = new VKEchoOptions { Enabled = true, AutoSaveHistory = true };
        var loggerMock = GetMock<ILogger<DefaultEchoSaveStage>>();
        var tokenCounterMock = GetMock<IVKTokenCounter>();
        tokenCounterMock.Setup(tc => tc.CountTokens("assistant response")).Returns(15);

        var stage = new DefaultEchoSaveStage(storeMock.Object, modelFactoryMock.Object, tokenCounterMock.Object, options, loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        context.SetState(session);
        context.ResponseBuilder.ChatResponse = new VKChatResponse
        {
            Message = new VKChatMessage { Role = VKChatRole.Assistant, Content = "assistant response" },
            Usage = new VKAITokenUsage { InputTokens = 100, OutputTokens = 2000 }
        };

        modelFactoryMock.Setup(m => m.CreateEcho(session.Id, VKChatRole.Assistant, "assistant response", 15, null)).Returns(assistantTrace);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        // Verifies that tokenCount was calculated from _tokenCounter (15) rather than raw Usage.OutputTokens (2000)
        modelFactoryMock.Verify(m => m.CreateEcho(session.Id, VKChatRole.Assistant, "assistant response", 15, null), Times.Once);
    }
}
