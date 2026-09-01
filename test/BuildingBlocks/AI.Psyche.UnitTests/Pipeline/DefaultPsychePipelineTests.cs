using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Pipeline.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Pipeline;

/// <summary>
/// Unit tests for the <see cref="DefaultPsychePipeline"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPsychePipelineTests : VKUnitTestBase
{
    [Fact]
    public async Task RunAsync_HappyPath_DelegatesToExecutorAndReturnsSuccess()
    {
        // Arrange
        var mockExecutor = GetMock<IVKPsychePipelineExecutor>();
        var mockServices = GetMock<IServiceProvider>();
        var mockGuidGenerator = GetMock<IVKGuidGenerator>();
        var mockLogger = GetMock<ILogger<DefaultPsychePipeline>>();

        var generatedGuid = Guid.NewGuid();
        mockGuidGenerator.Setup(g => g.Create()).Returns(generatedGuid);

        var expectedResponse = new VKPsycheResponse
        {
            Messages = [],
            ChatResponse = null,
            Usage = null,
            CorrelationId = generatedGuid.ToString()
        };

        mockExecutor
            .Setup(e => e.ExecuteAsync(It.IsAny<VKPsycheContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(expectedResponse));

        var pipeline = new DefaultPsychePipeline(
            mockExecutor.Object,
            mockGuidGenerator.Object,
            TimeProvider.System,
            mockLogger.Object,
            mockServices.Object);

        var request = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .Build();

        // Act
        var result = await pipeline.ExecuteAsync(request);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().Be(expectedResponse);

        mockExecutor.Verify(e => e.ExecuteAsync(
            It.Is<VKPsycheContext>(ctx =>
                ctx.CorrelationId == generatedGuid.ToString() &&
                ctx.Services == mockServices.Object),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
