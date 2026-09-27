using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Knowledge;

public sealed class DefaultKnowledgeFinalizerStageTests : VKUnitTestBase
{
    private DefaultKnowledgeFinalizerStage CreateStage(VKKnowledgeOptions? options = null)
    {
        return new DefaultKnowledgeFinalizerStage(
            options ?? new VKKnowledgeOptions(),
            GetMockObject<ILogger<DefaultKnowledgeFinalizerStage>>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCandidatesState_AddsKnowledgeSegmentsToContext()
    {
        // Arrange
        var stage = CreateStage();
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
        context.Segments.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutCandidatesState_DoesNotAddSegments()
    {
        // Arrange
        var stage = CreateStage();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenBudgetExceeded_TruncatesAndLogsDiagnostics()
    {
        // Arrange
        var options = new VKKnowledgeOptions { MaxEntriesToInject = 1 };
        var stage = CreateStage(options);
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        var entry1 = new VKKnowledgeEntryBuilder().WithContent("Doc 1").Build();
        var entry2 = new VKKnowledgeEntryBuilder().WithContent("Doc 2").Build();

        var state = new VKKnowledgeCandidatesState();
        state.Candidates.Add(entry1);
        state.Candidates.Add(entry2);
        context.SetState(state);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEntryTagNameOmitted_AppliesDefaultXmlTag()
    {
        // Arrange
        var stage = CreateStage();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        var entry = new VKKnowledgeEntryBuilder()
            .WithSegment(new VKPromptCoordinates { TagName = null }.ToSegment("Doc without tag"))
            .Build();

        var state = new VKKnowledgeCandidatesState();
        state.Candidates.Add(entry);
        context.SetState(state);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var segment = context.Segments.Should().ContainSingle().Subject;
        segment.Coordinates.TagName.Should().Be("knowledge");
        segment.Coordinates.Tier.Should().Be(VKPromptTierType.Knowledge);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEntryContentWhitespace_SkipsEmptySegment()
    {
        // Arrange
        var stage = CreateStage();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        var entry = new VKKnowledgeEntryBuilder()
            .WithContent("   ")
            .Build();

        var state = new VKKnowledgeCandidatesState();
        state.Candidates.Add(entry);
        context.SetState(state);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenEntryHasExplicitTagName_PreservesExplicitTagName()
    {
        // Arrange
        var stage = CreateStage();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        var entry = new VKKnowledgeEntryBuilder()
            .WithSegment(new VKPromptCoordinates { TagName = "custom_lore" }.ToSegment("Doc with custom tag"))
            .Build();

        var state = new VKKnowledgeCandidatesState();
        state.Candidates.Add(entry);
        context.SetState(state);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var segment = context.Segments.Should().ContainSingle().Subject;
        segment.Coordinates.TagName.Should().Be("custom_lore");
    }


    [Fact]
    public async Task ExecuteAsync_WhenBudgetNullOrDefault_InjectsAllCandidatesWithoutConstraint()
    {
        // Arrange - Default options has MaxEntriesToInject = null and ReservedTokens = null
        var stage = CreateStage();
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        var state = new VKKnowledgeCandidatesState();
        for (int i = 0; i < 10; i++)
        {
            state.Candidates.Add(new VKKnowledgeEntryBuilder()
                .WithContent($"Doc {i}")
                .Build());
        }
        context.SetState(state);

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().HaveCount(10);
    }
}
