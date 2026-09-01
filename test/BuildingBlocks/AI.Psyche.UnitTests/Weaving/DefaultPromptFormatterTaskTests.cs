using Moq;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

/// <summary>
/// Unit tests for the <see cref="DefaultPromptFormatterTask"/> class.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public sealed class DefaultPromptFormatterTaskTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_FormatsFragmentsAndUpdatesContent()
    {
        // Arrange
        var mockFormatter = GetMock<IVKPromptFormatter>();
        mockFormatter.Setup(f => f.CanFormat(It.IsAny<VKPromptFragment>())).Returns(true);
        mockFormatter.Setup(f => f.Format(It.IsAny<VKPromptFragment>(), It.IsAny<VKPsycheContext>()))
            .Returns(VKResult.Success("Formatted Content"));

        var task = new DefaultPromptFormatterTask([mockFormatter.Object]);
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();

        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Initial Content" },
            RenderOrder = 0,
            Metadata = GetMockObject<IVKFragmentMetadata>()
        };
        context.AddFragment(fragment);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().ContainSingle();
        context.Fragments.First().Segment.Content.Should().Be("Formatted Content");
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFormatterAndNoContent_ReturnsFormatterNotFoundError()
    {
        // Arrange
        var mockFormatter = GetMock<IVKPromptFormatter>();
        mockFormatter.Setup(f => f.CanFormat(It.IsAny<VKPromptFragment>())).Returns(false);

        var task = new DefaultPromptFormatterTask([mockFormatter.Object]);
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();

        // Fragment with empty Content and no matching formatter
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = string.Empty },
            RenderOrder = 0,
            Metadata = GetMockObject<IVKFragmentMetadata>()
        };
        context.AddFragment(fragment);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.Should().BeFailure(VKWeavingErrors.FormatterNotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFormatterButHasRawContent_RetainsFragmentContent()
    {
        // Arrange
        var mockFormatter = GetMock<IVKPromptFormatter>();
        mockFormatter.Setup(f => f.CanFormat(It.IsAny<VKPromptFragment>())).Returns(false);

        var task = new DefaultPromptFormatterTask([mockFormatter.Object]);
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();

        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Existing Raw Text" },
            RenderOrder = 0,
            Metadata = GetMockObject<IVKFragmentMetadata>()
        };
        context.AddFragment(fragment);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().ContainSingle(f => f.Segment.Content == "Existing Raw Text");
    }

    [Fact]
    public async Task ExecuteAsync_WhenFormatterFails_ReturnsFailure()
    {
        // Arrange
        var mockFormatter = GetMock<IVKPromptFormatter>();
        mockFormatter.Setup(f => f.CanFormat(It.IsAny<VKPromptFragment>())).Returns(true);
        mockFormatter.Setup(f => f.Format(It.IsAny<VKPromptFragment>(), It.IsAny<VKPsycheContext>()))
            .Returns(VKResult.Failure<string>(new VKError("Format.Error", "Formatting failed")));

        var task = new DefaultPromptFormatterTask([mockFormatter.Object]);
        var (context, _) = new VKPsycheRequestBuilder().BuildContext();

        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Initial" },
            RenderOrder = 0,
            Metadata = GetMockObject<IVKFragmentMetadata>()
        };
        context.AddFragment(fragment);

        // Act
        var result = await task.ExecuteAsync(context);

        // Assert
        result.Should().BeFailure("Format.Error");
    }
}
