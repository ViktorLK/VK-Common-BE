using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

/// <summary>
/// Unit tests for the <see cref="DefaultKnowledgeStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultKnowledgeStageTests : VKUnitTestBase
{
    private DefaultKnowledgeStage CreateStage(VKKnowledgeOptions? options = null)
    {
        return new DefaultKnowledgeStage(
            options ?? new VKKnowledgeOptions { Enabled = true },
            GetMockObject<IVKPsycheKnowledgeRepository>(),
            GetMockObject<ILogger<DefaultKnowledgeStage>>());
    }

    private DefaultKnowledgeFinalizerStage CreateFinalizer(VKKnowledgeOptions? options = null)
    {
        return new DefaultKnowledgeFinalizerStage(
            options ?? new VKKnowledgeOptions { Enabled = true },
            GetMockObject<ILogger<DefaultKnowledgeFinalizerStage>>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenKeywordMatches_AddsKnowledgeSegment()
    {
        // Arrange
        var options = new VKKnowledgeOptions { Enabled = true };

        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Apples are delicious fruits.")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "apple", MatchType = VKKnowledgeMatchType.Contains, CaseSensitive = false })
            .Build();

        GetMock<IVKPsycheKnowledgeRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKKnowledgeId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>([entry]));

        var stage = CreateStage(options);

        var finalizer = CreateFinalizer(options);
        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(entry.Id)
            .WithUserInput("I really like to eat an apple every day!")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);
        await finalizer.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var segment = context.Segments.Should().ContainSingle().Subject;
        segment.Content.Should().Be("Apples are delicious fruits.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenConstant_AddsKnowledgeSegment()
    {
        // Arrange
        var options = new VKKnowledgeOptions { Enabled = true };

        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Constant lore.")
            .WithTriggerType(VKKnowledgeTriggerType.Constant)
            .Build();

        GetMock<IVKPsycheKnowledgeRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKKnowledgeId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>([entry]));

        var stage = CreateStage(options);

        var finalizer = CreateFinalizer(options);
        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(entry.Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);
        await finalizer.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var segment = context.Segments.Should().ContainSingle().Subject;
        segment.Content.Should().Be("Constant lore.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabledInArgs_ReturnsSuccessEarly()
    {
        // Arrange
        var options = new VKKnowledgeOptions { Enabled = true };
        var stage = CreateStage(options);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(new VKKnowledgeEntryBuilder().Build().Id)
            .WithRequestArgs(new VKKnowledgeArgs { Enabled = false })
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IVKPsycheKnowledgeRepository>()
            .Verify(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKKnowledgeId>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenKnowledgeIdsEmpty_ReturnsSuccessEarly()
    {
        // Arrange
        var options = new VKKnowledgeOptions { Enabled = true };
        var stage = CreateStage(options);

        var (context, _) = new VKPsycheRequestBuilder().BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IVKPsycheKnowledgeRepository>()
            .Verify(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKKnowledgeId>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryReturnsFailure_ReturnsFailure()
    {
        // Arrange
        var entryId = new VKKnowledgeEntryBuilder().Build().Id;
        GetMock<IVKPsycheKnowledgeRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKKnowledgeId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<IReadOnlyList<VKKnowledgeEntry>>(VKKnowledgeErrors.NotFound));

        var options = new VKKnowledgeOptions { Enabled = true };
        var stage = CreateStage(options);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(entryId)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKKnowledgeErrors.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTurnRetentionActive_RetainsPreviouslyTriggeredEntries()
    {
        // Arrange
        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Retained lore")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "dragon", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        GetMock<IVKPsycheKnowledgeRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKKnowledgeId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>([entry]));

        var options = new VKKnowledgeOptions { Enabled = true, KeywordScanDepth = 3 };
        var stage = CreateStage(options);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(entry.Id)
            .WithUserInput("unrelated query without keyword")
            .BuildContext();

        context.AddEcho(new VKEchoFragment
        {
            Content = "I saw a fearsome dragon yesterday.",
            Role = VKChatRole.User,
            TurnIndex = 0,
            TokenCount = 10
        });

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var candidateState = context.State<VKKnowledgeCandidatesState>();
        candidateState.Should().NotBeNull();
        candidateState!.Candidates.Should().ContainSingle(e => e.Id == entry.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScanDepthIsOne_ScansOnlyLatestTurnAndIgnoresOlderTurns()
    {
        // Arrange
        var oldEntry = new VKKnowledgeEntryBuilder()
            .WithContent("Old dragon lore")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "dragon", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        var recentEntry = new VKKnowledgeEntryBuilder()
            .WithContent("Recent phoenix lore")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "phoenix", MatchType = VKKnowledgeMatchType.Contains })
            .Build();

        GetMock<IVKPsycheKnowledgeRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKKnowledgeId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>([oldEntry, recentEntry]));

        var options = new VKKnowledgeOptions { Enabled = true, KeywordScanDepth = 1 };
        var stage = CreateStage(options);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(oldEntry.Id)
            .WithKnowledgeId(recentEntry.Id)
            .WithUserInput("unrelated current question")
            .BuildContext();

        // Turn 1 (Old): User & Assistant talking about dragon
        context.AddEcho(new VKEchoFragment { Content = "I saw a dragon.", Role = VKChatRole.User, TurnIndex = 0 });
        context.AddEcho(new VKEchoFragment { Content = "Dragons are dangerous.", Role = VKChatRole.Assistant, TurnIndex = 1 });

        // Turn 2 (Recent): User & Assistant talking about phoenix
        context.AddEcho(new VKEchoFragment { Content = "Now tell me about phoenix.", Role = VKChatRole.User, TurnIndex = 2 });
        context.AddEcho(new VKEchoFragment { Content = "Phoenixes are mythical birds.", Role = VKChatRole.Assistant, TurnIndex = 3 });

        // Act (ScanDepth = 1 should only scan Turn 2, which has phoenix, but not Turn 1 which has dragon)
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var candidateState = context.State<VKKnowledgeCandidatesState>();
        candidateState.Should().NotBeNull();
        candidateState!.Candidates.Should().ContainSingle(e => e.Id == recentEntry.Id);
        candidateState.Candidates.Should().NotContain(e => e.Id == oldEntry.Id);
    }
}
