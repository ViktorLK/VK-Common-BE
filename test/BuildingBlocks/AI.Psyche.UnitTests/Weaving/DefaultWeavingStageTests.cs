using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

public sealed class DefaultWeavingStageTests
{
    private sealed class DummyTask : IVKWeavingPipelineTask
    {
        public VKPipelineSchedule Schedule => new(1);
        public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
        {
            context.Response.Messages.Add(new VKChatMessage { Role = VKChatRole.User, Content = "hello" });
            return Task.FromResult(VKResult.Success());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ExecutesWeavingTasksSuccessfully()
    {
        // Arrange
        var options = new VKWeavingOptions();
        var tasks = new List<IVKWeavingPipelineTask> { new DummyTask() };
        var stage = new DefaultWeavingStage(tasks, options);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Response.Messages.Should().ContainSingle();
    }
}
