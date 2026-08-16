using System;
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

        var directiveId = VKDirectiveId.Empty;
        var directive = new VKDirectiveCharter
        {
            TenantId = VKTenantId.Default,
            Id = directiveId,
            Overview = "Test Safety Rulebook"
        };
        storeMock.Setup(s => s.GetDirectiveAsync(directiveId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(directive));

        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(options, storeMock.Object, loggerMock.Object, new VKWeavingOptions());
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var fragment = context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Directive).Subject;
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
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().NotContain(f => f.TierType == VKPromptTierType.Directive);
        storeMock.Verify(s => s.GetDirectiveAsync(It.IsAny<VKDirectiveId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreFails_ReturnsFailure()
    {
        // Arrange
        var storeMock = new Mock<IVKDirectiveStore>();
        var loggerMock = new Mock<ILogger<DefaultDirectiveStage>>();
        storeMock.Setup(s => s.GetDirectiveAsync(It.IsAny<VKDirectiveId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<VKDirectiveCharter>(VKDirectiveErrors.NotFound));

        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(options, storeMock.Object, loggerMock.Object, new VKWeavingOptions());
        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("hello").BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(VKDirectiveErrors.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomDirectiveIdInArgs_CallsStoreWithCustomId()
    {
        // Arrange
        var storeMock = new Mock<IVKDirectiveStore>();
        var loggerMock = new Mock<ILogger<DefaultDirectiveStage>>();
        var customId = new VKDirectiveId(Guid.NewGuid());
        var directive = new VKDirectiveCharter
        {
            TenantId = VKTenantId.Default,
            Id = customId,
            Overview = "Custom Directive"
        };
        storeMock.Setup(s => s.GetDirectiveAsync(customId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(directive));

        var options = new VKDirectiveOptions { Enabled = true };
        var stage = new DefaultDirectiveStage(options, storeMock.Object, loggerMock.Object, new VKWeavingOptions());
        
        var request = new VKPsycheRequest
        {
            TenantId = VKTenantId.Default,
            PersonaId = new VKPersonaId(Guid.NewGuid()),
            UserInput = "hello"
        }.WithArgs(new VKDirectiveArgs { DirectiveId = customId });

        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        context = new VKPsycheContext
        {
            Request = request,
            Services = context.Services
        };

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        storeMock.Verify(s => s.GetDirectiveAsync(customId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

