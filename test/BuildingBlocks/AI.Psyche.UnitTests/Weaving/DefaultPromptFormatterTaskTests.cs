using Moq;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

/// <summary>
/// Unit tests for the <see cref="DefaultPromptFormatterTask"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPromptFormatterTaskTests
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_FormatsFragmentsAndUpdatesContent()
    {
        // Arrange
        var mockFormatter = new Mock<IVKPromptFormatter>();
        mockFormatter.Setup(f => f.CanFormat(It.IsAny<VKPromptFragment>())).Returns(true);
        mockFormatter.Setup(f => f.Format(It.IsAny<VKPromptFragment>(), It.IsAny<VKWeavingContext>()))
            .Returns(VKResult.Success("Formatted Content"));

        var task = new DefaultPromptFormatterTask(new[] { mockFormatter.Object });

        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "hello",
            CorrelationId = "test-correlation"
        };

        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Role = VKChatRole.System,
            RenderOrder = 0,
            Metadata = new Mock<IVKFragmentMetadata>().Object
        };
        context.AddFragment(fragment);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Fragments.Should().ContainSingle();
        context.Fragments.First().Content.Should().Be("Formatted Content");
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFormatterAndNoContent_ReturnsFormatterNotFoundError()
    {
        // Arrange
        var mockFormatter = new Mock<IVKPromptFormatter>();
        mockFormatter.Setup(f => f.CanFormat(It.IsAny<VKPromptFragment>())).Returns(false);

        var task = new DefaultPromptFormatterTask(new[] { mockFormatter.Object });

        var context = new VKWeavingContext
        {
            TenantId = "test-tenant",
            PersonaId = "test-persona",
            SessionId = "test-session",
            UserInput = "hello",
            CorrelationId = "test-correlation"
        };

        // Fragment with no Content and no matching formatter
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Role = VKChatRole.System,
            RenderOrder = 0,
            Metadata = new Mock<IVKFragmentMetadata>().Object,
            Content = null
        };
        context.AddFragment(fragment);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Be(VKWeavingErrors.FormatterNotFound);
    }
}
