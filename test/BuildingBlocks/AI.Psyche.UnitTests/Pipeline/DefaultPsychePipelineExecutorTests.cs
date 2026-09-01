using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Pipeline.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Pipeline;

public sealed class DefaultPsychePipelineExecutorTests : VKUnitTestBase
{
    private sealed class TestMiddleware(Action onExecuted) : IVKPsycheMiddleware
    {
        public VKPipelineSchedule Schedule => new(1);
        public bool IsActive => true;

        public async Task<VKResult> InvokeAsync(VKPsycheContext context, VKPipelineDelegate next, CancellationToken cancellationToken)
        {
            onExecuted();
            return await next();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenChatEngineFails_ReturnsFailure()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var chatEngineMock = GetMock<IVKChatEngine>();
        chatEngineMock
            .Setup(c => c.SendAsync(It.IsAny<IEnumerable<VKChatMessage>>(), It.IsAny<VKChatArgs?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<VKChatResponse>(new VKError("Chat.Failed", "Chat failed")));

        var services = new ServiceCollection()
            .AddSingleton<IVKChatEngine>(chatEngineMock.Object)
            .BuildServiceProvider();

        var executor = new DefaultPsychePipelineExecutor([], [], loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .BuildContext();

        context = new VKPsycheContext
        {
            Request = context.Request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = services
        };

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure("Chat.Failed");
    }

    [Fact]
    public async Task ExecuteAsync_WhenWeaveOnly_ReturnsSuccessWithoutChatEngine()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var executor = new DefaultPsychePipelineExecutor([], [], loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .WithWeaveOnly()
            .BuildContext();

        context.ResponseBuilder.Messages.Add(new VKChatMessage { Role = VKChatRole.User, Content = "hello" });
        context.Complete();

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAborted_ReturnsAbortedFailure()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var executor = new DefaultPsychePipelineExecutor([], [], loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();
        context.Abort();

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKPipelineErrors.Aborted);
    }

    [Fact]
    public async Task ExecuteAsync_WhenChatEngineMissing_ReturnsChatEngineNotFoundError()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var services = new ServiceCollection().BuildServiceProvider();
        var executor = new DefaultPsychePipelineExecutor([], [], loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();
        context = new VKPsycheContext
        {
            Request = context.Request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = services
        };

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKPipelineErrors.ChatEngineNotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllStagesSucceed_ReturnsCombinedResult()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var chatEngineMock = GetMock<IVKChatEngine>();
        var response = new VKChatResponse
        {
            Message = new VKChatMessage { Role = VKChatRole.Assistant, Content = "Response text" },
            Usage = new VKAITokenUsage { InputTokens = 10, OutputTokens = 20 }
        };

        chatEngineMock
            .Setup(c => c.SendAsync(It.IsAny<IEnumerable<VKChatMessage>>(), It.IsAny<VKChatArgs?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(response));

        var services = new ServiceCollection()
            .AddSingleton<IVKChatEngine>(chatEngineMock.Object)
            .BuildServiceProvider();

        var stageMock = GetMock<IVKPsychePipelineStage>();
        stageMock.SetupGet(s => s.Schedule).Returns(new VKPipelineSchedule(100));
        stageMock.SetupGet(s => s.IsActive).Returns(true);
        stageMock
            .Setup(s => s.ExecuteAsync(It.IsAny<VKPsycheContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        var executor = new DefaultPsychePipelineExecutor([stageMock.Object], [], loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .BuildContext();

        context = new VKPsycheContext
        {
            Request = context.Request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = services
        };

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value!.ChatResponse.Should().NotBeNull();
        result.Value!.ChatResponse!.Message.Content.Should().Be("Response text");
        result.Value!.Usage.Should().NotBeNull();
        result.Value!.Usage!.TotalTokens.Should().Be(30);
    }

    [Fact]
    public async Task ExecuteAsync_WhenChatEngineReturnsNoUsage_SucceedsWithNullUsage()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var chatEngineMock = GetMock<IVKChatEngine>();
        var response = new VKChatResponse
        {
            Message = new VKChatMessage { Role = VKChatRole.Assistant, Content = "Response without usage" },
            Usage = null
        };

        chatEngineMock
            .Setup(c => c.SendAsync(It.IsAny<IEnumerable<VKChatMessage>>(), It.IsAny<VKChatArgs?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(response));

        var services = new ServiceCollection()
            .AddSingleton<IVKChatEngine>(chatEngineMock.Object)
            .BuildServiceProvider();

        var executor = new DefaultPsychePipelineExecutor([], [], loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hi").BuildContext();
        context = new VKPsycheContext
        {
            Request = context.Request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = services
        };

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value!.Usage.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMiddlewaresPresent_ExecutesMiddlewaresInOrder()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var chatEngineMock = GetMock<IVKChatEngine>();
        var response = new VKChatResponse
        {
            Message = new VKChatMessage { Role = VKChatRole.Assistant, Content = "Hi" }
        };

        chatEngineMock
            .Setup(c => c.SendAsync(It.IsAny<IEnumerable<VKChatMessage>>(), It.IsAny<VKChatArgs?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(response));

        var services = new ServiceCollection()
            .AddSingleton<IVKChatEngine>(chatEngineMock.Object)
            .BuildServiceProvider();

        bool middlewareExecuted = false;
        var middleware = new TestMiddleware(() => middlewareExecuted = true);

        var executor = new DefaultPsychePipelineExecutor([], [middleware], loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hi").BuildContext();
        context = new VKPsycheContext
        {
            Request = context.Request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = services
        };

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        middlewareExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenStageFails_ReturnsFailure()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var stageMock = GetMock<IVKPsychePipelineStage>();
        stageMock.SetupGet(s => s.Schedule).Returns(new VKPipelineSchedule(100));
        stageMock.SetupGet(s => s.IsActive).Returns(true);
        stageMock
            .Setup(s => s.ExecuteAsync(It.IsAny<VKPsycheContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure(new VKError("Stage.Failed", "Stage error")));

        var executor = new DefaultPsychePipelineExecutor([stageMock.Object], [], loggerMock.Object);
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure("Stage.Failed");
    }

    [Fact]
    public async Task ExecuteAsync_WhenStageThrows_RethrowsException()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var stageMock = GetMock<IVKPsychePipelineStage>();
        stageMock.SetupGet(s => s.Schedule).Returns(new VKPipelineSchedule(100));
        stageMock.SetupGet(s => s.IsActive).Returns(true);
        stageMock
            .Setup(s => s.ExecuteAsync(It.IsAny<VKPsycheContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Stage boom"));

        var executor = new DefaultPsychePipelineExecutor([stageMock.Object], [], loggerMock.Object);
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        Func<Task> act = async () => await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Stage boom");
    }

    [Fact]
    public async Task ExecuteAsync_WhenChatEngineThrows_RethrowsException()
    {
        // Arrange
        var loggerMock = GetMock<ILogger<DefaultPsychePipelineExecutor>>();
        var chatEngineMock = GetMock<IVKChatEngine>();
        chatEngineMock
            .Setup(c => c.SendAsync(It.IsAny<IEnumerable<VKChatMessage>>(), It.IsAny<VKChatArgs?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("LLM network boom"));

        var services = new ServiceCollection()
            .AddSingleton<IVKChatEngine>(chatEngineMock.Object)
            .BuildServiceProvider();

        var executor = new DefaultPsychePipelineExecutor([], [], loggerMock.Object);
        var request = new VKPsycheRequest { UserInput = "test" }.WithArgs(new VKChatArgs { ModelId = "gpt-4o" });
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();
        context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = services
        };

        // Act
        Func<Task> act = async () => await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("LLM network boom");
    }
}
