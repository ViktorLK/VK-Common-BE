using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

/// <summary>
/// Unit tests for the <see cref="DefaultPromptFormatterTask"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPromptFormatterTaskTests
{
    private static (VKPsycheContext Context, IServiceProvider Services) CreateTestContext(string userInput = "hello")
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var request = new VKPsycheRequest
        {
            PersonaIds = [new VKPersonaId(Guid.NewGuid())],
            SessionId = new VKSessionId(Guid.NewGuid()),
            UserInput = userInput
        };

        var context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = Guid.NewGuid().ToString(),
            CreatedAt = DateTimeOffset.UtcNow,
            Services = services
        };

        return (context, services);
    }

    [Fact]
    public async Task ExecuteAsync_HappyPath_FormatsFragmentsAndUpdatesContent()
    {
        // Arrange
        var mockFormatter = new Mock<IVKPromptFormatter>();
        mockFormatter.Setup(f => f.CanFormat(It.IsAny<VKPromptFragment>())).Returns(true);
        mockFormatter.Setup(f => f.Format(It.IsAny<VKPromptFragment>(), It.IsAny<VKPsycheContext>()))
            .Returns(VKResult.Success("Formatted Content"));

        var task = new DefaultPromptFormatterTask(new[] { mockFormatter.Object });
        var (context, _) = CreateTestContext();

        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Initial Content" },
            RenderOrder = 0,
            Metadata = new Mock<IVKFragmentMetadata>().Object
        };
        context.AddFragment(fragment);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().ContainSingle();
        context.Fragments.First().Segment.Content.Should().Be("Formatted Content");
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFormatterAndNoContent_ReturnsFormatterNotFoundError()
    {
        // Arrange
        var mockFormatter = new Mock<IVKPromptFormatter>();
        mockFormatter.Setup(f => f.CanFormat(It.IsAny<VKPromptFragment>())).Returns(false);

        var task = new DefaultPromptFormatterTask(new[] { mockFormatter.Object });
        var (context, _) = CreateTestContext();

        // Fragment with empty Content and no matching formatter
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = string.Empty },
            RenderOrder = 0,
            Metadata = new Mock<IVKFragmentMetadata>().Object
        };
        context.AddFragment(fragment);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Be(VKWeavingErrors.FormatterNotFound);
    }
}
