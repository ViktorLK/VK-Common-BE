using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Weaving;

public sealed class DefaultCoordinateResolveTaskTests
{
    [Fact]
    public async Task ExecuteAsync_AssignsRenderOrderToFragments()
    {
        // Arrange
        var options = new VKWeavingOptions();
        var task = new DefaultCoordinateResolveTask(options);

        var (context, _) = new VKPsycheRequestBuilder().WithUserInput("test").BuildContext();
        var metadataMock = new Mock<IVKFragmentMetadata>();
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Directive,
            Metadata = metadataMock.Object,
            Segment = new VKPromptSegment { Role = VKChatRole.System, Content = "Rule" }
        };
        context.AddFragment(fragment);

        // Act
        var result = await task.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        fragment.RenderOrder.Should().NotBeNull();
    }
}
