using Microsoft.Extensions.DependencyInjection;
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
    [Fact]
    public async Task ExecuteAsync_WhenKeywordMatches_AddsKnowledgeFragment()
    {
        // Arrange
        var options = new VKKnowledgeOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Apples are delicious fruits.")
            .WithTriggerType(VKKnowledgeTriggerType.Keyword)
            .WithKey(new VKKnowledgeKey { Text = "apple", MatchType = VKKnowledgeMatchType.Contains, CaseSensitive = false })
            .Build();

        GetMock<IVKPsycheKnowledgeRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKKnowledgeId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>([entry]));

        var stage = new DefaultKnowledgeStage(
            options,
            GetMockObject<IVKPsycheKnowledgeRepository>(),
            GetMockObject<IVKKnowledgeRenderer>(),
            weavingOptions);

        var finalizer = new DefaultKnowledgeFinalizerStage();
        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(entry.Id)
            .WithUserInput("I really like to eat an apple every day!")
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);
        await finalizer.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge);

        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge).Subject;
        fragment.Metadata.Should().BeOfType<VKKnowledgeEntry>();

        var parsedEntry = (VKKnowledgeEntry)fragment.Metadata!;
        parsedEntry.Id.Should().Be(entry.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConstant_AddsKnowledgeFragment()
    {
        // Arrange
        var options = new VKKnowledgeOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();

        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("Constant lore.")
            .WithTriggerType(VKKnowledgeTriggerType.Constant)
            .Build();

        GetMock<IVKPsycheKnowledgeRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKKnowledgeId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>([entry]));

        var stage = new DefaultKnowledgeStage(
            options,
            GetMockObject<IVKPsycheKnowledgeRepository>(),
            GetMockObject<IVKKnowledgeRenderer>(),
            weavingOptions);

        var finalizer = new DefaultKnowledgeFinalizerStage();
        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(entry.Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);
        await finalizer.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Knowledge).Subject;
        var parsedEntry = (VKKnowledgeEntry)fragment.Metadata!;
        parsedEntry.Id.Should().Be(entry.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabledTiersContainsKnowledge_ReturnsSuccessEarly()
    {
        // Arrange
        var options = new VKKnowledgeOptions { Enabled = true };
        var weavingOptions = new VKWeavingOptions();
        var stage = new DefaultKnowledgeStage(
            options,
            GetMockObject<IVKPsycheKnowledgeRepository>(),
            GetMockObject<IVKKnowledgeRenderer>(),
            weavingOptions);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(new VKKnowledgeEntryBuilder().Build().Id)
            .WithRequestArgs(new VKWeavingArgs { DisabledTiers = [VKPromptTierType.Knowledge] })
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
        var weavingOptions = new VKWeavingOptions();
        var stage = new DefaultKnowledgeStage(
            options,
            GetMockObject<IVKPsycheKnowledgeRepository>(),
            GetMockObject<IVKKnowledgeRenderer>(),
            weavingOptions);

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
        var weavingOptions = new VKWeavingOptions();
        var stage = new DefaultKnowledgeStage(
            options,
            GetMockObject<IVKPsycheKnowledgeRepository>(),
            GetMockObject<IVKKnowledgeRenderer>(),
            weavingOptions);

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
        var weavingOptions = new VKWeavingOptions();
        var stage = new DefaultKnowledgeStage(
            options,
            GetMockObject<IVKPsycheKnowledgeRepository>(),
            GetMockObject<IVKKnowledgeRenderer>(),
            weavingOptions);

        var sessionThread = new VKSessionThreadBuilder()
            .WithKnowledgeState(new VKSessionKnowledgeState
            {
                LastTriggeredTurns = new Dictionary<VKKnowledgeId, int> { [entry.Id] = 1 }
            })
            .Build();
        sessionThread.IncrementTurn(DateTimeOffset.UtcNow);
        sessionThread.IncrementTurn(DateTimeOffset.UtcNow); // Turn 2, diff is 1 < 3

        var (context, _) = new VKPsycheRequestBuilder()
            .WithKnowledgeId(entry.Id)
            .WithUserInput("unrelated query without keyword")
            .BuildContext();
        context.SetState(sessionThread);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var candidateState = context.State<VKKnowledgeCandidatesState>();
        candidateState.Should().NotBeNull();
        candidateState!.Candidates.Should().ContainSingle(e => e.Id == entry.Id);
    }
}
