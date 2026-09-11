using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

/// <summary>
/// Unit tests for the <see cref="DefaultDirectiveStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultDirectiveStageTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_AddsDirectiveSegment()
    {
        // Arrange
        var directive = new VKDirectiveCharterBuilder()
            .WithOverview("Test Safety Rulebook")
            .Build();

        GetMock<IVKPsycheDirectiveRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKDirectiveCharter>>([directive]));

        GetMock<IVKDirectiveRenderer>()
            .Setup(r => r.Render(directive))
            .Returns("Rendered Directive Text");

        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(
            options,
            GetMockObject<IVKPsycheDirectiveRepository>(),
            GetMockObject<IVKDirectiveRenderer>(),
            GetMockObject<ILogger<DefaultDirectiveStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithDirectiveId(directive.Id)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        var segment = context.Segments.Should().ContainSingle(s => s.Tier == VKPromptTierType.Directive).Subject;
        segment.Content.Should().Be("Rendered Directive Text");
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ReturnsSuccessWithoutAddingSegment()
    {
        // Arrange
        var options = new VKDirectiveOptions { Enabled = false };
        var stage = new DefaultDirectiveStage(
            options,
            GetMockObject<IVKPsycheDirectiveRepository>(),
            GetMockObject<IVKDirectiveRenderer>(),
            GetMockObject<ILogger<DefaultDirectiveStage>>());

        var directiveId = new VKDirectiveCharterBuilder().Build().Id;
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithDirectiveId(directiveId)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Segments.Should().NotContain(s => s.Tier == VKPromptTierType.Directive);
        GetMock<IVKPsycheDirectiveRepository>()
            .Verify(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreFails_ReturnsFailure()
    {
        // Arrange
        var directiveId = new VKDirectiveCharterBuilder().Build().Id;
        GetMock<IVKPsycheDirectiveRepository>()
            .Setup(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<IReadOnlyList<VKDirectiveCharter>>(VKDirectiveErrors.NotFound));

        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(
            options,
            GetMockObject<IVKPsycheDirectiveRepository>(),
            GetMockObject<IVKDirectiveRenderer>(),
            GetMockObject<ILogger<DefaultDirectiveStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .WithDirectiveId(directiveId)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKDirectiveErrors.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDirectiveIdsEmpty_ReturnsSuccessWithoutCallingStore()
    {
        // Arrange
        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(
            options,
            GetMockObject<IVKPsycheDirectiveRepository>(),
            GetMockObject<IVKDirectiveRenderer>(),
            GetMockObject<ILogger<DefaultDirectiveStage>>());

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IVKPsycheDirectiveRepository>()
            .Verify(s => s.ListByIdsAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
