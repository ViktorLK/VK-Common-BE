using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

public sealed class DefaultKnowledgeFinalizerStageTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WithCandidatesState_AddsKnowledgeFragmentsToContext()
    {
        // Arrange
        var stage = new DefaultKnowledgeFinalizerStage();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        var entry1 = new VKKnowledgeEntryBuilder()
            .WithContent("Doc 1")
            .Build();
        var entry2 = new VKKnowledgeEntryBuilder()
            .WithContent("Doc 2")
            .Build();

        var state = new VKKnowledgeCandidatesState();
        state.Candidates.Add(entry1);
        state.Candidates.Add(entry2);
        context.SetState(state);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().HaveCount(2);
        context.Fragments.Should().OnlyContain(f => f.TierType == VKPromptTierType.Knowledge);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutCandidatesState_DoesNotAddFragments()
    {
        // Arrange
        var stage = new DefaultKnowledgeFinalizerStage();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().BeEmpty();
    }
}
