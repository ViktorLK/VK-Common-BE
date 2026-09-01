using Moq;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

public sealed class DefaultFragmentReplacementTaskTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WithVariables_RendersTemplateAndReplacesFragmentContent()
    {
        // Arrange
        GetMock<IVKPromptTemplateEngine>()
            .Setup(t => t.RenderAsync("Hello {{name}}", It.IsAny<IDictionary<string, object?>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success("Hello Alice"));

        var options = new VKWeavingOptions();
        var task = new DefaultFragmentReplacementTask(GetMockObject<IVKPromptTemplateEngine>(), options);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .WithRequestArgs(new VKWeavingArgs { Variables = new Dictionary<string, object?> { ["name"] = "Alice" } })
            .BuildContext();

        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = GetMockObject<IVKFragmentMetadata>(),
            Segment = new VKPromptSegment { Content = "Hello {{name}}" }
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().ContainSingle(f => f.Segment.Content == "Hello Alice");
    }

    [Fact]
    public async Task ExecuteAsync_WhenFragmentIsEcho_SkipsReplacement()
    {
        // Arrange
        var options = new VKWeavingOptions();
        var task = new DefaultFragmentReplacementTask(GetMockObject<IVKPromptTemplateEngine>(), options);

        var (context, _) = new VKPsycheRequestBuilder()
            .WithUserInput("test")
            .WithRequestArgs(new VKWeavingArgs { Variables = new Dictionary<string, object?> { ["user"] = "Bob" } })
            .BuildContext();

        context.AddFragment(new VKPromptFragment
        {
            TierType = VKPromptTierType.Echo,
            Metadata = GetMockObject<IVKFragmentMetadata>(),
            Segment = new VKPromptSegment { Content = "History {{user}}" }
        });

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IVKPromptTemplateEngine>()
            .Verify(t => t.RenderAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object?>?>(), It.IsAny<CancellationToken>()), Times.Never);
        context.Fragments.Should().ContainSingle(f => f.Segment.Content == "History {{user}}");
    }
}
