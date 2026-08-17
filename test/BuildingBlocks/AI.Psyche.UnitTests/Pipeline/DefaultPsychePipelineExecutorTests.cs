using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Pipeline;
using VK.Blocks.AI.Psyche.Pipeline.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Pipeline;

public sealed class DefaultPsychePipelineExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_WhenEmptyResponseMessages_ReturnsEmptyResponseFailure()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DefaultPsychePipelineExecutor>>();
        var executor = new DefaultPsychePipelineExecutor([], [], loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKPipelineErrors.EmptyResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWeaveOnly_ReturnsSuccessWithoutChatEngine()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DefaultPsychePipelineExecutor>>();
        var executor = new DefaultPsychePipelineExecutor([], [], loggerMock.Object);

        var request = new VKPsycheRequest
        {
            TenantId = VKTenantId.Default,
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            UserInput = "test",
            WeaveOnly = true
        };
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        context = new VKPsycheContext
        {
            Request = request,
            Services = context.Services
        };

        context.Response.Messages.Add(new VKChatMessage { Role = VKChatRole.User, Content = "hello" });

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAborted_ReturnsAbortedFailure()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DefaultPsychePipelineExecutor>>();
        var executor = new DefaultPsychePipelineExecutor([], [], loggerMock.Object);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();
        context.Abort();

        // Act
        var result = await executor.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKPipelineErrors.Aborted);
    }
}
