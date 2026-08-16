using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

public sealed class DefaultKnowledgeFinalizerStageTests
{
    [Fact]
    public async Task ExecuteAsync_WithCandidatesState_AddsKnowledgeFragmentsToContext()
    {
        // Arrange
        var stage = new DefaultKnowledgeFinalizerStage();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        var entry1 = new VKKnowledgeEntry
        {
            Id = new VKKnowledgeId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Segment = new VKPromptSegment { Content = "Doc 1" }
        };
        var entry2 = new VKKnowledgeEntry
        {
            Id = new VKKnowledgeId(Guid.NewGuid()),
            TenantId = VKTenantId.Default,
            Segment = new VKPromptSegment { Content = "Doc 2" }
        };

        var state = new VKKnowledgeCandidatesState();
        state.Candidates.Add(entry1);
        state.Candidates.Add(entry2);
        context.SetState(state);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
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
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().BeEmpty();
    }
}
