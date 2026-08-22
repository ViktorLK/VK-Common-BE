using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

/// <summary>
/// Unit tests for the <see cref="DefaultWeavingStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPromptWeavingEngineTests
{
    private static (VKPsycheContext Context, IServiceProvider Services) CreateTestContext(
        string userInput = "hello")
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var request = new VKPsycheRequest
        {
            PersonaIds = [new VKPersonaId(Guid.NewGuid())],
            SessionId = new VKSessionId(Guid.NewGuid()),
            UserInput = userInput
        };

        var context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = Guid.NewGuid().ToString(),
            CreatedAt = DateTimeOffset.UtcNow,
            Services = services
        };

        return (context, services);
    }

    [Fact]
    public async Task ExecuteAsync_HappyPath_ExecutesTasksSuccessfully()
    {
        // Arrange
        var mockTask = new Mock<IVKWeavingPipelineTask>();
        mockTask.SetupGet(t => t.IsActive).Returns(true);
        mockTask.SetupGet(t => t.Schedule).Returns(new VKPipelineSchedule(100));
        mockTask.Setup(t => t.ExecuteAsync(It.IsAny<VKPsycheContext>(), It.IsAny<CancellationToken>()))
            .Callback((VKPsycheContext ctx, CancellationToken _) =>
            {
                ctx.ResponseBuilder.Messages.Add(new VKChatMessage { Role = VKChatRole.System, Content = "System prompt" });
            })
            .Returns(Task.FromResult(VKResult.Success()));

        var options = new VKWeavingOptions();
        var stage = new DefaultWeavingStage(new[] { mockTask.Object }, options);
        var (context, _) = CreateTestContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockTask.Verify(t => t.ExecuteAsync(context, It.IsAny<CancellationToken>()), Times.Once);
    }
}
