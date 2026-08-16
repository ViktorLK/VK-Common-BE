using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Psyche.Pipeline.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Pipeline;

/// <summary>
/// Unit tests for the <see cref="DefaultPsychePipeline"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPsychePipelineTests
{
    [Fact]
    public async Task RunAsync_HappyPath_DelegatesToExecutorAndReturnsSuccess()
    {
        // Arrange
        var mockExecutor = new Mock<IVKPsychePipelineExecutor>();
        var mockServices = new Mock<IServiceProvider>();
        var mockGuidGenerator = new Mock<IVKGuidGenerator>();
        var mockLogger = new Mock<ILogger<DefaultPsychePipeline>>();

        var generatedGuid = Guid.NewGuid();
        mockGuidGenerator.Setup(g => g.Create()).Returns(generatedGuid);

        var expectedResponse = new VKPsycheResponse
        {
            Messages = [],
            ChatResponse = null,
            Usage = null
        };

        mockExecutor
            .Setup(e => e.ExecuteAsync(It.IsAny<VKPsycheContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(expectedResponse));

        var pipeline = new DefaultPsychePipeline(
            mockExecutor.Object,
            mockGuidGenerator.Object,
            mockLogger.Object,
            mockServices.Object);

        var request = new VKPsycheRequest
        {
            TenantId = VKTenantId.Default,
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            SessionId = new VKSessionId(Guid.NewGuid()),
            UserInput = "hello"
        };

        // Act
        var result = await pipeline.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedResponse);

        mockExecutor.Verify(e => e.ExecuteAsync(
            It.Is<VKPsycheContext>(ctx =>
                ctx.Request.CorrelationId == generatedGuid.ToString() &&
                ctx.Services == mockServices.Object),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
