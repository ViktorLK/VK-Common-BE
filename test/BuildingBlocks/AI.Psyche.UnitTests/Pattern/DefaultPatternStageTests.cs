using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Pattern.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Pattern;

public sealed class DefaultPatternStageTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WithPatternsInStore_AddsPatternFragments()
    {
        // Arrange
        var pattern = new VKPatternEntryBuilder()
            .WithContent("JSON Format Rule")
            .Build();

        GetMock<IVKPsychePatternRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKPatternId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKPatternEntry>>([pattern]));

        var options = new VKPatternOptions { Enabled = true };
        var stage = new DefaultPatternStage(options, GetMockObject<IVKPsychePatternRepository>(), GetMockObject<ILogger<DefaultPatternStage>>());
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .WithPatternId(pattern.Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().ContainSingle(s => s.Tier == VKPromptTierType.Pattern);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreFails_ReturnsFailure()
    {
        // Arrange
        GetMock<IVKPsychePatternRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKPatternId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<IReadOnlyList<VKPatternEntry>>(VKPatternErrors.NotFound));

        var options = new VKPatternOptions { Enabled = true };
        var stage = new DefaultPatternStage(options, GetMockObject<IVKPsychePatternRepository>(), GetMockObject<ILogger<DefaultPatternStage>>());
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .WithPatternId(new VKPatternEntryBuilder().Build().Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKPatternErrors.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultiplePatterns_PreservesRequestOrderAndDeduplicates()
    {
        // Arrange
        var pattern1 = new VKPatternEntryBuilder()
            .WithContent("First Pattern")
            .Build();
        var pattern2 = new VKPatternEntryBuilder()
            .WithContent("Second Pattern")
            .Build();

        // Repository returns in inverted order with duplicate
        GetMock<IVKPsychePatternRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKPatternId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKPatternEntry>>([pattern2, pattern1, pattern2]));

        var options = new VKPatternOptions { Enabled = true };
        var stage = new DefaultPatternStage(options, GetMockObject<IVKPsychePatternRepository>(), GetMockObject<ILogger<DefaultPatternStage>>());
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .WithPatternIds([pattern1.Id, pattern2.Id, pattern2.Id])
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().HaveCount(2);
        context.Segments[0].Content.Should().Be("First Pattern");
        context.Segments[0].Tier.Should().Be(VKPromptTierType.Pattern);
        context.Segments[1].Content.Should().Be("Second Pattern");
        context.Segments[1].Tier.Should().Be(VKPromptTierType.Pattern);

        var state = context.State<IReadOnlyList<VKPatternEntry>>();
        state.Should().NotBeNull();
        state.Should().HaveCount(2);
        state![0].Id.Should().Be(pattern1.Id);
        state[1].Id.Should().Be(pattern2.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSegmentContentWhitespace_SkipsEmptySegment()
    {
        // Arrange
        var pattern = new VKPatternEntryBuilder()
            .WithContent("   ")
            .Build();

        GetMock<IVKPsychePatternRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKPatternId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKPatternEntry>>([pattern]));

        var options = new VKPatternOptions { Enabled = true };
        var stage = new DefaultPatternStage(options, GetMockObject<IVKPsychePatternRepository>(), GetMockObject<ILogger<DefaultPatternStage>>());
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .WithPatternId(pattern.Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().BeEmpty();
    }
}

