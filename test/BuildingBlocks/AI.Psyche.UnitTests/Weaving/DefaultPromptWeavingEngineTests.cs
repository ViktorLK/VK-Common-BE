using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

/// <summary>
/// Unit tests for the <see cref="DefaultPromptWeavingEngine"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPromptWeavingEngineTests
{
    [Fact]
    public async Task WeavePromptAsync_HappyPath_ExecutesTasksAndReturnsTapestry()
    {
        // Arrange
        var mockTapestry = new VKPromptTapestry
        {
            SystemInstructions = "Mocked System Prompt",
            Messages = new List<VKChatMessage>(),
            TotalEstimatedTokens = 100
        };

        var mockTask = new Mock<IVKWeavingTask>();
        mockTask.SetupGet(t => t.TaskOrder).Returns(100);
        mockTask.SetupGet(t => t.IsParallel).Returns(false);
        mockTask.Setup(t => t.ExecuteAsync(It.IsAny<VKWeavingContext>(), It.IsAny<CancellationToken>()))
            .Callback<VKWeavingContext, CancellationToken>((ctx, ct) => ctx.Tapestry = mockTapestry)
            .ReturnsAsync(VKResult.Success());

        var optionsMock = new Mock<IOptions<VKWeavingOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions());

        var engine = new DefaultPromptWeavingEngine(new[] { mockTask.Object }, optionsMock.Object);

        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "hello",
            CorrelationId = "test-correlation"
        };

        // Act
        var result = await engine.WeavePromptAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(mockTapestry);
        mockTask.Verify(t => t.ExecuteAsync(context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WeavePromptAsync_WhenNoTapestryProduced_ReturnsWeavingNoTapestryError()
    {
        // Arrange
        var mockTask = new Mock<IVKWeavingTask>();
        mockTask.SetupGet(t => t.TaskOrder).Returns(100);
        mockTask.SetupGet(t => t.IsParallel).Returns(false);
        mockTask.Setup(t => t.ExecuteAsync(It.IsAny<VKWeavingContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success()); // Success but doesn't set context.Tapestry

        var optionsMock = new Mock<IOptions<VKWeavingOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new VKWeavingOptions());

        var engine = new DefaultPromptWeavingEngine(new[] { mockTask.Object }, optionsMock.Object);

        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "hello",
            CorrelationId = "test-correlation"
        };

        // Act
        var result = await engine.WeavePromptAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Code.Should().Be(VKWeavingErrors.NoTapestry.Code);
    }
}
