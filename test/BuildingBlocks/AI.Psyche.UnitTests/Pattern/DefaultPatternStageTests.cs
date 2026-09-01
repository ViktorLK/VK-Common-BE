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
        var stage = new DefaultPatternStage(options, GetMockObject<IVKPsychePatternRepository>(), new VKWeavingOptions());
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .WithPatternId(pattern.Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Pattern);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreFails_ReturnsFailure()
    {
        // Arrange
        GetMock<IVKPsychePatternRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKPatternId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<IReadOnlyList<VKPatternEntry>>(VKPatternErrors.NotFound));

        var options = new VKPatternOptions { Enabled = true };
        var stage = new DefaultPatternStage(options, GetMockObject<IVKPsychePatternRepository>(), new VKWeavingOptions());
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .WithPatternId(new VKPatternEntryBuilder().Build().Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKPatternErrors.NotFound);
    }
}
