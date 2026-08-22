using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

public sealed class DefaultFragmentReplacementTaskTests
{
    [Fact]
    public async Task ExecuteAsync_WithVariables_RendersTemplateAndReplacesFragmentContent()
    {
        // Arrange
        var templateEngineMock = new Mock<IVKPromptTemplateEngine>();
        templateEngineMock.Setup(t => t.RenderAsync("Hello {{name}}", It.IsAny<IDictionary<string, object?>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success("Hello Alice"));

        var options = new VKWeavingOptions();
        var task = new DefaultFragmentReplacementTask(templateEngineMock.Object, options);

        var request = new VKPsycheRequest
        {
            PersonaIds = [new VKPersonaId(Guid.NewGuid())],
            UserInput = "test"
        }.WithArgs(new VKWeavingArgs { Variables = new Dictionary<string, object?> { ["name"] = "Alice" } });

        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = context.Services
        };

        var metadataMock = new Mock<IVKFragmentMetadata>();
        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = metadataMock.Object,
            Segment = new VKPromptSegment { Content = "Hello {{name}}" }
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().ContainSingle(f => f.Segment.Content == "Hello Alice");
    }

    [Fact]
    public async Task ExecuteAsync_WhenFragmentIsEcho_SkipsReplacement()
    {
        // Arrange
        var templateEngineMock = new Mock<IVKPromptTemplateEngine>();
        var options = new VKWeavingOptions();
        var task = new DefaultFragmentReplacementTask(templateEngineMock.Object, options);

        var request = new VKPsycheRequest
        {
            PersonaIds = [new VKPersonaId(Guid.NewGuid())],
            UserInput = "test"
        }.WithArgs(new VKWeavingArgs { Variables = new Dictionary<string, object?> { ["user"] = "Bob" } });

        var (context, _) = new VKPsycheRequestBuilder().BuildContext();
        context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = context.CorrelationId,
            CreatedAt = context.CreatedAt,
            Services = context.Services
        };

        var metadataMock = new Mock<IVKFragmentMetadata>();
        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            Metadata = metadataMock.Object,
            Segment = new VKPromptSegment { Content = "History {{user}}" }
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        templateEngineMock.Verify(t => t.RenderAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object?>?>(), It.IsAny<CancellationToken>()), Times.Never);
        context.Fragments.Should().ContainSingle(f => f.Segment.Content == "History {{user}}");
    }
}
