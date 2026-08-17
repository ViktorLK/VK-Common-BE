using System.Collections.Generic;
using FluentAssertions;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Common;

public sealed class PromptPositionResolverTests
{
    [Fact]
    public void Resolve_WithAbsoluteDepth_ReturnsAbsoluteCoordinates()
    {
        // Arrange
        var segment = new VKPromptSegment
        {
            Role = VKChatRole.System,
            AbsoluteDepth = 1,
            DepthPriority = 10
        };
        var orders = new Dictionary<VKPromptTierType, int>();

        // Act
        var coord = PromptPositionResolver.Resolve(segment, orders);

        // Assert
        coord.Role.Should().Be(VKChatRole.System);
        coord.RenderOrder.Should().Be(10);
    }

    [Fact]
    public void Resolve_WithRelativeDepth_CalculatesRelativeOrder()
    {
        // Arrange
        var segment = new VKPromptSegment
        {
            Role = VKChatRole.User,
            RelativeDepth = VKPromptRelativeDepth.BeforeDirective,
            DepthPriority = 5
        };
        var orders = new Dictionary<VKPromptTierType, int>
        {
            [VKPromptTierType.Directive] = 10000
        };

        // Act
        var coord = PromptPositionResolver.Resolve(segment, orders);

        // Assert
        coord.Role.Should().Be(VKChatRole.System);
        coord.RenderOrder.Should().Be(10000 - PsycheConstants.Layout.RelativeOffset + 5);
    }
}
