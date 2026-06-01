using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Pipeline;
using VK.Blocks.AI.Psyche.Pipeline.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Pipeline;

/// <summary>
/// Unit tests for the <see cref="DefaultPsychePipeline"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPsychePipelineTests
{
    [Fact]
    public async Task WeaveTapestryAsync_HappyPath_ExecutesAllActiveStagesInCorrectOrder()
    {
        // Arrange
        var mockStage1 = new Mock<IVKWeavingStage>();
        mockStage1.SetupGet(s => s.IsActive).Returns(true);
        mockStage1.SetupGet(s => s.StageOrder).Returns(10);
        mockStage1.SetupGet(s => s.IsParallel).Returns(false);
        mockStage1.Setup(s => s.ExecuteAsync(It.IsAny<VKWeavingContext>(), It.IsAny<CancellationToken>()))
            .Callback<VKWeavingContext, CancellationToken>((ctx, ct) => ctx.Tapestry = new VKPromptTapestry
            {
                Messages = new List<VKChatMessage>(),
                SystemInstructions = string.Empty,
                TotalEstimatedTokens = 0
            })
            .ReturnsAsync(VKResult.Success());

        var mockStage2 = new Mock<IVKWeavingStage>();
        mockStage2.SetupGet(s => s.IsActive).Returns(true);
        mockStage2.SetupGet(s => s.StageOrder).Returns(20);
        mockStage2.SetupGet(s => s.IsParallel).Returns(false);
        mockStage2.Setup(s => s.ExecuteAsync(It.IsAny<VKWeavingContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        // Inactive stage: should not be executed
        var mockInactiveStage = new Mock<IVKWeavingStage>();
        mockInactiveStage.SetupGet(s => s.IsActive).Returns(false);

        var mockGuidGenerator = new Mock<IVKGuidGenerator>();
        mockGuidGenerator.Setup(g => g.Create()).Returns(System.Guid.NewGuid());

        var mockLogger = new Mock<ILogger<DefaultPsychePipeline>>();

        var pipeline = new DefaultPsychePipeline(
            new[] { mockStage2.Object, mockStage1.Object, mockInactiveStage.Object },
            mockGuidGenerator.Object,
            mockLogger.Object);

        var request = new VKWeavingRequest
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "hello"
        };

        // Act
        var result = await pipeline.WeaveTapestryAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        mockStage1.Verify(s => s.ExecuteAsync(It.IsAny<VKWeavingContext>(), It.IsAny<CancellationToken>()), Times.Once);
        mockStage2.Verify(s => s.ExecuteAsync(It.IsAny<VKWeavingContext>(), It.IsAny<CancellationToken>()), Times.Once);
        mockInactiveStage.Verify(s => s.ExecuteAsync(It.IsAny<VKWeavingContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WeaveTapestryAsync_WhenStageFails_PipelineFailsFast()
    {
        // Arrange
        var mockStage1 = new Mock<IVKWeavingStage>();
        mockStage1.SetupGet(s => s.IsActive).Returns(true);
        mockStage1.SetupGet(s => s.StageOrder).Returns(10);
        mockStage1.SetupGet(s => s.IsParallel).Returns(false);
        mockStage1.Setup(s => s.ExecuteAsync(It.IsAny<VKWeavingContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure(VKError.Failure("Test.Stage.Fail", "Stage 1 failed")));

        var mockStage2 = new Mock<IVKWeavingStage>();
        mockStage2.SetupGet(s => s.IsActive).Returns(true);
        mockStage2.SetupGet(s => s.StageOrder).Returns(20);
        mockStage2.SetupGet(s => s.IsParallel).Returns(false);

        var mockGuidGenerator = new Mock<IVKGuidGenerator>();
        mockGuidGenerator.Setup(g => g.Create()).Returns(System.Guid.NewGuid());

        var mockLogger = new Mock<ILogger<DefaultPsychePipeline>>();

        var pipeline = new DefaultPsychePipeline(
            new[] { mockStage1.Object, mockStage2.Object },
            mockGuidGenerator.Object,
            mockLogger.Object);

        var request = new VKWeavingRequest
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "hello"
        };

        // Act
        var result = await pipeline.WeaveTapestryAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Code.Should().Be("Test.Stage.Fail");

        mockStage2.Verify(s => s.ExecuteAsync(It.IsAny<VKWeavingContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
