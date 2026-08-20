using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

/// <summary>
/// Unit tests for the <see cref="DefaultDirectiveStage"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultDirectiveStageTests
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_AddsDirectiveFragment()
    {
        // Arrange
        var storeMock = new Mock<IVKDirectiveStore>();
        var loggerMock = new Mock<ILogger<DefaultDirectiveStage>>();

        var directiveId = new VKDirectiveId(Guid.NewGuid());
        var directive = new VKDirectiveCharter
        {
            Id = directiveId,
            Overview = "Test Safety Rulebook"
        };
        storeMock.Setup(s => s.GetDirectivesAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKDirectiveCharter>>([directive]));

        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(options, storeMock.Object, loggerMock.Object, new VKWeavingOptions());
        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("hello")
            .BuildContext();

        var request = context.Request with { DirectiveIds = [directiveId] };
        var testContext = new VKPsycheContext
        {
            Request = request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = context.Services
        };

        // Act
        var result = await stage.ExecuteAsync(testContext, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var fragment = testContext.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Directive).Subject;
        fragment.Metadata.Should().Be(directive);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabledTiersContainsDirective_ReturnsSuccessWithoutAddingFragment()
    {
        // Arrange
        var storeMock = new Mock<IVKDirectiveStore>();
        var loggerMock = new Mock<ILogger<DefaultDirectiveStage>>();
        var weavingOptions = new VKWeavingOptions { DisabledTiers = [VKPromptTierType.Directive] };
        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(options, storeMock.Object, loggerMock.Object, weavingOptions);
        var directiveId = new VKDirectiveId(Guid.NewGuid());
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();
        var request = context.Request with { DirectiveIds = [directiveId] };
        var testContext = new VKPsycheContext
        {
            Request = request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = context.Services
        };

        // Act
        var result = await stage.ExecuteAsync(testContext, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        testContext.Fragments.Should().NotContain(f => f.TierType == VKPromptTierType.Directive);
        storeMock.Verify(s => s.GetDirectivesAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreFails_ReturnsFailure()
    {
        // Arrange
        var storeMock = new Mock<IVKDirectiveStore>();
        var loggerMock = new Mock<ILogger<DefaultDirectiveStage>>();
        var directiveId = new VKDirectiveId(Guid.NewGuid());
        storeMock.Setup(s => s.GetDirectivesAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<IReadOnlyList<VKDirectiveCharter>>(VKDirectiveErrors.NotFound));

        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(options, storeMock.Object, loggerMock.Object, new VKWeavingOptions());
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();
        var request = context.Request with { DirectiveIds = [directiveId] };
        var testContext = new VKPsycheContext
        {
            Request = request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = context.Services
        };

        // Act
        var result = await stage.ExecuteAsync(testContext, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKDirectiveErrors.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDirectiveIdsEmpty_ReturnsSuccessWithoutCallingStore()
    {
        // Arrange
        var storeMock = new Mock<IVKDirectiveStore>();
        var loggerMock = new Mock<ILogger<DefaultDirectiveStage>>();
        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(options, storeMock.Object, loggerMock.Object, new VKWeavingOptions());
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        storeMock.Verify(s => s.GetDirectivesAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

