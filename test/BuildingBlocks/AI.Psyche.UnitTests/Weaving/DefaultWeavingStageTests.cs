using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

public sealed class DefaultWeavingStageTests : VKUnitTestBase
{
    private sealed class DummyTask : IVKWeavingPipelineTask
    {
        public VKPipelineSchedule Schedule => new(1);
        public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
        {
            context.ResponseBuilder.Messages.Add(new VKChatMessage { Role = VKChatRole.User, Content = "hello" });
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
        result.Should().BeSuccess();
        context.ResponseBuilder.Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoMessagesGenerated_ReturnsNoTapestryError()
    {
        // Arrange
        var options = new VKWeavingOptions();
        var stage = new DefaultWeavingStage([], options);
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKWeavingErrors.NoTapestry);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWeaveOnly_MarksContextCompleted()
    {
        // Arrange
        var options = new VKWeavingOptions();
        var tasks = new List<IVKWeavingPipelineTask> { new DummyTask() };
        var stage = new DefaultWeavingStage(tasks, options);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithWeaveOnly()
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabledTiersConfigured_PrunesDisabledTierFragments()
    {
        // Arrange
        var options = new VKWeavingOptions
        {
            DisabledTiers = [VKPromptTierType.Directive]
        };
        var tasks = new List<IVKWeavingPipelineTask> { new DummyTask() };
        var stage = new DefaultWeavingStage(tasks, options);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();
        context.SetFragments([
            new VKPromptFragment
            {
                TierType = VKPromptTierType.Directive,
                Metadata = new VKDirectiveCharterBuilder().Build(),
                Segment = new VKPromptSegment { Content = "Directive" }
            },
            new VKPromptFragment
            {
                TierType = VKPromptTierType.Persona,
                Metadata = new VKPersonaAnchorBuilder().WithName("Persona").WithDescription("Persona Description").Build(),
                Segment = new VKPromptSegment { Content = "Persona" }
            }
        ]);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Persona);
    }
}
