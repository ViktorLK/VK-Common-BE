using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.Pattern.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Pattern;

public sealed class DefaultPatternStageTests
{
    [Fact]
    public async Task ExecuteAsync_WithPatternsInStore_AddsPatternFragments()
    {
        // Arrange
        var storeMock = new Mock<IVKPatternStore>();
        var pattern = new VKPatternEntry
        {
            Id = new VKPatternId(Guid.NewGuid()),
            Segment = new VKPromptSegment { Content = "JSON Format Rule" }
        };
        storeMock.Setup(s => s.GetCurrentPatternsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IEnumerable<VKPatternEntry>>([pattern]));

        var options = new VKPatternOptions { Enabled = true };
        var stage = new DefaultPatternStage(options, storeMock.Object, new VKWeavingOptions());
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Pattern);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreFails_ReturnsFailure()
    {
        // Arrange
        var storeMock = new Mock<IVKPatternStore>();
        storeMock.Setup(s => s.GetCurrentPatternsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<IEnumerable<VKPatternEntry>>(VKPatternErrors.NotFound));

        var options = new VKPatternOptions { Enabled = true };
        var stage = new DefaultPatternStage(options, storeMock.Object, new VKWeavingOptions());
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKPatternErrors.NotFound);
    }
}
